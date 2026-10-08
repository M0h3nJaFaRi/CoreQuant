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
    }

    /// <summary>
    /// Creates an entity with the given identity.
    /// </summary>
    protected Entity(TId id)
    {
        Id = id;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Entity identity.
    /// </summary>
    public TId Id { get; protected set; } = default!;

    /// <summary>
    /// UTC creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; protected set; }

    /// <summary>
    /// UTC last modification timestamp.
    /// </summary>
    public DateTimeOffset? ModifiedAt { get; protected set; }

    /// <summary>
    /// UTC soft-delete timestamp. Null means active.
    /// </summary>
    public DateTimeOffset? DeletedAt { get; protected set; }

    /// <summary>
    /// Acting user id that created the row (snowflake). Null = unknown/system.
    /// </summary>
    public long? CreatedBy { get; protected set; }

    /// <summary>
    /// Acting user id that last modified the row.
    /// </summary>
    public long? ModifiedBy { get; protected set; }

    /// <summary>
    /// Acting user id that soft-deleted the row.
    /// </summary>
    public long? DeletedBy { get; protected set; }

    /// <summary>
    /// Optimistic concurrency token.
    /// </summary>
    public int Version { get; protected set; }

    /// <summary>
    /// True when the entity is soft-deleted.
    /// </summary>
    public bool IsDeleted => DeletedAt is not null;

    /// <summary>
    /// Stamps creation actor when known at create time.
    /// </summary>
    public void StampCreated(long? createdBy = null)
    {
        if (CreatedAt == default)
        {
            CreatedAt = DateTimeOffset.UtcNow;
        }

        CreatedBy = createdBy;
    }

    /// <summary>
    /// Marks the entity as modified.
    /// </summary>
    public void Touch(long? modifiedBy = null)
    {
        ModifiedAt = DateTimeOffset.UtcNow;
        ModifiedBy = modifiedBy;
        Version++;
    }

    /// <summary>
    /// Soft-deletes the entity.
    /// </summary>
    public void MarkAsRemoved(long? deletedBy = null)
    {
        if (IsDeleted)
        {
            return;
        }

        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
        ModifiedAt = DeletedAt;
        ModifiedBy = deletedBy;
        Version++;
    }
}
