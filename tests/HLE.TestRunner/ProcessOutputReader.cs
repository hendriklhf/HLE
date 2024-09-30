using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using HLE.Memory;

namespace HLE.TestRunner;

[SuppressMessage("Usage", "CA2213:Disposable fields should be disposed")]
internal sealed class ProcessOutputReader(Process process, TextWriter outputWriter) : IDisposable, IEquatable<ProcessOutputReader>
{
    private readonly Process _process = process;
    private readonly TextWriter _outputWriter = outputWriter;
    private readonly ManualResetEventSlim _processStartSignal = new(false);

    private static readonly SemaphoreSlim s_outputWriterLock = new(1);

    public void Dispose() => _processStartSignal.Dispose();

    public void NotifyProcessStarted() => _processStartSignal.Set();

    public async Task StartReadingAsync()
    {
        _processStartSignal.Wait();

        Debug.Assert(!_process.HasExited);

        using PooledBufferWriter<char> bufferWriter = new(4096);
        StreamReader outputReader = _process.StandardOutput;

        do
        {
            Memory<char> buffer = bufferWriter.GetMemory(4096);
            int charsRead = await outputReader.ReadAsync(buffer);
            bufferWriter.Advance(charsRead);
        }
        while (!_process.HasExited);

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
    public bool Equals([NotNullWhen(true)] ProcessOutputReader? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(ProcessOutputReader? left, ProcessOutputReader? right) => Equals(left, right);

    public static bool operator !=(ProcessOutputReader? left, ProcessOutputReader? right) => !(left == right);
}
