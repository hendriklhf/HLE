using HLE.Strings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HLE.Files
{
    public class Decrypter
    {
        public string FilePath { get; }

        private readonly Dictionary<string, char> _encoding = new();

        public Decrypter(string path)
        {
            FilePath = path;
        }

        public void Decrypt()
        {
            string text = File.ReadAllText(FilePath).Remove("\n");
            List<string> encoding = text.Split("€")[1].Split(10);
            for (int i = 0; i <= 20000; i++)
            {
                _encoding.Add(encoding[i], (char)i);
            }
            _encoding.Select(s => s.Key).ToList().ForEach(e => Console.WriteLine(e));
            Console.ReadLine();
            List<string> strings = text.Split("€")[0].Split(10);
            text = string.Empty;
            strings.ForEach(s => text += _encoding[s]);
            File.WriteAllText(FilePath, text);
        }
    }
}
