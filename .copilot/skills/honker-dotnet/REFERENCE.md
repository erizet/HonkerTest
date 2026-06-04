# Honker .NET API Reference

This reference covers the primary use cases for the `Honker` .NET binding.

## 1. Streams (Durable Pub/Sub)

Streams keep an ordered log of events. Consumers save their offset and resume on restart.

### Publishing
```csharp
var stream = db.Stream("sensors");

// Publish an anonymous object or strong type
long offset = stream.Publish(new SensorReading { Temp = 22.5 });
```

### Subscribing
```csharp
// "worker-1" is the consumer name. Honker tracks its offset automatically.
await foreach (var evt in stream.Subscribe("worker-1", cancellationToken: ct))
{
    // Access raw JSON string
    string json = evt.PayloadRaw;

    // Or deserialize to a type (case-sensitive by default)
    var reading = evt.GetPayload<SensorReading>();
    
    Console.WriteLine($"Received offset {evt.Offset}");
}
```

## 2. Queues (Task Workers)

Queues are for at-least-once task execution (claim, process, acknowledge).

### Enqueueing
```csharp
var queue = db.Queue("emails");
queue.Enqueue(new EmailJob { To = "alice@example.com" });
```

### Consuming
```csharp
// "worker-1" is the unique name of this consumer process/thread
await foreach (var job in queue.Claim("worker-1", cancellationToken: ct))
{
    try 
    {
        var email = job.GetPayload<EmailJob>();
        await SendEmailAsync(email);
        
        // Acknowledge the job to remove it from the queue
        job.Ack();
    }
    catch (Exception ex)
    {
        // Fail the job (will be retried based on queue settings)
        job.Fail(ex.Message);
    }
}
```

## 3. Notifications (Fire-and-Forget)

Ephemeral pub/sub. If a listener isn't connected when a notification fires, it is missed.

### Notifying
```csharp
// Sends a notification to the "config-reloaded" channel
db.Notify("config-reloaded", "v2.0 applied");
```

### Listening
```csharp
await foreach (var notification in db.Listen("config-reloaded", cancellationToken: ct))
{
    Console.WriteLine($"Received: {notification.PayloadRaw}");
}
```
