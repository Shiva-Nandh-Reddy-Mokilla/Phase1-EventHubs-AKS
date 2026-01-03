// ====================================================================================
// MESSAGE GENERATOR - Creates Random Test Messages
// ====================================================================================
// This utility class generates random messages for testing Event Hubs.
// It creates realistic sample data that you might see in a real application.
// ====================================================================================

namespace EventHubProducer;

public static class MessageGenerator
{
    // Random number generator (static = shared across all calls)
    private static readonly Random _random = new Random();
    
    // Sample data representing different types of events in a real system
    private static readonly string[] _sampleData = new[]
    {
        "Temperature reading from sensor A",
        "Order processed successfully",
        "User login event",
        "System health check",
        "Payment transaction completed",
        "Inventory update required",
        "Alert: high CPU usage",
        "File upload completed",
        "Email notification sent",
        "Database backup initiated"
    };

    // ====================================================================================
    // Generate Random Message
    // ====================================================================================
    // Creates a message with:
    // - Random MessageId (1-1000)
    // - Current timestamp
    // - Random sample data
    // - Random value (1-100)
    public static Message GenerateMessage()
    {
        var messageId = _random.Next(1, 1000);              // Random ID
        var payloadIndex = _random.Next(_sampleData.Length); // Pick random sample text
        var randomValue = _random.Next(1, 100);              // Random value

        return new Message
        {
            MessageId = messageId.ToString(),
            Timestamp = DateTime.UtcNow.ToString("O"),  // ISO 8601 format
            Payload = new
            {
                data = _sampleData[payloadIndex],
                value = randomValue,
                source = "producer-app"
            }
        };
    }
}
