using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using HLE.Files.Encryption.Exceptions;

namespace HLE.Files.Encryption
{
    /// <summary>
    /// A basic, not very safe encryption key to encrypt files with <see cref="FileEncrypter"/>.
    /// </summary>
    public class EncryptionKey
    {
        private readonly List<CharKey> _charKeys;
        private readonly string _path;

        /// <summary>
        /// The basic constructor of <see cref="EncryptionKey"/>.
        /// </summary>
        /// <param name="path">The path of the key.</param>
        public EncryptionKey(string path)
        {
            _path = path;
            try
            {
                _charKeys = JsonSerializer.Deserialize<List<CharKey>>(File.ReadAllText(_path));
            }
            catch (Exception ex)
            {
                throw new InvalidKeyFormatException("The file of the key is not in the right format.", ex);
            }
        }

        /// <summary>
        /// Return the encrypted char by key.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <returns>The encrypted char by key.</returns>
        public char this[string key] => (char)_charKeys.FirstOrDefault(ck => ck.Key == key).Char;

        /// <summary>
        /// Returns the encryption by <see cref="char"/>.
        /// </summary>
        /// <param name="c">A char.</param>
        /// <returns>The encryption by <see cref="char"/>.</returns>
        public string this[char c] => _charKeys.FirstOrDefault(ck => ck.Char == c).Key;
    }
}
