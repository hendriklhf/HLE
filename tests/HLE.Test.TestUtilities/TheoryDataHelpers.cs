using System;
using System.Numerics;
using HLE.Marshalling;
using Xunit;

namespace HLE.Test.TestUtilities;

public static class TheoryDataHelpers
{
    public static TheoryData<T> CreateRange<T>(T min, T max) where T : INumber<T>
    {
        TheoryData<T> data = new();
        for (T i = min; i <= max; i++)
        {
            data.Add(i);
        }

        return data;
    }

    public static TheoryData<string> CreateRandomStrings(int stringCount, int minLength, int maxLength)
    {
        TheoryData<string> data = new();
        for (int i = 0; i < stringCount; i++)
        {
            int length = Random.Shared.Next(minLength, maxLength);
            string str = StringMarshal.FastAllocateString(length, out Span<char> chars);
            Random.Shared.Fill(chars);
            data.Add(str);
        }

        return data;
    }
}
