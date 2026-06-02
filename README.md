# HonkerTest

A minimal .NET 10 demo for [Honker.dev](https://honker.dev) — a SQLite loadable extension that adds pub/sub and event streams without a broker or daemon.

Two separate processes share a single `demo.db` file:

```
HonkerWriter  →  stream.Publish(SensorReading)  every 500 ms
HonkerReader  →  stream.Subscribe(...)           wakes within ~1–2 ms
```

The reader saves its offset automatically and **resumes from where it left off** on restart.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Quick start

```powershell
.\run-demo.ps1          # build, then open Reader + Writer in separate windows
.\run-demo.ps1 -Fresh   # same, but delete demo.db first for a clean run
```

Or in two terminals:

```bash
dotnet run --project src/HonkerReader   # start reader first
dotnet run --project src/HonkerWriter   # then writer
```
