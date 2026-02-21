using System;
using System.Threading.Tasks;
using HLE.RemoteExecution;
using HLE.TestUtilities;
using HLE.Text;

namespace HLE.UnitTests.Text;

public sealed class BloomFilterStringArrayTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper _output = output;

    public static TheoryData<(RemoteExecutorOptions, int)> Parameters { get; } = TheoryDataHelpers.CreateMatrix(
        TheoryDataHelpers.VectorExecutionOptions,
        (TheoryData<int>)[4, 8, 16, 32, 64, 128, 256, 512, 1024]
    );

    [Theory]
    [MemberData(nameof(Parameters))]
    public async Task IndexOfTest((RemoteExecutorOptions Options, int Length) parameter)
    {
        RemoteExecutorResult result = await RemoteExecutor.InvokeAsync(Remote_IndexOfTest, parameter.Options, parameter.Length);
        Assert.RemoteExecutionSuccess(result, _output);
    }

    private static void Remote_IndexOfTest(int length)
    {
        Random random = new(285);

        BloomFilterStringArray array = BloomFilterStringArray.Create(length);
        for (int i = 0; i < array.Length; i++)
        {
            int stringLength = random.Next(4, 256);
            string str = random.NextString(stringLength, StringConstants.AlphaNumerics);
            array[i] = str;
        }

        for (int i = 0; i < array.Length; i++)
        {
            string? str = array[i];
            int index = array.IndexOf(str);
            Assert.Equal(i, index);
            Assert.Equal(str, array[index]);
        }
    }
}
