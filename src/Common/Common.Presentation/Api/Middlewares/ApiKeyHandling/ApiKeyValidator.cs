using Common.Presentation.Api.Configuration.Options;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace Common.Presentation.Api.Middlewares.ApiKeyHandling;

/// <summary>
/// Register as a singleton. Keys of enabled clients are SHA-256 hashed once per options instance
/// (i.e. once per config reload) instead of on every request.
/// </summary>
public sealed class ApiKeyValidator
{
    private volatile Snapshot? _snapshot;
    private readonly record struct Entry(string ClientName, byte[] Hash);
    private sealed record Snapshot(ApiKeyOptions Source, Entry[] Entries);

    public bool TryValidate(ApiKeyOptions options, string? suppliedKey, [NotNullWhen(true)] out string? clientName)
    {
        clientName = null;
        string? match = null;

        if (string.IsNullOrWhiteSpace(suppliedKey))
            return false;

        var entries = GetEntries(options);

        Span<byte> supplied = stackalloc byte[SHA256.HashSizeInBytes];
        SHA256.HashData(Encoding.UTF8.GetBytes(suppliedKey.Trim()), supplied);


        foreach (var entry in entries)
            if (CryptographicOperations.FixedTimeEquals(supplied, entry.Hash))
                match = entry.ClientName;

        clientName = match;
        return match is not null;
    }

    private Entry[] GetEntries(ApiKeyOptions options)
    {
        var snapshot = _snapshot;

        if (snapshot is not null && ReferenceEquals(snapshot.Source, options))
            return snapshot.Entries;

        var entries = options.Clients.Where(c => c.Value is { Enabled: true })
                                     .SelectMany(c => c.Value.Keys.Where(k => !string.IsNullOrWhiteSpace(k))
                                                                  .Select(k => new Entry(c.Key, SHA256.HashData(Encoding.UTF8.GetBytes(k.Trim())))))
                                     .ToArray();

        _snapshot = new Snapshot(options, entries);
        return entries;
    }
}