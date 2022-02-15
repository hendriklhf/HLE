using System;
using HLE.Time;

namespace HLEDebug
{
    public class Program
    {
        private static void Main()
        {
            var t = TimeHelper.GetUnixDifference(TimeHelper.Now() + new Second(45).Milliseconds);
            Console.WriteLine(t.ToString());
            Console.ReadLine();
        }
    }
}
