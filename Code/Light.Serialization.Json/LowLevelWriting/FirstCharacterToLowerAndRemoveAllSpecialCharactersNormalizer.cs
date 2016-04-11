using System;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;
using Light.Serialization.Abstractions;
using Light.Serialization.Json.FrameworkExtensions;

namespace Light.Serialization.Json.LowLevelWriting
{
    /// <summary>
    ///     Represents a JSON key normalizer that removes all special characters and makes the first character lowercase if necessary.
    /// </summary>
    public sealed class FirstCharacterToLowerAndRemoveAllSpecialCharactersNormalizer : IJsonKeyNormalizer
    {
        /// <summary>
        ///     Normalizes the specified key if necessary. All special characters will be removed and the first character will be made lowercase.
        /// </summary>
        /// <param name="key">The key to be normalized.</param>
        /// <returns>The normalized key.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key" /> is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="key" /> is an empty string.</exception>
        public string Normalize(string key)
        {
            key.MustNotBeNullOrEmpty(nameof(key));

            var i = 0;
            char character;
            for (; i < key.Length; i++)
            {
                character = key[i];
                if (char.IsLetterOrDigit(character) == false)
                    goto NormalizeString;
            }

            return key.MakeFirstCharacterLowercaseIfNecessary();

            // This section is only used when a new string has to be created because the old one contains special or uppercase characters
            // Otherwise, the passed in string is returned (to minimize the creation of objects - your GC will thank you).
            NormalizeString:
            var numberOfSpecialCharacters = 0;

            for (; i < key.Length; i++)
            {
                if (char.IsLetterOrDigit(key[i]) == false)
                    numberOfSpecialCharacters++;
            }

            if (numberOfSpecialCharacters == key.Length)
                throw new SerializationException($"The specified key \"{key}\" contains only special characters that cannot be normalized.");

            var charArray = new char[key.Length - numberOfSpecialCharacters];
            var charArrayIndex = 0;

            for (i = 0; i < key.Length; i++)
            {
                character = key[i];
                if (char.IsLetterOrDigit(character) == false)
                    continue;

                charArray[charArrayIndex] = character;
                charArrayIndex++;
            }

            if (char.IsLower(charArray[0]))
                return new string(charArray);

            charArray[0] = char.ToLowerInvariant(charArray[0]);

            return new string(charArray);
        }
    }
}