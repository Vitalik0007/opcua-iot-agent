using Agent.AgentCore;
using Agent.Azure;
using Agent.Configuration;
using Agent.OpcUa;

class Program
{
    static void Main(string[] args)
    {
        var config = new AppConfig();
        var opcClient = new OpcUaClient(config.OpcUaEndpointUrl);
        var iotClient = new IoTHubClient(config.IoTHubConnectionString);
        var agent = new ProductionAgent(opcClient, iotClient);

        agent.Start();

        Console.WriteLine("Agent is running. Press Enter to exit...");
        Console.ReadLine();

        agent.Stop();
    }
}