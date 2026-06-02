# Copilot Instructions

## Build & Run

The easiest way — builds once and opens Reader + Writer in separate windows:

```powershell
.\run-demo.ps1          # reuse existing demo.db (reader resumes its offset)
.\run-demo.ps1 -Fresh   # delete demo.db first for a clean run
```

Or manually in two terminals (both default to `demo.db` in the working directory):

```bash
# Terminal 1 — reader waits for events, resumes offset on restart
dotnet run --project src/HonkerReader

# Terminal 2 — writer publishes every 500 ms
dotnet run --project src/HonkerWriter

# Pass a custom db path as the first argument
dotnet run --project src/HonkerReader -- /tmp/my.db
```

## Architecture

This is a two-process .NET 10 demo for [Honker.dev](https://honker.dev) — a SQLite loadable extension that adds Postgres-style pub/sub, durable queues, and event streams without a broker or daemon.

```
demo.db  (shared SQLite file on disk)
   │
   ├── HonkerWriter  ──  stream.Publish(payload)  every 500 ms
   │
   └── HonkerReader  ──  stream.Subscribe(consumer:)  wakes within ~1–2 ms
```

- **HonkerWriter** opens `demo.db`, gets the `"sensors"` stream, and publishes a temperature `SensorReading` record every 500 ms.
- **HonkerReader** opens the same `demo.db`, subscribes as consumer `"demo-reader"`, and prints each event as Honker wakes it. Consumer offsets are saved automatically, so the reader **resumes from where it left off** on restart.
- The two processes share no memory — Honker's 1 ms `PRAGMA data_version` poll on the SQLite WAL-index is the only wake signal.

## Key Conventions

**Honker .NET API used here:**

| Operation | Call |
|---|---|
| Open DB | `Database.Open("path.db")` |
| Get stream | `db.Stream("name")` |
| Publish | `stream.Publish(payload)` → `long offset` |
| Subscribe | `stream.Subscribe(consumer: "name", cancellationToken: ct)` → `IAsyncEnumerable<Event>` |
| Event fields | `evt.Offset`, `evt.PayloadRaw` (JSON), `evt.GetPayload<T>()` |

**Payload serialization:** `stream.Publish(obj)` serializes with `System.Text.Json` defaults (PascalCase). Deserialization records must use `[JsonPropertyName]` to match the serialized keys.

**Do not use `:memory:` databases** — Honker requires a file-backed `.db` for the watcher/wake mechanism to work across connections and processes.

**Consumer offset persistence:** `Subscribe(consumer: "name")` saves the read offset automatically (every 1000 events or 1 s by default). Restarting the reader replays any events it missed while stopped.
