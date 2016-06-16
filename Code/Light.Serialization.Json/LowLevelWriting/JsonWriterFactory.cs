using System;
using System.IO;
using System.Text;
using Light.GuardClauses;

namespace Light.Serialization.Json.LowLevelWriting
{
    /// <summary>
    ///     Represents the factory that creates the default JSON Writer and manages all corresponding disposables.
    /// </summary>
    public sealed class JsonWriterFactory : IJsonWriterFactory
    {
        private readonly IJsonKeyNormalizer _keyNormalizer;
        private readonly IJsonWhitespaceFormatter _whitespaceFormatter;

        /// <summary>
        ///     Creates a new instance of JsonWriterFactory.
        /// </summary>
        /// <param name="keyNormalizer">The key normalizer that will be injected into the JsonWriter.</param>
        /// <param name="whitespaceFormatter">The whitespace formatter that will be injected into the JsonWriter.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public JsonWriterFactory(IJsonKeyNormalizer keyNormalizer, IJsonWhitespaceFormatter whitespaceFormatter)
        {
            keyNormalizer.MustNotBeNull(nameof(keyNormalizer));
            whitespaceFormatter.MustNotBeNull(nameof(whitespaceFormatter));

            _keyNormalizer = keyNormalizer;
            _whitespaceFormatter = whitespaceFormatter;
        }

        /// <summary>
        ///     Creates a new <see cref="JsonWriter" /> instance by encapsulating the specified string builder in a <see cref="StringWriter" /> instance.
        /// </summary>
        /// <param name="builder">The builder that the JSON writer will write to.</param>
        /// <returns>The new <see cref="JsonWriter" /> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> is null.</exception>
        public IJsonWriter CreateFromStringBuilder(StringBuilder builder)
        {
            return new JsonWriter(new StringWriter(builder), _whitespaceFormatter, _keyNormalizer);
        }

        /// <summary>
        ///     Creates a new <see cref="JsonWriter" /> instance using the specified text writer.
        /// </summary>
        /// <param name="writer">The text writer that the JSON writer will use to write the document.</param>
        /// <returns>The new <see cref="JsonWriter" /> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer" /> is null.</exception>
        public IJsonWriter CreateFromTextWriter(TextWriter writer)
        {
            return new JsonWriter(writer, _whitespaceFormatter, _keyNormalizer);
        }

        /// <summary>
        ///     Creates a new JsonWriterFactory instance with the default normalizer (for JSON keys with lowerCamelCase style and without special characters)
        ///     and the default whitespace formatter (no whitespace to keep the document small).
        /// </summary>
        public static JsonWriterFactory CreateDefault()
        {
            return new JsonWriterFactory(new FirstCharacterToLowerAndRemoveAllSpecialCharactersNormalizer(), new WhitespaceFormatterNullObject());
        }

        /// <summary>
        ///     Creates a new JsonWriterFactory instance with the default normalizer (for JSON keys with lowerCamelCase style and without special characters)
        ///     and an IndentingWhitespaceFormatter for human-readable JSON documents with indenting.
        /// </summary>
        public static JsonWriterFactory CreateDefaultWithIndentingWhitespaceFormatter()
        {
            return new JsonWriterFactory(new FirstCharacterToLowerAndRemoveAllSpecialCharactersNormalizer(), new IndentingWhitespaceFormatter());
        }
    }
}