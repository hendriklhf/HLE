using System;
using System.Threading;

namespace HLE.TestRunner;

internal static class ConsoleCancellation
{
    public static CancellationToken Token => s_cancellationTokenSource.Token;

    private static readonly CancellationTokenSource s_cancellationTokenSource;

    static ConsoleCancellation()
    {
        s_cancellationTokenSource = new();

        Console.CancelKeyPress += static (_, e) =>
        {
            e.Cancel = true;
            s_cancellationTokenSource.Cancel();
        };
    }
}
