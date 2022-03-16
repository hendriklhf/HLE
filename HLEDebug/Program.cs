#nullable enable

using System;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using HLE.Strings;

namespace HLEDebug;

public static class Program
{
    private static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        
        Console.WriteLine($"a{StringHelper.InvisibleChar}a");
        Console.WriteLine($"a{StringHelper.ZeroWidthChar}a");
        Console.ReadLine();
    }
}
