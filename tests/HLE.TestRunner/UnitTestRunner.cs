using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using HLE.Collections;

namespace HLE.TestRunner;

internal sealed class UnitTestRunner(TextWriter outputWriter) : IDisposable, IEquatable<UnitTestRunner>
{
    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "UnitTestRunner doesn't own the writer")]
    private readonly TextWriter _outputWriter = outputWriter;
    private readonly ImmutableArray<TestProject> _testProjects = DiscoverTestProjects(outputWriter);

    public void Dispose()
    {
        foreach (TestProject project in _testProjects)
        {
            project.Dispose();
        }
    }

    public async Task<ImmutableArray<UnitTestRunResult>> RunAsync()
    {
        using PooledList<Task<UnitTestRunResult>> tasks = new();

        ReadOnlyMemory<EnvironmentConfiguration> environmentConfigurations = EnvironmentCombinator.Combinate();
        foreach (TestProject testProject in _testProjects)
        {
            foreach (EnvironmentConfiguration environment in environmentConfigurations.Span)
            {
                UnitTestRun run = new(_outputWriter, testProject, environment);
                tasks.Add(run.StartAsync());
            }
        }

        UnitTestRunResult[] result = await Task.WhenAll(tasks);
        return ImmutableCollectionsMarshal.AsImmutableArray(result);
    }

    private static ImmutableArray<TestProject> DiscoverTestProjects(TextWriter outputWriter)
    {
        string[] testProjectFiles = Directory.GetFiles($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}UnitTests", "*.csproj", SearchOption.AllDirectories);
        TestProject[] testProjects = new TestProject[testProjectFiles.Length];
        for (int i = 0; i < testProjectFiles.Length; i++)
        {
            testProjects[i] = new(outputWriter, testProjectFiles[i]);
        }

        return ImmutableCollectionsMarshal.AsImmutableArray(testProjects);
    }

    [Pure]
    public bool Equals([NotNullWhen(true)] UnitTestRunner? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(UnitTestRunner? left, UnitTestRunner? right) => Equals(left, right);

    public static bool operator !=(UnitTestRunner? left, UnitTestRunner? right) => !(left == right);
}
