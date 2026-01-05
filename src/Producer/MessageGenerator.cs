namespace EventHubProducer;

public static class MessageGenerator
{
    private static readonly Random _random = new Random();
    
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

    public static Message GenerateMessage()
    {
        var messageId = _random.Next(1, 1000);
        var payloadIndex = _random.Next(_sampleData.Length);
        var randomValue = _random.Next(1, 100);

        return new Message
        {
            MessageId = messageId.ToString(),
            Timestamp = DateTime.UtcNow.ToString("O"),
            Payload = new
            {
                data = _sampleData[payloadIndex],
                value = randomValue,
                source = "producer-app"
            }
        };
    }
}
