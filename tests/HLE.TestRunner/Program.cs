using System;
using System.Threading.Tasks;

namespace HLE.TestRunner;

internal static class Program
{
    private static async Task<int> Main()
    {
        using UnitTestRunner runner = new(Console.Out);
        return await runner.RunAsync();
    }
}
