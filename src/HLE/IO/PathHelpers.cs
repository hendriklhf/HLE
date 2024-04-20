using System;
using System.Diagnostics.Contracts;
using System.IO;
using HLE.Strings;

namespace HLE.IO;

public static class PathHelpers
{
    private static readonly TypeFormatter s_typeToPathFormatter = new(new()
    {
        NamespaceSeparator = Path.DirectorySeparatorChar,
        GenericTypesSeparator = ", ",
        GenericDelimiters = new("{", "}")
    });

    [Pure]
    public static string TypeNameToPath<T>() => TypeNameToPath(typeof(T));

    [Pure]
    public static string TypeNameToPath(Type type) => s_typeToPathFormatter.Format(type);
}
