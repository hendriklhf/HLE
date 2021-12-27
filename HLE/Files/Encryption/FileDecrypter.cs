using System.Text;
using HLE.Collections;
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
        /// Decrypts the given file content.
        /// </summary>
        /// <param name="path">The file content that will be decrypted.</param>
        public string Decrypt(string fileContent)
        {
            StringBuilder builder = new();
            fileContent.Split(10).ForEach(str =>
            {
                builder.Append(_encryptionKey[str]);
            });
            return builder.ToString();
        }
    }
}
