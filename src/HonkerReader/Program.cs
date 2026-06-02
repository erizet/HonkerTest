using System.Text.Json.Serialization;
using Honker;

var dbPath = args.Length > 0 ? args[0] : "demo.db";

Console.WriteLine("╔══════════════════════════════════════════╗");
Console.WriteLine("║          Honker.dev  —  Reader           ║");
Console.WriteLine("╚══════════════════════════════════════════╝");
Console.WriteLine($"  Database : {Path.GetFullPath(dbPath)}");
Console.WriteLine("  Subscribing to the \"sensors\" stream.");
Console.WriteLine("  Resumes from last saved offset on restart.");
Console.WriteLine("  Press Ctrl+C to stop.\n");

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

using var db = Database.Open(dbPath);
var stream = db.Stream("sensors");

Console.WriteLine("  Waiting for events...\n");

await foreach (var evt in stream.Subscribe(consumer: "demo-reader", cancellationToken: cts.Token))
{
    var reading = evt.GetPayload<SensorReading>()!;
    Console.WriteLine($"  received   #{reading.Index:D4}  offset={evt.Offset}  temp={reading.Temperature:F2}{reading.Unit}  ← woke!");
}

Console.WriteLine("\n  Reader stopped.");

internal sealed record SensorReading(
    [property: JsonPropertyName("index")]       int    Index,
    [property: JsonPropertyName("temperature")] double Temperature,
    [property: JsonPropertyName("unit")]        string Unit
);
