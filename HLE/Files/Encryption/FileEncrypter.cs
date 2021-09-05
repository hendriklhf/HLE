using System.IO;

namespace HLE.Files.Encryption
{
    /// <summary>
    /// A basic, not very safe text encrypter.
    /// </summary>
    public class FileEncrypter
    {
        private readonly EncryptionKey _encryptionKey;

        /// <summary>
        /// The basic constructor for <see cref="FileEncrypter"/>.
        /// </summary>
        /// <param name="key">The encryption key.</param>
        public FileEncrypter(EncryptionKey key)
        {
            _encryptionKey = key;
        }

        /// <summary>
        /// Encrypts the file at the given path.
        /// </summary>
        /// <param name="path">The path of the file that will be encrypted.</param>
        public void Encrypt(string path)
        {
            string fileContent = File.ReadAllText(path);
            string result = string.Empty;
            foreach (char c in fileContent)
            {
                result += _encryptionKey[c];
            }
            File.WriteAllText(path, result);
        }
    }
}
