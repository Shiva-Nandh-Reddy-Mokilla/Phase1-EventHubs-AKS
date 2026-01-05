// Consumer - reads messages from Azure Event Hubs
// Runs in Kubernetes and logs messages to console

using Azure.Messaging.EventHubs.Consumer;
using Microsoft.Extensions.Configuration;
using System.Text;

Console.WriteLine("================================================");
Console.WriteLine("   PHASE 1: Azure Event Hubs Consumer");
Console.WriteLine("================================================");
Console.WriteLine();

// ================================================================================
// STEP 1: Load Configuration
// ================================================================================
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var connectionString = configuration["EventHub:ConnectionString"]
    ?? Environment.GetEnvironmentVariable("EVENTHUB_CONNECTION_STRING");

var eventHubName = configuration["EventHub:EventHubName"]
    ?? Environment.GetEnvironmentVariable("EVENTHUB_NAME");

var consumerGroup = configuration["EventHub:ConsumerGroup"] ?? "$Default";

// ================================================================================
// STEP 2: Validate Configuration
// ================================================================================
if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(eventHubName))
{
    Console.WriteLine("ERROR: Missing Event Hub configuration");
    Console.WriteLine();
    Console.WriteLine("Please set:");
    Console.WriteLine("  - EVENTHUB_CONNECTION_STRING");
    Console.WriteLine("  - EVENTHUB_NAME");
    Console.WriteLine();
    Console.WriteLine("Or configure in appsettings.json");
    return;
}

Console.WriteLine("Configuration:");
Console.WriteLine($"  Event Hub: {eventHubName}");
Console.WriteLine($"  Consumer Group: {consumerGroup}");
Console.WriteLine();

// ================================================================================
// STEP 3: Create Event Hub Consumer Client
// ================================================================================
// EventHubConsumerClient is a simple client that reads from Event Hubs
// No checkpointing needed for Phase 1
await using var consumer = new EventHubConsumerClient(
    consumerGroup,
    connectionString,
    eventHubName);

Console.WriteLine("Connected to Event Hub successfully");
Console.WriteLine("Listening for messages... (Press Ctrl+C to stop)");
Console.WriteLine();

// ================================================================================
// STEP 4: Read Messages Continuously
// ================================================================================
try
{
    // Get all partition IDs
    string[] partitionIds = await consumer.GetPartitionIdsAsync();
    Console.WriteLine($"Event Hub has {partitionIds.Length} partition(s)");
    Console.WriteLine();

    // Create cancellation token for graceful shutdown
    var cancellationSource = new CancellationTokenSource();
    Console.CancelKeyPress += (sender, args) =>
    {
        args.Cancel = true;
        cancellationSource.Cancel();
        Console.WriteLine();
        Console.WriteLine("Shutting down...");
    };

    // Read from all partitions concurrently
    var readTasks = partitionIds.Select(partitionId =>
        ReadPartitionAsync(consumer, partitionId, cancellationSource.Token));

    await Task.WhenAll(readTasks);
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR: {ex.Message}");
}

Console.WriteLine("Consumer stopped");

// ====================================================================================
// METHOD: Read Messages from a Partition
// ====================================================================================
static async Task ReadPartitionAsync(
    EventHubConsumerClient consumer,
    string partitionId,
    CancellationToken cancellationToken)
{
    try
    {
        // ReadEventsFromPartitionAsync reads from the latest available messages
        // For Phase 1, we start from the latest to see new messages as they arrive
        await foreach (var partitionEvent in consumer.ReadEventsFromPartitionAsync(
            partitionId,
            Azure.Messaging.EventHubs.Consumer.EventPosition.Latest,
            cancellationToken))
        {
            if (partitionEvent.Data == null)
                continue;

            // Extract message body
            var messageBody = Encoding.UTF8.GetString(partitionEvent.Data.EventBody.ToArray());

            // Parse message properties (if it's JSON)
            string messageId = "N/A";
            string timestamp = "N/A";

            try
            {
                var json = System.Text.Json.JsonDocument.Parse(messageBody);
                if (json.RootElement.TryGetProperty("MessageId", out var idProp))
                    messageId = idProp.GetString() ?? "N/A";
                if (json.RootElement.TryGetProperty("Timestamp", out var tsProp))
                    timestamp = tsProp.GetString() ?? "N/A";
            }
            catch
            {
                // If not JSON or parsing fails, just log the raw body
            }

            // ================================================================================
            // Log the Received Message
            // ================================================================================
            Console.WriteLine("========================================");
            Console.WriteLine("RECEIVED MESSAGE");
            Console.WriteLine($"  Partition:   {partitionId}");
            Console.WriteLine($"  Message ID:  {messageId}");
            Console.WriteLine($"  Timestamp:   {timestamp}");
            Console.WriteLine($"  Body:        {messageBody}");
            Console.WriteLine("========================================");
            Console.WriteLine();
        }
    }
    catch (TaskCanceledException)
    {
        // Expected when Ctrl+C is pressed
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR reading from partition {partitionId}: {ex.Message}");
    }
}
