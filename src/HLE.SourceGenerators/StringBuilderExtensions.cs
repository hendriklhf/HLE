using System;
using System.Text;

namespace HLE.SourceGenerators;

public static class StringBuilderExtensions
{
    public static StringBuilder AppendJoin(this StringBuilder builder, string separator, ReadOnlySpan<string> values)
    {
        switch (values.Length)
        {
            case 0:
                return builder;
            case 1:
                return builder.Append(values[0]);
        }

        int length = values.Length - 1;
        for (int i = 0; i < length; i++)
        {
            builder.Append(values[i]).Append(separator);
        }

        return builder.Append(values[length]);
    }

    public static StringBuilder AppendJoin(this StringBuilder builder, char separator, string value, int count)
    {
        switch (count)
        {
            case 0:
                return builder;
            case 1:
                return builder.Append(value);
        }

        int length = count - 1;
        for (int i = 0; i < length; i++)
        {
            builder.Append(value).Append(separator);
        }

        return builder.Append(value);
    }

    public static StringBuilder Append(this StringBuilder builder, string str, int count)
    {
        for (int i = 0; i < count; i++)
        {
            builder.Append(str);
        }

        return builder;
    }
}
