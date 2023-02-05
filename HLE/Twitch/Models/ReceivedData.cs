using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace HLE.Twitch.Models;

/// <summary>
/// Holds received data in an <see cref="Array"/> rented from an <see cref="ArrayPool{T}"/>. If an instance isn't needed anymore, it has to be disposed.
/// Otherwise it will lead to memory leaks.
/// Which also means that the instances or any members of this class should not be cached or persisted in any way after received by an event invocation.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public readonly struct ReceivedData : IDisposable
{
    public ReadOnlySpan<char> Span => ((Span<char>)_data)[.._dataLength];

    public ReadOnlyMemory<char> Memory => ((Memory<char>)_data)[.._dataLength];

    private readonly char[] _data;
    private readonly int _dataLength;

    private ReceivedData(char[] data, int dataLength)
    {
        _data = data;
        _dataLength = dataLength;
    }

    /// <summary>
    /// Copies the content of <paramref name="data"/> into a rented array and returns a new <see cref="ReceivedData"/> instance containing the array.
    /// The instance needs to be disposed when not used anymore. Read more in the documentation of the <see cref="ReceivedData"/> class.
    /// </summary>
    /// <param name="data">The data that will be copied into the instance.</param>
    /// <returns>A <see cref="ReceivedData"/> instance containing the copied data.</returns>
    [Pure]
    public static ReceivedData Create(ReadOnlySpan<char> data)
    {
        int dataLength = data.Length;
        char[] rentedArray = ArrayPool<char>.Shared.Rent(dataLength);
        data.CopyTo(rentedArray);
        return new(rentedArray, dataLength);
    }

    public void Dispose()
    {
        ArrayPool<char>.Shared.Return(_data);
    }

    public override string ToString()
    {
        return new(Span);
    }
}
