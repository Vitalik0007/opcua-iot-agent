using Microsoft.Extensions.Configuration;

namespace Agent.Configuration
{
    public class AppConfig
    {
        public string OpcUaEndpointUrl { get; }
        public string IoTHubConnectionString { get; }
        public string DeviceId { get; }

        public AppConfig()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            OpcUaEndpointUrl = config["OpcUa:EndpointUrl"];
            IoTHubConnectionString = config["IoTHub:ConnectionString"];
            DeviceId = config["General:DeviceId"];
        }
    }
}
