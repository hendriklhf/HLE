namespace HLE.Files.Encryption
{
    /// <summary>
    /// A key to represent each <see cref="char"/> and its encryption text.
    /// </summary>
    public class CharKey
    {
        /// <summary>
        /// The code of the encrypted <see cref="char"/>.
        /// </summary>
        public int Char { get; set; }

        /// <summary>
        /// The key of the encrypted <see cref="char"/>.
        /// </summary>
        public string? Key { get; set; }
    }
}
