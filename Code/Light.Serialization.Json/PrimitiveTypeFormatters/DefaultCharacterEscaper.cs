using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.Serialization.Json.PrimitiveTypeFormatters
{
    // Many thanks to JSON.NET (https://github.com/JamesNK/Newtonsoft.Json). I would have never figured out how to do this without it.

    /// <summary>
    ///     Represents an <see cref="ICharacterEscaper" /> that escapes .NET characters if
    ///     they are a quotation mark '"', a back-slash "\", a line separator "\u2028", a paragraph separator "\u2029",
    ///     or belong to the Unicode C0 or C1 block. This default list of characters can be modified during construction.
    /// </summary>
    public sealed class DefaultCharacterEscaper : ICharacterEscaper
    {
        private const char X000F = '\x000f';
        private readonly List<char> _escapedCharacters;

        /// <summary>
        ///     Creates a new instance of <see cref="DefaultCharacterEscaper" /> with the default list of escaped characters.
        /// </summary>
        public DefaultCharacterEscaper()
        {
            _escapedCharacters = CreateDefaultEscapedCharacters();
        }

        /// <summary>
        ///     Creates a new instance of <see cref="DefaultCharacterEscaper" />.
        /// </summary>
        /// <param name="escapedCharacters">The list of .NET characters that should be escaped. You can use <see cref="CreateDefaultEscapedCharacters" /> to create the default list.</param>
        public DefaultCharacterEscaper(List<char> escapedCharacters)
        {
            escapedCharacters.MustNotBeNull(nameof(escapedCharacters));

            _escapedCharacters = escapedCharacters;
        }

        public char[] Escape(char character)
        {
            if (_escapedCharacters.Contains(character) == false)
                return null;

            // The following characters are escaped in a special way
            switch (character)
            {
                case '\t':
                    return new[] { '\\', 't' };
                case '\n':
                    return new[] { '\\', 'n' };
                case '\r':
                    return new[] { '\\', 'r' };
                case '\f':
                    return new[] { '\\', 'f' };
                case '\b':
                    return new[] { '\\', 'b' };
                case '\\':
                    return new[] { '\\', '\\' };
                case '"':
                    return new[] { '\\', '"' };
            }

            // All other characters will be represented in the "\uxxxx" format
            var buffer = new[]
                         {
                             '\\',
                             'u',
                             ConvertIntToChar((character >> 12) & X000F),
                             ConvertIntToChar((character >> 8) & X000F),
                             ConvertIntToChar((character >> 4) & X000F),
                             ConvertIntToChar(character & X000F)
                         };
            return buffer;
        }

        /// <summary>
        ///     Creates a new list containing the default set of characters that must be escaped:
        ///     a quotation mark '"', a back-slash "\", a line separator "\u2028", a paragraph separator "\u2029",
        ///     and all characters from the Unicode C0 or C1 block.
        /// </summary>
        public static List<char> CreateDefaultEscapedCharacters()
        {
            var escapedCharacters = new List<char> { '"', '\\' };

            // Unicode C0 block
            for (var i = 0; i < ' '; i++)
            {
                escapedCharacters.Add((char) i);
            }

            // Unicode C1 block
            for (int i = '\u007f'; i <= '\u009f'; i++)
            {
                escapedCharacters.Add((char) i);
            }

            // Unicode Line Separator and Paragraph Seperator
            escapedCharacters.Add('\u2028');
            escapedCharacters.Add('\u2029');

            return escapedCharacters;
        }

        private static char ConvertIntToChar(int value)
        {
            if (value <= 9)
                return (char) (value + 48);

            return (char) (value + 87);
        }
    }
}