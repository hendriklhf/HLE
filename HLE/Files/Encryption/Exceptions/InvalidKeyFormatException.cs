using System;

namespace HLE.Files.Encryption.Exceptions
{
    public class InvalidKeyFormatException : Exception
    {
        public override string Message { get; } = "The file of the key is not in the corret format.";

        public InvalidKeyFormatException() : base()
        {
        }

        public InvalidKeyFormatException(string message) : base(message)
        {
        }

        public InvalidKeyFormatException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
