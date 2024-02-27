namespace HLE.Collections;

public interface IIndexable<out T>
{
    /// <summary>
    /// Gets the item at the given index of the collection.
    /// </summary>
    /// <param name="index">The index of the item.</param>
    T this[int index] { get; }
}
