using System;
using System.Text;

namespace HLE.SourceGenerators.AppendMethods;

public static class StringBuilderExtensions
{
    public static StringBuilder AppendJoin(this StringBuilder builder, string separator, ReadOnlySpan<string> values)
    {
        if (values.Length == 0)
        {
            return builder;
        }

        int length = values.Length - 1;

        for (int i = 0; i < length; i++)
        {
            builder.Append(values[i]).Append(separator);
        }

        return builder.Append(values[values.Length - 1]);
    }
}
