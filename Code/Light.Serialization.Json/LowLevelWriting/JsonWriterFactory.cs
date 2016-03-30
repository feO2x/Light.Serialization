using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Light.GuardClauses;

namespace Light.Serialization.Json.LowLevelWriting
{
    /// <summary>
    ///     Represents the factory that creates the default <see cref="JsonWriter" /> and manages all corresponding disposables.
    /// </summary>
    public sealed class JsonWriterFactory : IJsonWriterFactory
    {
        private readonly List<Func<IJsonWriter, IJsonWriter>> _decorateFunctions = new List<Func<IJsonWriter, IJsonWriter>>();
        private IJsonWhitespaceFormatter _jsonWhitespaceFormatter = new WhitespaceFormatterNullObject();
        private IJsonKeyNormalizer _keyNormalizer = new FirstCharacterToLowerAndRemoveAllSpecialCharactersNormalizer();
        private StringBuilder _stringBuilder;
        private StringWriter _stringWriter;

        /// <summary>
        ///     Gets or sets the Whitespace Formatter used to format the document. This value defaults to a <see cref="WhitespaceFormatterNullObject" /> instance.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        public IJsonWhitespaceFormatter JsonWhitespaceFormatter
        {
            get { return _jsonWhitespaceFormatter; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _jsonWhitespaceFormatter = value;
            }
        }

        /// <summary>
        ///     Gets or sets the normalizer that is used to format keys in complex JSON objects. This values defaults to a <see cref="FirstCharacterToLowerAndRemoveAllSpecialCharactersNormalizer" /> instance.
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
        ///     Creates a new instance of <see cref="JsonWriter" /> using a <see cref="StringBuilder" /> and <see cref="StringWriter" />.
        /// </summary>
        /// <returns>The initialized <see cref="JsonWriter" />.</returns>
        public IJsonWriter Create()
        {
            _stringBuilder = new StringBuilder();
            _stringWriter = new StringWriter(_stringBuilder);
            IJsonWriter returnValue = new JsonWriter(_stringWriter, _jsonWhitespaceFormatter, _keyNormalizer);
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var decorateFunction in _decorateFunctions)
            {
                returnValue = decorateFunction(returnValue);
            }
            return returnValue;
        }

        /// <summary>
        ///     Releases the <see cref="StringBuilder" /> and <see cref="StringWriter" /> that were created for the <see cref="JsonWriter" /> and returns the resulting JSON document.
        /// </summary>
        /// <returns>The JSON document as a string.</returns>
        public string FinishWriteProcessAndReleaseResources()
        {
            var returnValue = _stringBuilder.ToString();
            _stringWriter = null;
            _stringBuilder = null;
            return returnValue;
        }

        /// <summary>
        ///     Registers the specified delegate with this factory that is called during <see cref="Create" />. The delegate should then decorate the <see cref="JsonWriter" />.
        /// </summary>
        /// <param name="decoratorFunction">The delegate that decorates the <see cref="JsonWriter" /> with another object.</param>
        /// <returns>The factory for method-chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="decoratorFunction" /> is null.</exception>
        public JsonWriterFactory DecorateCreationWith(Func<IJsonWriter, IJsonWriter> decoratorFunction)
        {
            decoratorFunction.MustNotBeNull(nameof(decoratorFunction));

            _decorateFunctions.Add(decoratorFunction);
            return this;
        }
    }
}