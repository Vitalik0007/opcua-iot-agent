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
        private DeviceClient _deviceClient;

        public IoTHubClient(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Connect()
        {
            _deviceClient = DeviceClient.CreateFromConnectionString(_connectionString, TransportType.Mqtt);
        }

        public async Task SendTelemetryAsync(object telemetryData)
        {
            var json = JsonConvert.SerializeObject(telemetryData);
            var message = new Message(Encoding.UTF8.GetBytes(json));
            await _deviceClient.SendEventAsync(message);
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
                var twin = await _deviceClient.GetTwinAsync();
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
            if (_deviceClient == null) return;

            var twinCollection = new TwinCollection(JsonConvert.SerializeObject(reportedData));
            await _deviceClient.UpdateReportedPropertiesAsync(twinCollection);
            Console.WriteLine("[Device Twin] Reported properties updated");
        }

        public async Task UpdateDesiredPropertyAsync(string propertyName, string value)
        {
            if (_deviceClient == null) return;

            object typedValue = value;

            if (int.TryParse(value, out int intValue))
                typedValue = intValue;
            else if (double.TryParse(value, out double doubleValue))
                typedValue = doubleValue;

            var desired = new TwinCollection();
            desired[propertyName] = typedValue;

            await _deviceClient.UpdateReportedPropertiesAsync(desired);
            Console.WriteLine($"[Device Twin] Desired property '{propertyName}' updated to '{typedValue}'");
        }

        public void Disconnect()
        {
            _deviceClient?.CloseAsync().Wait();
        }
    }
}