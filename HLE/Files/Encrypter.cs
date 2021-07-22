using HLE.Strings;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Random = HLE.Randoms.Random;

namespace HLE.Files
{
    public class Encrypter
    {
        public string FilePath { get; }

        private readonly Dictionary<char, string> _encoding = new();

        public Encrypter(string path)
        {
            FilePath = path;
            for (int i = 0; i <= 20000; i++)
            {
                string value = Random.String();
                while (_encoding.ContainsValue(value))
                {
                    value = Random.String();
                }
                _encoding.Add((char)i, value);
            }
        }

        public void Encrypt()
        {
            string text = File.ReadAllText(FilePath);
            List<char> chars = text.ToCharArray().ToList();
            text = string.Empty;
            chars.ForEach(c => text += _encoding[c]);
            text += "€";
            _encoding.Select(e => e.Value).ToList().ForEach(v => text += v);
            File.WriteAllLines(FilePath, text.Split(200));
        }
    }
}
