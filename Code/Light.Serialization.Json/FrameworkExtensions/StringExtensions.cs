using System;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;
using Light.Serialization.Abstractions;

namespace Light.Serialization.Json.FrameworkExtensions
{
    /// <summary>
    ///     Provides extension methods for strings.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        ///     Surrounds the string with the specified values.
        /// </summary>
        /// <param name="string">The string that will be surrounded by <paramref name="value" />.</param>
        /// <param name="value">The value that will be inserted at the beginning and end of <paramref name="string" />.</param>
        /// <returns>The new string instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="value" /> is empty.</exception>
        public static string SurroundWith(this string @string, string value)
        {
            @string.MustNotBeNull(nameof(@string));
            value.MustNotBeNullOrEmpty(nameof(value));

            var charBuffer = new char[@string.Length + 2 * value.Length];
            for (var i = 0; i < value.Length; i++)
            {
                var characterToBeCopied = value[i];
                charBuffer[i] = characterToBeCopied;
                charBuffer[i + @string.Length + value.Length] = characterToBeCopied;
            }

            for (var i = 0; i < @string.Length; i++)
            {
                charBuffer[i + value.Length] = @string[i];
            }
            return new string(charBuffer);
        }

        /// <summary>
        ///     Surrounds the string with the specified character.
        /// </summary>
        /// <param name="string">The string that will be surrounded by <paramref name="character" />.</param>
        /// <param name="character">The character that will be inserted at the beginning and end of <paramref name="string" />.</param>
        /// <returns>The new string instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="string" /> is null.</exception>
        public static string SurroundWith(this string @string, char character)
        {
            @string.MustNotBeNull(nameof(@string));

            var characterBuffer = new char[@string.Length + 2];
            characterBuffer[0] = characterBuffer[characterBuffer.Length - 1] = character;
            for (var i = 0; i < @string.Length; i++)
            {
                characterBuffer[i + 1] = @string[i];
            }
            return new string(characterBuffer);
        }

        /// <summary>
        ///     Surrounds the specified string with quotation marks (").
        /// </summary>
        /// <param name="string">The string that will be surrounded.</param>
        /// <returns>The new string instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="string" /> is null.</exception>
        public static string SurroundWithQuotationMarks(this string @string)
        {
            return @string.SurroundWith('"');
        }

        /// <summary>
        ///     Surrounds the specified string with parantheses "()".
        /// </summary>
        /// <param name="string">The string that will be surrounded.</param>
        /// <returns>The new string instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="string" /> is null.</exception>
        public static string SurroundWithParantheses(this string @string)
        {
            @string.MustNotBeNull(nameof(@string));

            var characterBuffer = new char[@string.Length + 2];
            characterBuffer[0] = '(';
            characterBuffer[characterBuffer.Length - 1] = ')';
            for (var i = 0; i < @string.Length; i++)
            {
                characterBuffer[i + 1] = @string[i];
            }
            return new string(characterBuffer);
        }

        /// <summary>
        ///     Checks if the specified string is surrounded by quotation marks (at first and last position).
        /// </summary>
        /// <param name="string">The string to be checked.</param>
        /// <returns>True if the first and last position of the string are the '"' character, else false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="string" /> is null.</exception>
        public static bool IsSurroundedByQuotationMarks(this string @string)
        {
            @string.MustNotBeNull(nameof(@string));

            if (@string.Length <= 1)
                return false;
            return @string[0] == '"' && @string[@string.Length - 1] == '"';
        }

        /// <summary>
        ///     Makes the first character of the specified string lowercase, if necessary.
        /// </summary>
        /// <param name="string">The string to be checked.</param>
        /// <returns>The specified string instance if its first character is a lowercase character, else a new string instance where the first character is lowercase.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="string" /> is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="string" /> is an empty string.</exception>
        public static string MakeFirstCharacterLowercaseIfNecessary(this string @string)
        {
            @string.MustNotBeNullOrEmpty(nameof(@string));

            if (char.IsLower(@string[0]))
                return @string;

            return char.ToLowerInvariant(@string[0]) + @string.Substring(1);
        }

        /// <summary>
        ///     Removes all special characters and lowers the remaining ones.
        /// </summary>
        /// <param name="string">The string to be checked.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="string" /> is null.</exception>
        public static string ToLowerAndRemoveAllSpecialCharacters(this string @string)
        {
            @string.MustNotBeNull(nameof(@string));

            int i;
            char character;
            for (i = 0; i < @string.Length; i++)
            {
                character = @string[i];
                if (char.IsLetterOrDigit(character) == false || char.IsLower(character) == false)
                    goto NormalizeString;
            }

            return @string;

            // This section is only used when a new string has to be created because the old one contains special or uppercase characters
            // Otherwise, the passed in string is returned (to minimize the creation of objects - your GC will thank you).
            NormalizeString:
            var numberOfSpecialCharacters = 0;

            for (; i < @string.Length; i++)
            {
                if (char.IsLetterOrDigit(@string[i]) == false)
                    numberOfSpecialCharacters++;
            }

            if (numberOfSpecialCharacters == @string.Length)
                throw new DeserializationException($"The specified name {@string} contains only special characters that cannot be normalized.");

            var charArray = new char[@string.Length - numberOfSpecialCharacters];

            for (i = 0; i < @string.Length; i++)
            {
                character = @string[i];
                if (char.IsLetterOrDigit(character) == false)
                    continue;

                charArray[i] = char.ToLower(character);
            }

            return new string(charArray);
        }
    }
}