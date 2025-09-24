using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Azure
{
    public class IoTHubClient
    {
        private readonly string _connectionString;
        public DeviceClient DeviceClient { get; private set; }

        public IoTHubClient(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Connect()
        {
            DeviceClient = DeviceClient.CreateFromConnectionString(_connectionString, TransportType.Mqtt);
        }

        public async Task SendTelemetryAsync(object telemetryData)
        {
            if (DeviceClient == null) return;

            var json = JsonConvert.SerializeObject(telemetryData);
            var message = new Message(Encoding.UTF8.GetBytes(json));
            await DeviceClient.SendEventAsync(message);
            Console.WriteLine($"[Telemetry Sent] {json}");

            var reported = new TwinCollection();
            foreach (var prop in telemetryData.GetType().GetProperties())
            {
                reported[prop.Name] = prop.GetValue(telemetryData);
            }
            reported["lastTelemetry"] = DateTime.UtcNow;

            await UpdateReportedPropertiesAsync(reported);
        }

        public async Task ShowDeviceTwinAsync()
        {
            try
            {
                var twin = await DeviceClient.GetTwinAsync();
                Console.WriteLine("\n=== DEVICE TWIN ===");
                Console.WriteLine(twin.ToJson());
                Console.WriteLine("===================\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeviceTwin Error] {ex.Message}");
            }
        }

        public async Task UpdateReportedPropertiesAsync(TwinCollection reportedData)
        {
            if (DeviceClient == null) return;

            await DeviceClient.UpdateReportedPropertiesAsync(reportedData);
            Console.WriteLine("[Device Twin] Reported properties updated");
        }

        public async Task UpdateReportedPropertyAsync(string propertyName, object value)
        {
            if (DeviceClient == null) return;

            var desired = new TwinCollection();
            desired[propertyName] = value;

            await DeviceClient.UpdateReportedPropertiesAsync(desired);
            Console.WriteLine($"[Device Twin] Desired property '{propertyName}' updated to '{value}'");
        }

        public void Disconnect()
        {
            DeviceClient?.CloseAsync().Wait();
        }
    }
}