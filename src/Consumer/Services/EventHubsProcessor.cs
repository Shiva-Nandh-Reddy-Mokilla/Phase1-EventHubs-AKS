// ====================================================================================
// EVENT HUBS PROCESSOR - Background Service
// ====================================================================================
// This service runs continuously in the background, reading messages from Azure Event Hubs.
//
// KEY CONCEPTS:
// - Event Hubs: Azure's messaging service for event streaming
// - EventProcessorClient: Azure SDK component that reads messages from Event Hubs
// - Checkpointing: Remembering which messages we've already processed (using Blob Storage)
// - Background Service: A .NET service that runs continuously until the app stops
// ====================================================================================

using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using System.Text;
using System.Text.Json;

namespace EventHubConsumer.Services;

// This class inherits from BackgroundService, which means it runs in the background
public class EventHubsProcessor : BackgroundService
{
    // ====================================================================================
    // DEPENDENCIES (Injected by .NET Dependency Injection)
    // ====================================================================================
    private readonly ILogger<EventHubsProcessor> _logger;  // For logging messages to console
    private readonly IConfiguration _configuration;         // For reading configuration/secrets
    
    // Azure Event Hubs client that will process messages
    private EventProcessorClient? _processor;

    // Constructor: Called when the service is created
    public EventHubsProcessor(
        ILogger<EventHubsProcessor> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    // ====================================================================================
    // MAIN EXECUTION METHOD
    // ====================================================================================
    // This method runs when the background service starts.
    // It sets up the Event Hubs connection and keeps running until the app stops.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // ================================================================================
            // STEP 1: Load Configuration from Environment Variables
            // ================================================================================
            // In Kubernetes, these values come from ConfigMaps and Secrets
            var eventHubConnectionString = _configuration["EventHub:ConnectionString"] 
                ?? Environment.GetEnvironmentVariable("EVENTHUB_CONNECTION_STRING");
            
            var eventHubName = _configuration["EventHub:EventHubName"] 
                ?? Environment.GetEnvironmentVariable("EVENTHUB_NAME");
            
            // Consumer Group: Allows multiple consumers to read the same Event Hub independently
            var consumerGroup = _configuration["EventHub:ConsumerGroup"] ?? "$Default";
            
            // Blob Storage: Required for "checkpointing" (remembering which messages we've processed)
            var blobStorageConnectionString = Environment.GetEnvironmentVariable("STORAGE_CONNECTION_STRING")
                ?? _configuration["Storage:ConnectionString"];

            // ================================================================================
            // STEP 2: Validate Configuration
            // ================================================================================
            if (string.IsNullOrEmpty(eventHubConnectionString) || string.IsNullOrEmpty(eventHubName))
            {
                _logger.LogError("Event Hub connection string or name not configured.");
                _logger.LogError("Set EVENTHUB_CONNECTION_STRING and EVENTHUB_NAME environment variables.");
                return;
            }

            if (string.IsNullOrEmpty(blobStorageConnectionString))
            {
                _logger.LogError("Blob storage connection string not configured.");
                _logger.LogError("Set STORAGE_CONNECTION_STRING environment variable.");
                _logger.LogError("NOTE: Blob Storage is required for Event Hubs checkpointing.");
                return;
            }

            // ================================================================================
            // STEP 3: Initialize Blob Storage for Checkpointing
            // ================================================================================
            // Checkpointing: Event Hubs uses Blob Storage to remember which messages we've
            // already processed. This way, if the app restarts, we don't reprocess old messages.
            _logger.LogInformation("Initializing Blob Storage for checkpointing...");
            var blobContainerClient = new BlobContainerClient(
                blobStorageConnectionString, 
                "eventhub-checkpoints");  // Container name for storing checkpoint data
            
            // Create the container if it doesn't exist
            await blobContainerClient.CreateIfNotExistsAsync(cancellationToken: stoppingToken);
            _logger.LogInformation("Blob Storage container ready: eventhub-checkpoints");

            // ================================================================================
            // STEP 4: Create Event Processor Client
            // ================================================================================
            // The EventProcessorClient handles:
            // - Connecting to Event Hubs
            // - Reading messages from all partitions (Event Hubs splits data into partitions)
            // - Load balancing across multiple consumer instances
            // - Checkpointing progress
            _logger.LogInformation("Connecting to Event Hub...");
            _logger.LogInformation("Event Hub Name: {EventHubName}", eventHubName);
            _logger.LogInformation("Consumer Group: {ConsumerGroup}", consumerGroup);
            
            _processor = new EventProcessorClient(
                blobContainerClient,           // Where to store checkpoint data
                consumerGroup,                  // Consumer group name
                eventHubConnectionString,       // Connection string to Event Hubs
                eventHubName);                  // Name of the Event Hub to read from

            // ================================================================================
            // STEP 5: Register Event Handlers
            // ================================================================================
            // These methods will be called when:
            // - A message arrives (ProcessEventHandler)
            // - An error occurs (ProcessErrorHandler)
            _processor.ProcessEventAsync += ProcessEventHandler;
            _processor.ProcessErrorAsync += ProcessErrorHandler;

            // ================================================================================
            // STEP 6: Start Processing Messages
            // ================================================================================
            await _processor.StartProcessingAsync(stoppingToken);
            _logger.LogInformation("Event Hubs Processor started successfully");
            _logger.LogInformation("Listening for messages from Event Hub: {EventHubName}", eventHubName);
            _logger.LogInformation("");

            // Keep the service running until the application stops
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // This is expected when the application is shutting down
            _logger.LogInformation("Event Hubs Processor is shutting down...");
        }
        catch (Exception ex)
        {
            // Log any unexpected errors
            _logger.LogError(ex, "Fatal error in Event Hubs Processor");
            throw;
        }
        finally
        {
            // ================================================================================
            // CLEANUP: Stop Processing and Release Resources
            // ================================================================================
            if (_processor != null)
            {
                await _processor.StopProcessingAsync();
                _logger.LogInformation("Event Hubs Processor stopped gracefully");
            }
        }
    }

    // ====================================================================================
    // EVENT HANDLER: Process Incoming Messages
    // ====================================================================================
    // This method is called every time a message arrives from Event Hubs.
    private async Task ProcessEventHandler(ProcessEventArgs args)
    {
        try
        {
            // Skip empty messages
            if (args.Data == null || args.Data.EventBody.ToMemory().Length == 0)
            {
                return;
            }

            // ================================================================================
            // STEP 1: Extract the Message Body
            // ================================================================================
            // Convert the message from bytes to a string (JSON format)
            var messageBody = Encoding.UTF8.GetString(args.Data.EventBody.ToArray());
            
            // Parse the JSON to extract specific fields
            var message = JsonSerializer.Deserialize<JsonDocument>(messageBody);
            
            // Extract MessageId and Timestamp from the JSON
            var messageId = message?.RootElement.TryGetProperty("MessageId", out var idProp) == true 
                ? idProp.GetString() 
                : "unknown";
            
            var timestamp = message?.RootElement.TryGetProperty("Timestamp", out var tsProp) == true 
                ? tsProp.GetString() 
                : DateTime.UtcNow.ToString("O");

            // ================================================================================
            // STEP 2: Log the Received Message (THIS IS THE MAIN OUTPUT FOR PHASE 1)
            // ================================================================================
            _logger.LogInformation("========================================");
            _logger.LogInformation("RECEIVED MESSAGE from Event Hub");
            _logger.LogInformation("Message ID: {MessageId}", messageId);
            _logger.LogInformation("Timestamp:  {Timestamp}", timestamp);
            _logger.LogInformation("Body:       {MessageBody}", messageBody);
            _logger.LogInformation("========================================");
            _logger.LogInformation("");

            // ================================================================================
            // STEP 3: Update Checkpoint
            // ================================================================================
            // Tell Event Hubs that we've successfully processed this message.
            // This way, if the app restarts, we won't reprocess this message.
            await args.UpdateCheckpointAsync(args.CancellationToken);
        }
        catch (Exception ex)
        {
            // Log any errors that occur while processing
            _logger.LogError(ex, "Error processing Event Hub message");
        }
    }

    // ====================================================================================
    // ERROR HANDLER: Handle Processing Errors
    // ====================================================================================
    // This method is called when an error occurs in the Event Hubs connection or processing.
    private Task ProcessErrorHandler(Azure.Messaging.EventHubs.Processor.ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, 
            "Event Hub Error - Partition: {PartitionId}, Operation: {Operation}", 
            args.PartitionId, 
            args.Operation);
        return Task.CompletedTask;
    }
}
