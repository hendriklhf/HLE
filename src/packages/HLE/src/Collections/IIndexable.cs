using System;

namespace HLE.Collections;

public interface IIndexable<out T> : ICountable
{
    /// <summary>
    /// Gets the item at the given index of the collection.
    /// </summary>
    /// <param name="index">The index of the item.</param>
    T this[int index] { get; }

    /// <inheritdoc cref="this[int]"/>
    T this[Index index] { get; }
}
