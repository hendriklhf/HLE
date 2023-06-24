namespace HLE.Collections;

public interface IRefIndexAccessible<T>
{
    /// <summary>
    /// Gets a reference to an item at the given index of the collection.
    /// </summary>
    /// <param name="index">The index of the item.</param>
    ref T this[int index] { get; }
}
