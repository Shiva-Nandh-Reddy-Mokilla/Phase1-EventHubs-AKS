using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace EventHubProducer;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("================================================");
        Console.WriteLine("   Azure Event Hubs Producer");
        Console.WriteLine("================================================");
        Console.WriteLine();

        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration["EventHub:ConnectionString"] 
            ?? Environment.GetEnvironmentVariable("EVENTHUB_CONNECTION_STRING");
        
        var eventHubName = configuration["EventHub:EventHubName"] 
            ?? Environment.GetEnvironmentVariable("EVENTHUB_NAME");

        if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(eventHubName))
        {
            Console.WriteLine("ERROR: Missing Event Hub configuration");
            Console.WriteLine("Set EVENTHUB_CONNECTION_STRING and EVENTHUB_NAME");
            return;
        }

        await using var producerClient = new EventHubProducerClient(connectionString, eventHubName);

        Console.WriteLine("Connected to Event Hub: {0}", eventHubName);
        Console.WriteLine();

        int messageCount = 10;
        bool interactiveMode = true;

        if (args.Length > 0 && args[0] == "--count" && args.Length > 1)
        {
            if (int.TryParse(args[1], out int count))
            {
                messageCount = count;
                interactiveMode = false;
            }
        }

        if (interactiveMode)
        {
            await RunInteractiveMode(producerClient);
        }
        else
        {
            await SendBatchMessages(producerClient, messageCount);
        }

        Console.WriteLine();
        Console.WriteLine("Producer completed successfully");
    }

    static async Task RunInteractiveMode(EventHubProducerClient producerClient)
    {
        Console.WriteLine("========================================");
        Console.WriteLine("       INTERACTIVE MODE MENU");
        Console.WriteLine("========================================");
        Console.WriteLine("  1 - Send single random message");
        Console.WriteLine("  2 - Send batch of 10 messages");
        Console.WriteLine("  3 - Send custom message");
        Console.WriteLine("  q - Quit");
        Console.WriteLine();

        while (true)
        {
            Console.Write("Enter choice: ");
            var input = Console.ReadLine();

            switch (input?.ToLower())
            {
                case "1":
                    await SendSingleMessage(producerClient);
                    break;
                case "2":
                    await SendBatchMessages(producerClient, 10);
                    break;
                case "3":
                    await SendCustomMessage(producerClient);
                    break;
                case "q":
                    Console.WriteLine("Exiting...");
                    return;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }

            Console.WriteLine();
        }
    }

    static async Task SendSingleMessage(EventHubProducerClient producerClient)
    {
        var message = MessageGenerator.GenerateMessage();
        await SendMessage(producerClient, message);
    }

    static async Task SendBatchMessages(EventHubProducerClient producerClient, int count)
    {
        Console.WriteLine("Sending {0} messages...", count);
        
        for (int i = 0; i < count; i++)
        {
            var message = MessageGenerator.GenerateMessage();
            await SendMessage(producerClient, message);
            await Task.Delay(100);
        }

        Console.WriteLine("Successfully sent {0} messages", count);
    }

    static async Task SendCustomMessage(EventHubProducerClient producerClient)
    {
        Console.Write("Enter your message text: ");
        var payloadData = Console.ReadLine() ?? "custom data";

        var message = new Message
        {
            MessageId = Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow.ToString("O"),
            Payload = new { data = payloadData, custom = true }
        };

        await SendMessage(producerClient, message);
    }

    static async Task SendMessage(EventHubProducerClient producerClient, Message message)
    {
        try
        {
            using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();
            
            var jsonMessage = JsonSerializer.Serialize(message);
            var eventData = new EventData(jsonMessage);
            
            if (!eventBatch.TryAdd(eventData))
            {
                throw new Exception("Message too large for batch");
            }

            await producerClient.SendAsync(eventBatch);
            Console.WriteLine("Sent: MessageId={0}, Timestamp={1}", 
                message.MessageId, message.Timestamp);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error sending message: {0}", ex.Message);
        }
    }
}

public class Message
{
    public string MessageId { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public object Payload { get; set; } = new { };
}
