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
        private StringBuilder _stringBuilder;
        private StringWriter _stringWriter;

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
        ///     Creates a new instance of JsonWriter using a StringBuilder and StringWriter.
        /// </summary>
        /// <returns>The initialized JsonWriter.</returns>
        /// <exception cref="InvalidOperationException">Thrown when a previous call to Create was not finished with <see cref="FinishWriteProcessAndReleaseResources" />.</exception>
        public IJsonWriter Create()
        {
            Check.That(_stringBuilder == null,
                       () => new InvalidOperationException("You cannot call Create before releasing the objects of a previous Create call."));

            _stringBuilder = new StringBuilder();
            _stringWriter = new StringWriter(_stringBuilder);
            _whitespaceFormatter.ResetIndentationLevel();
            IJsonWriter returnValue = new JsonWriter(_stringWriter, _whitespaceFormatter, _keyNormalizer);
            return returnValue;
        }

        /// <summary>
        ///     Releases the StringBuilder and the StringWriter that were created for the JsonWriter and returns the resulting JSON document.
        /// </summary>
        /// <returns>The JSON document as a string.</returns>
        public string FinishWriteProcessAndReleaseResources()
        {
            Check.Against(_stringBuilder == null,
                          () => new InvalidOperationException("FinishWriteProcessAndReleaseResources must be called after Create."));

            // ReSharper disable once PossibleNullReferenceException
            var returnValue = _stringBuilder.ToString();
            _stringWriter = null;
            _stringBuilder = null;
            return returnValue;
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