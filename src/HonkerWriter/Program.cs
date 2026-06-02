using System.Text.Json.Serialization;
using Honker;

var dbPath = args.Length > 0 ? args[0] : "demo.db";

Console.WriteLine("╔══════════════════════════════════════════╗");
Console.WriteLine("║          Honker.dev  —  Writer           ║");
Console.WriteLine("╚══════════════════════════════════════════╝");
Console.WriteLine($"  Database : {Path.GetFullPath(dbPath)}");
Console.WriteLine("  Publishing a temperature reading every 500 ms.");
Console.WriteLine("  Press Ctrl+C to stop.\n");

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

using var db = Database.Open(dbPath);
var stream = db.Stream("sensors");

var rng = new Random();
var temp = 20.0;

for (var i = 0; !cts.Token.IsCancellationRequested; i++)
{
    temp += rng.NextDouble() * 2.0 - 1.0;
    var payload = new SensorReading(i, Math.Round(temp, 2), "°C");
    var offset = stream.Publish(payload);
    Console.WriteLine($"  published  #{i:D4}  offset={offset}  temp={temp:F2}°C");

    try { await Task.Delay(500, cts.Token); }
    catch (OperationCanceledException) { break; }
}

Console.WriteLine("\n  Writer stopped.");

internal sealed record SensorReading(
    [property: JsonPropertyName("index")]       int    Index,
    [property: JsonPropertyName("temperature")] double Temperature,
    [property: JsonPropertyName("unit")]        string Unit
);
