using System;

namespace HLE.Debug;

public static class Program
{
    private static void Main()
    {
        HString h = "hello";
        h[3] = 'x';
        Console.WriteLine(h);
    }
}
