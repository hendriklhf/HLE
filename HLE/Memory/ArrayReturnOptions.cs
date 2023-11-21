using System;

namespace HLE.Memory;

[Flags]
public enum ArrayReturnOptions
{
    /// <summary>
    /// Returns the array to the pool without performing any action on it.
    /// </summary>
    None,
    /// <summary>
    /// Clears the array before returning it to the pool.
    /// </summary>
    Clear = 1,
    /// <summary>
    /// Clears the array only if the element type is a managed type before returning it to the pool.
    /// </summary>
    ClearOnlyIfManagedType = 2,
    /// <summary>
    /// Disposes all elements in the array if the element type implements <see cref="IDisposable"/> before returning it to the pool.
    /// </summary>
    DisposeElements = 4
}
