using HLE.RemoteExecution;
using Xunit;

namespace HLE.TestUtilities;

public static class AssertExtensions
{
    extension(Assert)
    {
        public static void RemoteExecutionSuccess(RemoteExecutorResult result, ITestOutputHelper? outputHelper = null)
        {
            if (result.Output is { Length: not 0 } output)
            {
                outputHelper?.WriteLine(output);
            }

            Assert.Equal(0, result.ExitCode);
        }
    }
}
