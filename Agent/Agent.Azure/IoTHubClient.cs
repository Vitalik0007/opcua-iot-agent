using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Text;

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

        public async void SendTelemetry(object telemetryData)
        {
            var json = JsonConvert.SerializeObject(telemetryData);
            var message = new Message(Encoding.UTF8.GetBytes(json));
            await _deviceClient.SendEventAsync(message);
            Console.WriteLine($"[Telemetry Sent] {json}");
        }

        public void Disconnect()
        {
            _deviceClient?.CloseAsync().Wait();
        }
    }
}