using System.Numerics;
using Xunit;

namespace HLE.Test.TestUtilities;

public static class TheoryDataHelpers
{
    public static TheoryData<T> CreateExclusiveRange<T>(T min, T max) where T : INumber<T>
    {
        TheoryData<T> data = new();
        for (T i = min; i < max; i++)
        {
            data.Add(i);
        }

        return data;
    }

    public static TheoryData<T> CreateInclusiveRange<T>(T min, T max) where T : INumber<T>
    {
        TheoryData<T> data = new();
        for (T i = min; i <= max; i++)
        {
            data.Add(i);
        }

        return data;
    }
}
