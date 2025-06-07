using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using HLE.Collections;

namespace HLE.TestRunner;

internal sealed class UnitTestRunner : IDisposable, IEquatable<UnitTestRunner>
{
    private readonly ImmutableArray<TestProject> _testProjects = DiscoverTestProjects();

    public void Dispose() => DisposeHelpers.DisposeAll(_testProjects.AsSpan());

    public async Task<ImmutableArray<UnitTestRunResult>> RunAsync(CancellationToken stoppingToken)
    {
        ReadOnlyMemory<EnvironmentConfiguration> environmentConfigurations = EnvironmentCombinator.Combine();
        using PooledList<UnitTestRunResult> results = new(_testProjects.Length * environmentConfigurations.Length);

        foreach (TestProject testProject in _testProjects)
        {
            for (int i = 0; i < environmentConfigurations.Length; i++)
            {
                stoppingToken.ThrowIfCancellationRequested();
                await testProject.BuildAsync(environmentConfigurations.Span[i], stoppingToken).ConfigureAwait(false);
            }
        }

        foreach (TestProject testProject in _testProjects)
        {
            for (int i = 0; i < environmentConfigurations.Length; i++)
            {
                stoppingToken.ThrowIfCancellationRequested();
                await testProject.RunAsync(results, stoppingToken).ConfigureAwait(false);
            }
        }

        return ImmutableCollectionsMarshal.AsImmutableArray(results.ToArray());
    }

    private static ImmutableArray<TestProject> DiscoverTestProjects()
    {
        string[] testProjectFiles = Directory.GetFiles($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}libraries", "HLE.*.UnitTests.csproj", SearchOption.AllDirectories);
        TestProject[] testProjects = new TestProject[testProjectFiles.Length];
        for (int i = 0; i < testProjectFiles.Length; i++)
        {
            testProjects[i] = new(testProjectFiles[i]);
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
