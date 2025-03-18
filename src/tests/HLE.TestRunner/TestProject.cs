using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using HLE.IO;

namespace HLE.TestRunner;

internal sealed class TestProject(TextWriter outputWriter, string filePath) : IDisposable, IEquatable<TestProject>
{
    public string FilePath { get; } = filePath;

    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "TestProject doesn't own the writer.")]
    private readonly TextWriter _outputWriter = outputWriter;
    private readonly List<string> _temporaryDirectories = new();

    private static readonly SemaphoreSlim s_projectBuildLock = new(1);
    private static readonly string s_baseBuildOutputPath = Path.Combine(Path.GetTempPath(), PathHelpers.TypeNameToPath<UnitTestRunner>());

    public void Dispose()
    {
        foreach (string temporaryDirectory in _temporaryDirectories)
        {
            if (Directory.Exists(temporaryDirectory))
            {
                Directory.Delete(temporaryDirectory, true);
            }
        }

        _temporaryDirectories.Clear();
    }

    public async Task<string?> BuildAsync(EnvironmentConfiguration environment)
    {
        string buildOutputPath = CreateTemporaryDirectory();
        ProcessStartInfo startInfo = new()
        {
            FileName = "dotnet",
            Arguments = $"build \"{FilePath}\" -c {environment.Configuration} -r {environment.RuntimeIdentifier} -o \"{buildOutputPath}\"",
            RedirectStandardOutput = true
        };

        using Process? buildProcess = Process.Start(startInfo);
        ArgumentNullException.ThrowIfNull(buildProcess);

        await s_projectBuildLock.WaitAsync();
        try
        {
            ProcessOutputReader outputReader = new(buildProcess, _outputWriter);

            buildProcess.Start();
            Task readerTask = outputReader.StartReadingAsync();

            await buildProcess.WaitForExitAsync();
            await readerTask;
        }
        finally
        {
            s_projectBuildLock.Release();
        }

        return buildProcess.ExitCode == 0 ? buildOutputPath : null;
    }

    private string CreateTemporaryDirectory()
    {
        string path = Path.Combine(s_baseBuildOutputPath, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        lock (_temporaryDirectories)
        {
            _temporaryDirectories.Add(path);
        }

        return path;
    }

    [Pure]
    public bool Equals([NotNullWhen(true)] TestProject? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(TestProject? left, TestProject? right) => Equals(left, right);

    public static bool operator !=(TestProject? left, TestProject? right) => !(left == right);
}
