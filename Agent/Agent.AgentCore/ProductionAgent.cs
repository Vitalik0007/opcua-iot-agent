using Agent.Azure;
using Agent.OpcUa;
using System;
using System.Timers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;

namespace Agent.AgentCore
{
    public class ProductionAgent
    {
        private readonly OpcUaClient _opcUa;
        private readonly IoTHubClient _iot;
        private readonly System.Timers.Timer _telemetryTimer;

        private readonly int _errorWindowSeconds;
        private readonly System.Collections.Generic.Queue<(DateTime timestamp, int errorFlags)> _recentErrors;

        public ProductionAgent(OpcUaClient opcUa, IoTHubClient iot, int errorWindowSeconds = 60)
        {
            _opcUa = opcUa;
            _iot = iot;
            _errorWindowSeconds = errorWindowSeconds;
            _recentErrors = new System.Collections.Generic.Queue<(DateTime, int)>();

            _telemetryTimer = new System.Timers.Timer(5000);
            _telemetryTimer.Elapsed += async (s, e) => await SendTelemetryOnceAsync();
            _telemetryTimer.AutoReset = true;
        }

        public void Start()
        {
            _opcUa.Connect();
            _iot.Connect();
            Console.WriteLine("[Agent] Connected to OPC UA and IoT Hub.");

            RegisterDirectMethods();
        }

        public async Task SendTelemetryOnceAsync()
        {
            try
            {
                var status = _opcUa.ReadNode("ns=2;s=Device 2/ProductionStatus").Value;
                var goodCount = Convert.ToDouble(_opcUa.ReadNode("ns=2;s=Device 2/GoodCount").Value);
                var badCount = Convert.ToDouble(_opcUa.ReadNode("ns=2;s=Device 2/BadCount").Value);
                var temperature = Convert.ToDouble(_opcUa.ReadNode("ns=2;s=Device 2/Temperature").Value);
                var errorFlags = Convert.ToInt32(_opcUa.ReadNode("ns=2;s=Device 2/DeviceErrors").Value);

                if (temperature > 80)
                {
                    Console.WriteLine("[BusinessLogic] High temperature! Activating Emergency Stop...");
                    _opcUa.InvokeMethod("ns=2;s=Device 2", "ns=2;s=Device 2/EmergencyStop");
                }

                double totalProduced = goodCount + badCount;
                double goodRate = totalProduced > 0 ? (goodCount / totalProduced) * 100.0 : 100.0;
                if (goodRate < 90)
                {
                    Console.WriteLine("[BusinessLogic] Good production rate < 90%, reducing Desired Production Rate by 10%");
                    var twin = await _iot.DeviceClient.GetTwinAsync();
                    double currentDesired = twin.Properties.Desired.Contains("ProductionRate") ?
                        Convert.ToDouble(twin.Properties.Desired["ProductionRate"]) : 100;
                    var newDesired = Math.Max(0, currentDesired - 10);
                    await _iot.UpdateReportedPropertyAsync("ProductionRate", newDesired.ToString());
                }

                DateTime now = DateTime.UtcNow;
                if (errorFlags != 0)
                {
                    _recentErrors.Enqueue((now, errorFlags));
                    Console.WriteLine($"[BusinessLogic] Device Error detected: {errorFlags}. (Email placeholder)");
                }

                while (_recentErrors.Count > 0 && (now - _recentErrors.Peek().timestamp).TotalSeconds > _errorWindowSeconds)
                    _recentErrors.Dequeue();

                int recentErrorCount = 0;
                foreach (var e in _recentErrors)
                    if (e.errorFlags != 0) recentErrorCount++;

                if (recentErrorCount > 3)
                {
                    Console.WriteLine("[BusinessLogic] More than 3 errors detected in window! Triggering Emergency Stop...");
                    _opcUa.InvokeMethod("ns=2;s=Device 2", "ns=2;s=Device 2/EmergencyStop");
                    _recentErrors.Clear();
                }

                await _iot.SendTelemetryAsync(new
                {
                    productionStatus = status,
                    goodCount,
                    badCount,
                    temperature,
                    timestamp = now
                });

                Console.WriteLine("[Telemetry] Sent once.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Telemetry Error] {ex.Message}");
            }
        }

        public void StartTelemetryLoop()
        {
            _telemetryTimer.Start();
            Console.WriteLine("[Telemetry] Auto sending every 5s started.");
        }

        public void StopTelemetryLoop()
        {
            _telemetryTimer.Stop();
            Console.WriteLine("[Telemetry] Auto sending stopped.");
        }

        public void Stop()
        {
            _telemetryTimer.Stop();
            _opcUa.Disconnect();
            _iot.Disconnect();
            Console.WriteLine("[Agent] Disconnected.");
        }

        #region Direct Methods
        private void RegisterDirectMethods()
        {
            _iot.DeviceClient.SetMethodHandlerAsync("StartProduction", StartProductionMethod, null);
            _iot.DeviceClient.SetMethodHandlerAsync("StopProduction", StopProductionMethod, null);
            _iot.DeviceClient.SetMethodHandlerAsync("EmergencyStop", EmergencyStopMethod, null);
            _iot.DeviceClient.SetMethodHandlerAsync("ResetErrorStatus", ResetErrorStatusMethod, null);
        }

        private Task<MethodResponse> StartProductionMethod(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine("[Direct Method] StartProduction invoked");
            StartTelemetryLoop();
            string result = "{\"status\":\"Production started\"}";
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }

        private Task<MethodResponse> StopProductionMethod(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine("[Direct Method] StopProduction invoked");
            StopTelemetryLoop();
            string result = "{\"status\":\"Production stopped\"}";
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }

        private Task<MethodResponse> EmergencyStopMethod(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine("[Direct Method] EmergencyStop invoked");
            _opcUa.InvokeMethod("ns=2;s=Device 2", "ns=2;s=Device 2/EmergencyStop");
            string result = "{\"status\":\"Emergency Stop triggered\"}";
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }

        private Task<MethodResponse> ResetErrorStatusMethod(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine("[Direct Method] ResetErrorStatus invoked");

            try
            {
                _opcUa.InvokeMethod("ns=2;s=Device 2", "ns=2;s=Device 2/ResetErrorStatus");
                string result = "{\"status\":\"Device error status reset\"}";
                return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Direct Method Error] {ex.Message}");
                string result = "{\"status\":\"Failed to reset device error status\"}";
                return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 500));
            }
        }

        #endregion
    }
}