using System;
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
        Console.WriteLine("Agent is running.\n");

        bool running = true;
        while (running)
        {
            Console.WriteLine("\n=== MENU ===");
            Console.WriteLine("1. Wyślij telemetrię jednorazowo");
            Console.WriteLine("2. Uruchom auto-telemetrię (co 5 sek.)");
            Console.WriteLine("3. Zatrzymaj auto-telemetrię");
            Console.WriteLine("4. Device Twin (placeholder)");
            Console.WriteLine("5. Direct Methods (placeholder)");
            Console.WriteLine("6. Logika biznesowa (placeholder)");
            Console.WriteLine("7. Zakończ program");
            Console.Write("Wybierz opcję: ");

            var key = Console.ReadLine();

            switch (key)
            {
                case "1":
                    agent.SendTelemetryOnce();
                    break;

                case "2":
                    agent.StartTelemetryLoop();
                    break;

                case "3":
                    agent.StopTelemetryLoop();
                    break;

                case "4":
                    Console.WriteLine("[Device Twin] Placeholder — tu będzie obsługa Twin");
                    break;

                case "5":
                    Console.WriteLine("[Direct Methods] Placeholder — tu będzie obsługa metod");
                    break;

                case "6":
                    Console.WriteLine("[Logika biznesowa] Placeholder — tu będą obliczenia");
                    break;

                case "7":
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