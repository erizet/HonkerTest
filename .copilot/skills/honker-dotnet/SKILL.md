---
name: honker-dotnet
description: Guidelines and API references for writing .NET applications using Honker (SQLite pub/sub, queues, event streams). Use when the user wants to implement messaging, queues, or event streams using SQLite and Honker in C# or .NET, or mentions honker-dotnet, pub/sub, or durable queues.
---

# Honker .NET Skill

This skill provides the conventions for using the `Honker` NuGet package in .NET. Honker adds Postgres-style pub/sub, durable queues, and event streams to SQLite without a broker.

## Quick Start

```csharp
using Honker;

// 1. Open database (loads extension, bootstraps tables)
using var db = Database.Open("app.db");

// 2. Publish to a stream
var stream = db.Stream("sensors");
stream.Publish(new { SensorId = 1, Temp = 22.5 });

// 3. Subscribe to a stream
await foreach (var evt in stream.Subscribe("my-consumer", cancellationToken: ct))
{
    Console.WriteLine(evt.PayloadRaw);
}
```

## Core Constraints & Gotchas

1. **No `:memory:` databases:** Honker requires a file-backed `.db` file. The wake mechanism relies on SQLite WAL and file modifications.
2. **JSON Serialization:** `Publish` and `Enqueue` use `System.Text.Json` defaults (PascalCase properties). When deserializing via `GetPayload<T>()`, you must use `[JsonPropertyName("PropertyName")]` on your C# records to match, or ensure property names are identical.
3. **Concurrency:** `Database` is thread-safe for writing. `Subscribe`, `Listen`, and queue consumers open their own isolated SQLite connections internally.

## Advanced Usage

For exact API syntax for **Queues (Worker/Claim)**, **Streams (Pub/Sub)**, and **Notifications (Fire-and-forget)**, see [REFERENCE.md](REFERENCE.md).
