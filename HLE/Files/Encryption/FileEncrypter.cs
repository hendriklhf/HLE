using System.Text;

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
        public string Encrypt(string fileContent)
        {
            StringBuilder builder = new();
            foreach (char c in fileContent)
            {
                builder.Append(_encryptionKey[c]);
            }
            return builder.ToString();
        }
    }
}
