using System;
using System.IO;
using System.Text;
using Light.GuardClauses;
using Light.Serialization.Json.BuilderHelpers;

namespace Light.Serialization.Json.LowLevelWriting
{
    /// <summary>
    ///     Represents the factory that creates the default JSON Writer and manages all corresponding disposables.
    /// </summary>
    public sealed class JsonWriterFactory : IJsonWriterFactory, ISetWhitespaceFormatterCreationDelegate, ISetKeyNormalizer
    {
        private Func<IJsonWhitespaceFormatter> _createWhitespaceFormatter;
        private IJsonKeyNormalizer _keyNormalizer;

        /// <summary>
        ///     Creates a new instance of JsonWriterFactory.
        /// </summary>
        /// <param name="keyNormalizer">The key normalizer that will be injected into the JsonWriter.</param>
        /// <param name="createWhitespaceFormatter">The whitespace formatter that will be injected into the JsonWriter.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public JsonWriterFactory(IJsonKeyNormalizer keyNormalizer, Func<IJsonWhitespaceFormatter> createWhitespaceFormatter)
        {
            keyNormalizer.MustNotBeNull(nameof(keyNormalizer));
            createWhitespaceFormatter.MustNotBeNull(nameof(createWhitespaceFormatter));

            _keyNormalizer = keyNormalizer;
            _createWhitespaceFormatter = createWhitespaceFormatter;
        }

        /// <summary>
        ///     Creates a new <see cref="JsonWriter" /> instance by encapsulating the specified string builder in a <see cref="StringWriter" /> instance.
        /// </summary>
        /// <param name="builder">The builder that the JSON writer will write to.</param>
        /// <returns>The new <see cref="JsonWriter" /> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> is null.</exception>
        public IJsonWriter CreateFromStringBuilder(StringBuilder builder)
        {
            return new JsonWriter(new StringWriter(builder), _createWhitespaceFormatter(), _keyNormalizer);
        }

        /// <summary>
        ///     Creates a new <see cref="JsonWriter" /> instance using the specified text writer.
        /// </summary>
        /// <param name="writer">The text writer that the JSON writer will use to write the document.</param>
        /// <returns>The new <see cref="JsonWriter" /> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer" /> is null.</exception>
        public IJsonWriter CreateFromTextWriter(TextWriter writer)
        {
            return new JsonWriter(writer, _createWhitespaceFormatter(), _keyNormalizer);
        }

        /// <summary>
        ///     Gets or sets the key normalizer that is injected into the <see cref="JsonWriter" />.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        public IJsonKeyNormalizer KeyNormalizer
        {
            get { return _keyNormalizer; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _keyNormalizer = value;
            }
        }

        /// <summary>
        ///     Gets or Sets the delegate that creates an <see cref="IJsonWhitespaceFormatter" /> instance.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        public Func<IJsonWhitespaceFormatter> CreateWhitespaceFormatter
        {
            get { return _createWhitespaceFormatter; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _createWhitespaceFormatter = value;
            }
        }

        /// <summary>
        ///     Creates a new <see cref="JsonWriterFactory" /> instance with the default normalizer (for JSON keys with lowerCamelCase style and without special characters)
        ///     and the default whitespace formatter (no whitespace to keep the document small).
        /// </summary>
        public static JsonWriterFactory CreateDefault()
        {
            return new JsonWriterFactory(new FirstCharacterToLowerAndRemoveAllSpecialCharactersNormalizer(), WhitespaceFormatterNullObject.Create);
        }

        /// <summary>
        ///     Creates a new <see cref="JsonWriterFactory" /> instance with the default normalizer (for JSON keys with lowerCamelCase style and without special characters)
        ///     and an <see cref="IndentingWhitespaceFormatter" /> for human-readable JSON documents with indenting.
        /// </summary>
        public static JsonWriterFactory CreateDefaultWithIndentingWhitespaceFormatter()
        {
            return new JsonWriterFactory(new FirstCharacterToLowerAndRemoveAllSpecialCharactersNormalizer(), IndentingWhitespaceFormatter.Create);
        }
    }
}