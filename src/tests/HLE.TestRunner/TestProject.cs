using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HLE.IO;

namespace HLE.TestRunner;

internal sealed class TestProject(string projectFilePath) : IDisposable, IEquatable<TestProject>
{
    public string ProjectFilePath { get; } = projectFilePath;

    private readonly List<string> _temporaryDirectories = new();

    private static readonly string s_baseBuildOutputPath = Path.Combine(Path.GetTempPath(), PathHelpers.TypeNameToPath<UnitTestRunner>());
    private static readonly string s_executableExtension = OperatingSystem.IsWindows() ? ".exe" : string.Empty;

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
            Arguments = $"build \"{ProjectFilePath}\" -f {environment.TargetFramework} -c {environment.Configuration} -r {environment.RuntimeIdentifier} -o \"{buildOutputPath}\""
        };

        using Process? buildProcess = Process.Start(startInfo);
        ArgumentNullException.ThrowIfNull(buildProcess);

        await buildProcess.WaitForExitAsync();

        if (buildProcess.ExitCode != 0)
        {
            throw new InvalidOperationException($"Building {ProjectFilePath} failed.");
        }

        string projectName = Path.GetFileNameWithoutExtension(ProjectFilePath);
        return Path.Combine(buildOutputPath, projectName + s_executableExtension);
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
