using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace HLE.TestRunner;

internal static class Program
{
    private static async Task<int> Main()
    {
        using UnitTestRunner runner = new();
        ImmutableArray<UnitTestRunResult> results = await runner.RunAsync();
        return results.Count(static r => !r.IsSuccess);
    }
}
