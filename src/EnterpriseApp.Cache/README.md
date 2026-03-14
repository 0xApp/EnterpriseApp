# EnterpriseApp.Cache

A high-performance, thread-safe caching library for .NET targeting `netstandard2.1`.

## Features

- **Generic LRU (Least Recently Used) cache** with O(1) get, put, and eviction
- **Thread-safe** — all operations are synchronized and safe for concurrent use
- **Post-eviction callbacks** — register one or more callbacks per entry, invoked when an entry is evicted, replaced, or removed
- **IDisposable-aware** — automatically disposes cached values that implement `IDisposable` on eviction, clear, or cache disposal
- **Ownership transfer** — retrieve and remove an entry without the cache disposing it, transferring disposal responsibility to the caller
- **Snapshot-based enumeration** — safely iterate over cache contents (MRU to LRU order) without holding the lock
- **Zero external dependencies** — pure .NET Standard 2.1, no third-party packages

## Installation

Add a project reference:

```xml
<ProjectReference Include="..\EnterpriseApp.Cache\EnterpriseApp.Cache.csproj" />
```

## Quick Start

```csharp
using EnterpriseApp.Cache;

// Create a cache with a maximum of 1000 entries
var cache = new LruCache<string, UserProfile>(capacity: 1000);

// Add or update entries
cache.AddOrUpdate("user-42", profile);

// Retrieve an entry (promotes it to most-recently-used)
if (cache.TryGetValue("user-42", out var profile))
{
    // use profile
}

// Peek without promoting
cache.TryPeek("user-42", out var peeked);

// Remove
cache.Remove("user-42");

// Iterate in MRU → LRU order
foreach (var kvp in cache)
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
}
```

## Documentation

| Topic | Description |
|-------|-------------|
| [LruCache](./docs/LruCache.md) | Full API reference, design details, and usage examples |

## Target Framework

| Package | Target |
|---------|--------|
| EnterpriseApp.Cache | `netstandard2.1` |
| EnterpriseApp.Cache.Tests | `net6.0` through `net10.0` |
