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
            Console.WriteLine("5. Direct Methods (placeholder)");
            Console.WriteLine("6. Logika biznesowa (placeholder)");
            Console.WriteLine("7. Zmień Desired property");
            Console.WriteLine("8. Zakończ program");
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
                    Console.WriteLine("[Direct Methods] Placeholder — tu będzie obsługa metod");
                    break;

                case "6":
                    Console.WriteLine("[Logika biznesowa] Placeholder — tu będą obliczenia");
                    break;

                case "7":
                    Console.Write("Podaj nazwę właściwości: ");
                    var propName = Console.ReadLine();
                    Console.Write("Podaj wartość: ");
                    var propValue = Console.ReadLine();

                    await iotClient.UpdateDesiredPropertyAsync(propName, propValue);
                    break;

                case "8":
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