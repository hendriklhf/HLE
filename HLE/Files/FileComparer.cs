using System.IO;

namespace HLE.Files
{
    /// <summary>
    /// A class that determines whether two file are equal by comparing their size in bytes.
    /// </summary>
    public class FileComparer
    {
        /// <summary>
        /// The path to the first file.
        /// </summary>
        public string FilePathOne { get; set; }

        /// <summary>
        /// The path to the second file.
        /// </summary>
        public string FilePathTwo { get; set; }

        /// <summary>
        /// True, if the files are equal, otherwise false.
        /// </summary>
        public bool AreEqual => new FileStream(FilePathOne, FileMode.Open).Length == new FileStream(FilePathTwo, FileMode.Open).Length;

        /// <summary>
        /// The basic constructor for <see cref="FileComparer"/>
        /// </summary>
        /// <param name="pathOne">The first path.</param>
        /// <param name="pathTwo">The second path.</param>
        public FileComparer(string pathOne, string pathTwo)
        {
            FilePathOne = pathOne;
            FilePathTwo = pathTwo;
        }
    }
}
