using System.IO;
using HLE.Strings;

namespace HLE.Files.Encryption
{
    /// <summary>
    /// A basic file decrypter to decrypt files encrypted by the <see cref="FileEncrypter"/>.
    /// </summary>
    public class FileDecrypter
    {
        private readonly EncryptionKey _encryptionKey;

        /// <summary>
        /// The basic constructor for <see cref="FileDecrypter"/>.
        /// </summary>
        /// <param name="key"></param>
        public FileDecrypter(EncryptionKey key)
        {
            _encryptionKey = key;
        }

        /// <summary>
        /// Decrypts the file at the given path.
        /// </summary>
        /// <param name="path">The path of the file that will be decrypted.</param>
        public void Decrypt(string path)
        {
            string fileContent = File.ReadAllText(path);
            string result = string.Empty;
            fileContent.Split(10).ForEach(str =>
            {
                result += _encryptionKey[str];
            });
            File.WriteAllText(path, result);
        }
    }
}
