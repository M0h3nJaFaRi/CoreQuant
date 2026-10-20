namespace Common.Domain.Core;

/// <summary>
/// Base entity with identity, soft-delete, audit actors, and concurrency version.
/// </summary>
/// <typeparam name="TId">Identifier type.</typeparam>
public abstract class Entity<TId>
where TId : notnull
{
    /// <summary>
    /// EF Core materialization constructor.
    /// </summary>
    protected Entity()
    {
        StampCreated();
    }

    /// <summary>
    /// Creates an entity with the given identity.
    /// </summary>
    protected Entity(TId id, ulong? userId = null)
    {
        Id = id;
        StampCreated(userId);
    }

    /// <summary>
    /// Entity identity.
    /// </summary>
    public TId Id { get; private set; } = default!;

    /// <summary>
    /// UTC creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// UTC last modification timestamp.
    /// </summary>
    public DateTimeOffset? ModifiedAt { get; private set; }

    /// <summary>
    /// UTC soft-delete timestamp. Null means active.
    /// </summary>
    public DateTimeOffset? DeletedAt { get; private set; }

    /// <summary>
    /// Acting user id that created the row (snowflake). Null = unknown/system.
    /// </summary>
    public ulong? CreatedBy { get; private set; }

    /// <summary>
    /// Acting user id that last modified the row.
    /// </summary>
    public ulong? ModifiedBy { get; private set; }

    /// <summary>
    /// Acting user id that soft-deleted the row.
    /// </summary>
    public ulong? DeletedBy { get; private set; }

    /// <summary>
    /// Optimistic concurrency token.
    /// </summary>
    public uint Version { get; private set; }

    /// <summary>
    /// True when the entity is soft-deleted.
    /// </summary>
    public bool IsDeleted => DeletedAt is not null;

    /// <summary>
    /// Stamps creation actor when known at create time.
    /// </summary>
    protected void StampCreated(ulong? createdBy = null)
    {
        if (CreatedAt == default) CreatedAt = DateTimeOffset.UtcNow;

        CreatedBy = createdBy;
    }

    /// <summary>
    /// Marks the entity as modified.
    /// </summary>
    protected void Touch(ulong? modifiedBy = null)
    {
        ModifiedAt = DateTimeOffset.UtcNow;
        ModifiedBy = modifiedBy;
        Version++;
    }

    /// <summary>
    /// Soft-deletes the entity.
    /// </summary>
    public void MarkAsRemoved(ulong? deletedBy = null)
    {
        if (IsDeleted) return;

        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
        Version++;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other) return false;

        if (ReferenceEquals(this, other)) return true;

        return GetType() == other.GetType() && EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }
    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) => Equals(left, right);
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !Equals(left, right);
}