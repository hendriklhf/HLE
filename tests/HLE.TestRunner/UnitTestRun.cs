using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace HLE.TestRunner;

internal sealed class UnitTestRun(TextWriter outputWriter, TestProject testProject, EnvironmentConfiguration environment) : IEquatable<UnitTestRun>
{
    private readonly TextWriter _outputWriter = outputWriter;
    private readonly TestProject _testProject = testProject;
    private readonly EnvironmentConfiguration _environment = environment;

    private static readonly string s_executableFileExtensions = OperatingSystem.IsWindows() ? ".exe" : string.Empty;

    public async Task<bool> StartAsync()
    {
        string? buildOutputPath = await _testProject.BuildAsync(_environment);
        if (buildOutputPath is null)
        {
            return false;
        }

        string projectName = Path.GetFileNameWithoutExtension(_testProject.FilePath);
        string executablePath = Path.Combine(buildOutputPath, projectName) + s_executableFileExtensions;
        return await RunExecutableAsync(executablePath, _environment);
    }

    private async Task<bool> RunExecutableAsync(string executablePath, EnvironmentConfiguration environment)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = executablePath,
            WorkingDirectory = executablePath[..^Path.GetFileName(executablePath.AsSpan()).Length],
            RedirectStandardOutput = true
        };

        foreach (KeyValuePair<string, string> variable in environment.EnvironmentVariables)
        {
            startInfo.Environment.Add(variable.Key, variable.Value);
        }

        using Process testRunProcess = new()
        {
            StartInfo = startInfo
        };

        using ProcessOutputReader outputReader = new(testRunProcess, _outputWriter);
        Task readerTask = outputReader.StartReadingAsync();

        testRunProcess.Start();
        outputReader.NotifyProcessStarted();

        await testRunProcess.WaitForExitAsync();
        await readerTask;

        return testRunProcess.ExitCode == 0;
    }

    [Pure]
    public bool Equals([NotNullWhen(true)] UnitTestRun? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(UnitTestRun? left, UnitTestRun? right) => Equals(left, right);

    public static bool operator !=(UnitTestRun? left, UnitTestRun? right) => !(left == right);
}
