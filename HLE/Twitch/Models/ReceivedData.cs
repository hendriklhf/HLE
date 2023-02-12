using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace HLE.Twitch.Models;

/// <summary>
/// Holds received data in an <see cref="Array"/> rented from an <see cref="ArrayPool{T}"/>. If an instance isn't needed anymore, it has to be disposed.
/// Otherwise it will lead to memory leaks.
/// Which also means that the instances or any members of this class should not be cached or persisted in any way after received by an event invocation.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public readonly struct ReceivedData : IDisposable
{
    public ReadOnlySpan<byte> Span => ((Span<byte>)_data)[.._dataLength];

    public ReadOnlyMemory<byte> Memory => ((Memory<byte>)_data)[.._dataLength];

    public int Length => _dataLength;

    private readonly byte[] _data;
    private readonly int _dataLength;

    private ReceivedData(byte[] data, int dataLength)
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
    public static ReceivedData Create(ReadOnlySpan<byte> data)
    {
        int dataLength = data.Length;
        byte[] rentedArray = ArrayPool<byte>.Shared.Rent(dataLength);
        ref byte source = ref Unsafe.As<byte, byte>(ref MemoryMarshal.GetReference(data));
        ref byte destination = ref Unsafe.As<byte, byte>(ref MemoryMarshal.GetArrayDataReference(rentedArray));
        Unsafe.CopyBlock(ref destination, ref source, (uint)(dataLength << 1));
        return new(rentedArray, dataLength);
    }

    public void Dispose()
    {
        ArrayPool<byte>.Shared.Return(_data);
    }

    public override string ToString()
    {
        return Encoding.UTF8.GetString(Span);
    }
}
