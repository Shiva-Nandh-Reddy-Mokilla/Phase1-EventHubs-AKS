// Producer - sends messages to Azure Event Hubs
// Run with: dotnet run

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
        Console.WriteLine("   PHASE 1: Azure Event Hubs Producer");
        Console.WriteLine("================================================");
        Console.WriteLine();

        // ================================================================================
        // STEP 1: Load Configuration
        // ================================================================================
        // Configuration can come from:
        // 1. appsettings.json file
        // 2. Environment variables (EVENTHUB_CONNECTION_STRING, EVENTHUB_NAME)
        
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()  // Environment variables override file settings
            .Build();

        // Get Event Hub connection details from configuration
        var connectionString = configuration["EventHub:ConnectionString"] 
            ?? Environment.GetEnvironmentVariable("EVENTHUB_CONNECTION_STRING");
        
        var eventHubName = configuration["EventHub:EventHubName"] 
            ?? Environment.GetEnvironmentVariable("EVENTHUB_NAME");

        // Check if we have connection info
        if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(eventHubName))
        {
            Console.WriteLine("ERROR: Missing Event Hub configuration");
            Console.WriteLine();
            Console.WriteLine("Please set the following environment variables:");
            Console.WriteLine("  - EVENTHUB_CONNECTION_STRING");
            Console.WriteLine("  - EVENTHUB_NAME");
            Console.WriteLine();
            Console.WriteLine("Or configure them in appsettings.json");
            return;
        }

        // Connect to Event Hubs
        await using var producerClient = new EventHubProducerClient(connectionString, eventHubName);

        Console.WriteLine("Connected to Event Hub: {0}", eventHubName);
        Console.WriteLine();

        // Check command line args
        
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

        // Run the producer
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

    // Interactive mode - show menu
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
                    // Send a single random message
                    await SendSingleMessage(producerClient);
                    break;
                case "2":
                    // Send 10 random messages
                    await SendBatchMessages(producerClient, 10);
                    break;
                case "3":
                    // Send a custom message with user input
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

    // ====================================================================================
    // SEND SINGLE MESSAGE
    // ====================================================================================
    static async Task SendSingleMessage(EventHubProducerClient producerClient)
    {
        var message = MessageGenerator.GenerateMessage();
        await SendMessage(producerClient, message);
    }

    // ====================================================================================
    // SEND BATCH OF MESSAGES
    // ====================================================================================
    static async Task SendBatchMessages(EventHubProducerClient producerClient, int count)
    {
        Console.WriteLine("Sending {0} messages...", count);
        
        for (int i = 0; i < count; i++)
        {
            var message = MessageGenerator.GenerateMessage();
            await SendMessage(producerClient, message);
            
            // Small delay to make the output readable and avoid overwhelming the system
            await Task.Delay(100);
        }

        Console.WriteLine("Successfully sent {0} messages", count);
    }

    // ====================================================================================
    // SEND CUSTOM MESSAGE
    // ====================================================================================
    static async Task SendCustomMessage(EventHubProducerClient producerClient)
    {
        Console.Write("Enter your message text: ");
        var payloadData = Console.ReadLine() ?? "custom data";

        // Create a custom message with user input
        var message = new Message
        {
            MessageId = Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow.ToString("O"),  // "O" = ISO 8601 format
            Payload = new { data = payloadData, custom = true }
        };

        await SendMessage(producerClient, message);
    }

    // ====================================================================================
    // CORE METHOD: Send Message to Event Hubs
    // ====================================================================================
    // This method takes a Message object and sends it to Azure Event Hubs
    static async Task SendMessage(EventHubProducerClient producerClient, Message message)
    {
        try
        {
            // ================================================================================
            // STEP 1: Create Event Data Batch
            // ================================================================================
            // Event Hubs requires messages to be sent in batches (even for single messages)
            // This ensures efficient transmission and respects size limits
            using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();
            
            // ================================================================================
            // STEP 2: Serialize Message to JSON
            // ================================================================================
            var jsonMessage = JsonSerializer.Serialize(message);
            var eventData = new EventData(jsonMessage);
            
            // ================================================================================
            // STEP 3: Add Message to Batch
            // ================================================================================
            if (!eventBatch.TryAdd(eventData))
            {
                // This happens if the message is too large
                throw new Exception("Event is too large for the batch");
            }

            // ================================================================================
            // STEP 4: Send the Batch to Event Hubs
            // ================================================================================
            await producerClient.SendAsync(eventBatch);

            // ================================================================================
            // STEP 5: Log Success
            // ================================================================================
            Console.WriteLine("Sent: MessageId={0}, Timestamp={1}", 
                message.MessageId, message.Timestamp);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error sending message: {0}", ex.Message);
        }
    }
}

// ====================================================================================
// MESSAGE MODEL
// ====================================================================================
// This class defines the structure of messages sent to Event Hubs
public class Message
{
    public string MessageId { get; set; } = string.Empty;    // Unique identifier
    public string Timestamp { get; set; } = string.Empty;    // When the message was created
    public object Payload { get; set; } = new { };           // The actual message data
}
