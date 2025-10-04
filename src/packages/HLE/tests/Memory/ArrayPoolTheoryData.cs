using HLE.Memory;
using HLE.TestUtilities;

namespace HLE.UnitTests.Memory;

internal static class ArrayPoolTheoryData
{
    public static TheoryData<int> Pow2LengthMinimumToMaximumLengthParameters { get; } = CreatePow2LengthMinimumToMaximumLengthParameters();

    public static TheoryData<int> ConsecutiveValues0To4096Parameters { get; } = TheoryDataHelpers.CreateRange(0, 4096);

    public static TheoryData<int> ConsecutiveValues0ToMinimumLengthMinus1 { get; } = TheoryDataHelpers.CreateRange(0, ArrayPoolSettings.MinimumArrayLength - 1);

    private static TheoryData<int> CreatePow2LengthMinimumToMaximumLengthParameters()
    {
        TheoryData<int> data = new();
        for (int i = ArrayPoolSettings.MinimumArrayLength; i <= ArrayPoolSettings.MaximumArrayLength; i <<= 1)
        {
            data.Add(i);
        }

        return data;
    }
}
