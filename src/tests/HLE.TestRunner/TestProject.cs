using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using HLE.Collections;
using HLE.IO;
using HLE.Text;

namespace HLE.TestRunner;

internal sealed class TestProject(string projectFilePath) : IDisposable, IEquatable<TestProject>
{
    public string ProjectFilePath { get; } = projectFilePath;

    private readonly List<Task<string>> _buildTasks = new();
    private readonly List<string> _temporaryDirectories = new();

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

#pragma warning disable CA1859
    public Task BuildAsync(EnvironmentConfiguration environment, CancellationToken stoppingToken)
#pragma warning restore CA1859
    {
        Task<string> t = BuildCoreAsync(environment, stoppingToken);
        _buildTasks.Add(t);
        return t;

        async Task<string> BuildCoreAsync(EnvironmentConfiguration environment, CancellationToken stoppingToken)
        {
            string buildOutputPath = CreateTemporaryDirectory(environment);
            ProcessStartInfo startInfo = new()
            {
                FileName = "dotnet",
                Arguments = $"build \"{ProjectFilePath}\" -f {environment.TargetFramework} -c {environment.Configuration} -r {environment.RuntimeIdentifier} -o \"{buildOutputPath}\""
            };

            stoppingToken.ThrowIfCancellationRequested();

            using Process? buildProcess = Process.Start(startInfo);
            ArgumentNullException.ThrowIfNull(buildProcess);

            await buildProcess.WaitForExitAsync(stoppingToken).ConfigureAwait(false);

            if (buildProcess.ExitCode != 0)
            {
                throw new InvalidOperationException($"Building {ProjectFilePath} failed.");
            }

            string projectName = Path.GetFileNameWithoutExtension(ProjectFilePath);
            return Path.Combine(buildOutputPath, projectName + ".dll");
        }
    }

    public async Task RunAsync(PooledList<UnitTestRunResult> results, CancellationToken stoppingToken)
    {
        List<Task<string>> buildTasks = _buildTasks;

        for (int i = 0; i < buildTasks.Count; i++)
        {
            string assemblyPath = await buildTasks[i];

            ProcessStartInfo startInfo = new()
            {
                FileName = "dotnet",
                Arguments = $"test \"{assemblyPath}\"",
                WorkingDirectory = Path.GetDirectoryName(assemblyPath)
            };

            stoppingToken.ThrowIfCancellationRequested();

            using Process? testProcess = Process.Start(startInfo);
            ArgumentNullException.ThrowIfNull(testProcess);

            await testProcess.WaitForExitAsync(stoppingToken).ConfigureAwait(false);

            results.Add(testProcess.ExitCode == 0 ? UnitTestRunResult.Success : UnitTestRunResult.Failure);
        }
    }

    private string CreateTemporaryDirectory(EnvironmentConfiguration environment)
    {
        Span<char> randomChars = stackalloc char[8];
        Random.Shared.Fill(randomChars, StringConstants.HexadecimalsLowerCase);

        string path = Path.Combine(s_baseBuildOutputPath, $"{environment.TargetFramework}-{environment.RuntimeIdentifier}-{environment.Configuration}-{randomChars}");
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
