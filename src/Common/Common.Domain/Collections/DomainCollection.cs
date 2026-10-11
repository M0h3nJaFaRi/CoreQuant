using System.Collections;

namespace Common.Domain.Collections;

/// <summary>
/// Encapsulated mutable collection exposed to callers as <see cref="IReadOnlyList{T}"/>.
/// </summary>
public sealed class DomainCollection<T> : IReadOnlyList<T>
{
    private readonly List<T> _items = [];

    /// <inheritdoc />
    public T this[int index] => _items[index];

    /// <inheritdoc />
    public int Count => _items.Count;

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Adds an item.
    /// </summary>
    public void Add(T item) => _items.Add(item);

    /// <summary>
    /// Removes an item.
    /// </summary>
    public bool Remove(T item) => _items.Remove(item);

    /// <summary>
    /// Clears all items.
    /// </summary>
    public void Clear() => _items.Clear();
}