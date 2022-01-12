using System;
using System.Collections.Generic;
using System.Linq;
using HLE.Collections;

namespace HLEDebug
{
    public class Program
    {
        private static void Main()
        {
            string[] i = GetRank(new string[] { "a", "b", "c" });
            i.ForEach(x => Console.WriteLine(x));
            Console.ReadLine();
        }

        private static readonly List<string> _strings = new() { "a", "a", "c" };

        public static string[] GetRank(string[] name)
        {
            List<string> intersectRanks = _strings.Intersect(name).ToList();
            return intersectRanks.ToArray();
        }
    }
}
