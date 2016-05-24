using System;
using Light.Serialization.Json.FrameworkExtensions;

namespace Light.Serialization.Json.ComplexTypeConstruction
{
    /// <summary>
    ///     Represents a name normalizer that lowers all characters and removes all special characters from a string.
    /// </summary>
    public sealed class ToLowerWithoutSpecialCharactersNormalizer : INameNormalizer
    {
        /// <summary>
        ///     Normalizes the given string by removing all special characters and lowering the remaining ones.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name" /> is null.</exception>
        public string Normalize(string name)
        {
            return name.ToLowerAndRemoveAllSpecialCharacters();
        }
    }
}