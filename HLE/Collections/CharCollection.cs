using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HLE.Collections
{
    /// <summary>
    /// A class containing collections of chars.
    /// </summary>
    public static class CharCollection
    {
        private static readonly List<char> _alphabetLowerCase = new()
        {
            'a',
            'b',
            'c',
            'd',
            'e',
            'f',
            'g',
            'h',
            'i',
            'j',
            'k',
            'l',
            'm',
            'n',
            'o',
            'p',
            'q',
            'r',
            's',
            't',
            'u',
            'v',
            'w',
            'x',
            'y',
            'z'
        };

        private static readonly List<char> _alphabetUpperCase = new()
        {
            'A',
            'B',
            'C',
            'D',
            'E',
            'F',
            'G',
            'H',
            'I',
            'J',
            'K',
            'L',
            'M',
            'N',
            'O',
            'P',
            'Q',
            'R',
            'S',
            'T',
            'U',
            'V',
            'W',
            'X',
            'Y',
            'Z'
        };

        private static readonly List<char> _everyCharNumber = new()
        {
            '0',
            '1',
            '2',
            '3',
            '4',
            '5',
            '6',
            '7',
            '8',
            '9'
        };

        private static readonly List<char> _specialChars = new()
        {
            '!',
            '"',
            '#',
            '$',
            '%',
            '&',
            '\'',
            '(',
            ')',
            '*',
            '+',
            ',',
            '-',
            '.',
            '/',
            ':',
            ';',
            '<',
            '=',
            '>',
            '?',
            '@',
            '[',
            '\\',
            ']',
            '^',
            '_',
            '`',
            '{',
            '|',
            '}',
            '~'
        };

        /// <summary>
        /// A <see cref="ReadOnlyCollection{Char}"/> of type <see cref="char"/> that contains every letter of the Alphabet in upper and lower case.
        /// </summary>
        public static ReadOnlyCollection<char> Alphabet => new(_alphabetLowerCase.Concat(_alphabetUpperCase).ToList());

        /// <summary>
        /// A <see cref="ReadOnlyCollection{Char}"/> of type <see cref="char"/> that contains every letter of the Alphabet in lower case.
        /// </summary>
        public static ReadOnlyCollection<char> AlphabetLowerCase => new(_alphabetLowerCase);

        /// <summary>
        /// A <see cref="ReadOnlyCollection{Char}"/> of type <see cref="char"/> that contains every letter of the Alphabet in upper case.
        /// </summary>
        public static ReadOnlyCollection<char> AlphabetUpperCase => new(_alphabetUpperCase);

        /// <summary>
        /// A <see cref="ReadOnlyCollection{Char}"/> of type <see cref="char"/> that contains every basic Latin character.<br />
        /// Basically a combination of <see cref="Alphabet"/>, <see cref="CharNumbers"/> and <see cref="SpecialChars"/>.
        /// </summary>
        public static ReadOnlyCollection<char> BasicLatinChars =>
            new(_alphabetLowerCase
                .Concat(_alphabetUpperCase)
                .Concat(_specialChars)
                .Concat(_everyCharNumber)
                .ToList());

        /// <summary>
        /// A <see cref="ReadOnlyCollection{Char}"/> of type <see cref="char"/> that contains every number from 0 to 9.
        /// </summary>
        public static ReadOnlyCollection<char> CharNumbers => new(_everyCharNumber);

        /// <summary>
        /// A <see cref="ReadOnlyCollection{Char}"/> of type <see cref="char"/> that contains all basic Latin special characters.
        /// </summary>
        public static ReadOnlyCollection<char> SpecialChars => new(_specialChars);
    }
}
