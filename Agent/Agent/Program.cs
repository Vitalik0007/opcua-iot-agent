using System;
using System.Threading.Tasks;
using Agent.AgentCore;
using Agent.Azure;
using Agent.Configuration;
using Agent.OpcUa;

class Program
{
    static async Task Main(string[] args)
    {
        var config = new AppConfig();
        var opcClient = new OpcUaClient(config.OpcUaEndpointUrl);
        var iotClient = new IoTHubClient(config.IoTHubConnectionString);
        var agent = new ProductionAgent(opcClient, iotClient);

        agent.Start();
        Console.WriteLine("Agent is running.\n");

        bool running = true;
        while (running)
        {
            Console.WriteLine("\n=== MENU ===");
            Console.WriteLine("1. Wyślij telemetrię jednorazowo");
            Console.WriteLine("2. Uruchom auto-telemetrię (co 5 sek.)");
            Console.WriteLine("3. Zatrzymaj auto-telemetrię");
            Console.WriteLine("4. Device Twin");
            Console.WriteLine("5. Direct Methods (Start/Stop Production)");
            Console.WriteLine("7. Zakończ program");
            Console.Write("Wybierz opcję: ");

            var key = Console.ReadLine();

            switch (key)
            {
                case "1":
                    await agent.SendTelemetryOnceAsync();
                    break;

                case "2":
                    agent.StartTelemetryLoop();
                    break;

                case "3":
                    agent.StopTelemetryLoop();
                    break;

                case "4":
                    await iotClient.ShowDeviceTwinAsync();
                    break;

                case "5":
                    Console.WriteLine("\n=== Direct Methods Menu ===");
                    Console.WriteLine("1. StartProduction");
                    Console.WriteLine("2. StopProduction");
                    Console.Write("Wybierz metodę: ");
                    var methodKey = Console.ReadLine();

                    string result;

                    switch (methodKey)
                    {
                        case "1":
                            agent.StartTelemetryLoop();
                            result = "StartProduction invoked locally";
                            break;
                        case "2":
                            agent.StopTelemetryLoop();
                            result = "StopProduction invoked locally";
                            break;
                        default:
                            result = "Nieprawidłowa metoda";
                            break;
                    }

                    Console.WriteLine($"[Direct Method Result] {result}");
                    break;

                case "6":
                    running = false;
                    break;

                default:
                    Console.WriteLine("Nieprawidłowy wybór!");
                    break;
            }
        }

        agent.Stop();
        Console.WriteLine("Agent stopped.");
    }
}