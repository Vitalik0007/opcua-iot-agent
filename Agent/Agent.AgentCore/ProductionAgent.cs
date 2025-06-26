using Agent.Azure;
using Agent.OpcUa;
using System.Timers;

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
            _telemetryTimer.Elapsed += SendTelemetry;
            _telemetryTimer.AutoReset = true;
        }

        public void Start()
        {
            _opcUa.Connect();
            _iot.Connect();
            _telemetryTimer.Start();
        }

        private void SendTelemetry(object sender, ElapsedEventArgs e)
        {
            try
            {
                var status = _opcUa.ReadNode("ns=2;s=ProductionStatus").Value;
                var goodCount = _opcUa.ReadNode("ns=2;s=GoodCount").Value;

                _iot.SendTelemetry(new
                {
                    productionStatus = status,
                    goodCount = goodCount
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Telemetry Error] {ex.Message}");
            }
        }

        public void Stop()
        {
            _telemetryTimer.Stop();
            _opcUa.Disconnect();
            _iot.Disconnect();
        }
    }
}