namespace CoreQuant.Infrastructure.Redis.Core.Contract;

/// <summary>
/// Lock owner token stored as a raw Redis string (not JSON-wrapped).
/// </summary>
internal sealed record RedisLockOwner(string Token);
