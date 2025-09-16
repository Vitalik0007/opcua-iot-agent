using Agent.Azure;
using Agent.OpcUa;
using System;
using System.Timers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;

namespace Agent.AgentCore
{
    public class ProductionAgent
    {
        private readonly OpcUaClient _opcUa;
        private readonly IoTHubClient _iot;
        private readonly System.Timers.Timer _telemetryTimer;

        public ProductionAgent(OpcUaClient opcUa, IoTHubClient iot)
        {
            _opcUa = opcUa;
            _iot = iot;

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
                var status = _opcUa.ReadNode("ns=2;s=ProductionStatus").Value;
                var goodCount = _opcUa.ReadNode("ns=2;s=GoodCount").Value;

                await _iot.SendTelemetryAsync(new
                {
                    productionStatus = status,
                    goodCount = goodCount,
                    timestamp = DateTime.UtcNow
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
            _iot.DeviceClient.SetMethodHandlerAsync("ResetCounters", ResetCountersMethod, null);
            _iot.DeviceClient.SetMethodHandlerAsync("StopProduction", StopProductionMethod, null);
        }

        private Task<MethodResponse> ResetCountersMethod(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine("[Direct Method] ResetCounters invoked");

            string result = "{\"status\":\"Counters reset\"}";
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }

        private Task<MethodResponse> StopProductionMethod(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine("[Direct Method] StopProduction invoked");
            StopTelemetryLoop();

            string result = "{\"status\":\"Production stopped\"}";
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }
        public async Task<string> ResetCountersAsync()
        {
            Console.WriteLine("[Direct Method] ResetCounters invoked from menu");

            return await Task.FromResult("Counters reset successfully");
        }

        public async Task<string> StopProductionAsync()
        {
            Console.WriteLine("[Direct Method] StopProduction invoked from menu");
            StopTelemetryLoop();
            return await Task.FromResult("Production stopped successfully");
        }

        #endregion
    }
}