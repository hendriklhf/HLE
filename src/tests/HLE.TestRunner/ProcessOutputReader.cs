using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HLE.Memory;

namespace HLE.TestRunner;

[SuppressMessage("Usage", "CA2213:Disposable fields should be disposed")]
internal readonly struct ProcessOutputReader(Process process, TextWriter outputWriter) : IEquatable<ProcessOutputReader>
{
    private readonly Process _process = process;
    private readonly TextWriter _outputWriter = outputWriter;

    private static readonly SemaphoreSlim s_outputWriterLock = new(1);

    public async Task StartReadingAsync()
    {
        using PooledBufferWriter<char> bufferWriter = new(4096);
        Process process = _process;
        StreamReader outputReader = process.StandardOutput;

        Debug.Assert(!_process.HasExited);

        do
        {
            Memory<char> buffer = bufferWriter.GetMemory(4096);
            int charsRead = await outputReader.ReadAsync(buffer);
            bufferWriter.Advance(charsRead);
        }
        while (!process.HasExited);

        await s_outputWriterLock.WaitAsync();
        try
        {
            await _outputWriter.WriteAsync(bufferWriter.WrittenMemory);
        }
        finally
        {
            s_outputWriterLock.Release();
        }
    }

    [Pure]
    public bool Equals(ProcessOutputReader other) => _process.Equals(other._process) && _outputWriter.Equals(other._outputWriter);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ProcessOutputReader other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(_process, _outputWriter);

    public static bool operator ==(ProcessOutputReader? left, ProcessOutputReader? right) => Equals(left, right);

    public static bool operator !=(ProcessOutputReader? left, ProcessOutputReader? right) => !(left == right);
}
