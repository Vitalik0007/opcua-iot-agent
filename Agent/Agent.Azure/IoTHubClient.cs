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
            var json = JsonConvert.SerializeObject(telemetryData);
            var message = new Message(Encoding.UTF8.GetBytes(json));
            await DeviceClient.SendEventAsync(message);
            Console.WriteLine($"[Telemetry Sent] {json}");

            var reported = new
            {
                productionStatus = telemetryData.GetType().GetProperty("productionStatus")?.GetValue(telemetryData),
                goodCount = telemetryData.GetType().GetProperty("goodCount")?.GetValue(telemetryData),
                lastTelemetry = DateTime.UtcNow
            };

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

        public async Task UpdateReportedPropertiesAsync(object reportedData)
        {
            if (DeviceClient == null) return;

            var twinCollection = new TwinCollection(JsonConvert.SerializeObject(reportedData));
            await DeviceClient.UpdateReportedPropertiesAsync(twinCollection);
            Console.WriteLine("[Device Twin] Reported properties updated");
        }

        public async Task UpdateDesiredPropertyAsync(string propertyName, string value)
        {
            if (DeviceClient == null) return;

            object typedValue = value;

            if (int.TryParse(value, out int intValue))
                typedValue = intValue;
            else if (double.TryParse(value, out double doubleValue))
                typedValue = doubleValue;

            var desired = new TwinCollection();
            desired[propertyName] = typedValue;

            await DeviceClient.UpdateReportedPropertiesAsync(desired);
            Console.WriteLine($"[Device Twin] Desired property '{propertyName}' updated to '{typedValue}'");
        }

        public void Disconnect()
        {
            DeviceClient?.CloseAsync().Wait();
        }
    }
}