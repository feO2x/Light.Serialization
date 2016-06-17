using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;
using Light.Serialization.Json.TokenParsers;

namespace Light.Serialization.Json.BuilderHelpers
{
    /// <summary>
    ///     Represents an <see cref="IJsonTokenParserFactory" /> that creates transient instances of <see cref="JsonStringInheritanceParser" />.
    /// </summary>
    public sealed class JsonStringInheritanceParserFactory : IJsonTokenParserFactory, ISetJsonStringToPrimitiveParsers
    {
        private List<IJsonStringToPrimitiveParser> _jsonStringToPrimitiveParsers;

        /// <summary>
        ///     Creates a new instance of <see cref="JsonStringInheritanceParserFactory" />.
        /// </summary>
        /// <param name="jsonStringToPrimitiveParsers">The list containing the <see cref="IJsonStringToPrimitiveParser" /> instances that are injected into the created <see cref="JsonStringInheritanceParser" /> objects.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="jsonStringToPrimitiveParsers" /> is null.</exception>
        /// <exception cref="EmptyCollectionException">Thrown when <paramref name="jsonStringToPrimitiveParsers" /> contains no items.</exception>
        public JsonStringInheritanceParserFactory(List<IJsonStringToPrimitiveParser> jsonStringToPrimitiveParsers)
        {
            jsonStringToPrimitiveParsers.MustNotBeNullOrEmpty(nameof(jsonStringToPrimitiveParsers));

            _jsonStringToPrimitiveParsers = jsonStringToPrimitiveParsers;
        }

        /// <summary>
        ///     Gets the type of <see cref="JsonStringInheritanceParser" />.
        /// </summary>
        public Type ParserType => typeof(JsonStringInheritanceParser);

        /// <summary>
        ///     Creates a new transient instance of <see cref="JsonStringInheritanceParser" />.
        /// </summary>
        public IJsonTokenParser Create()
        {
            return new JsonStringInheritanceParser(_jsonStringToPrimitiveParsers);
        }

        /// <summary>
        ///     Gets or sets the list of <see cref="IJsonStringToPrimitiveParser" /> instances.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        /// <exception cref="EmptyCollectionException">Thrown when <paramref name="value" /> contains no items.</exception>
        public List<IJsonStringToPrimitiveParser> JsonStringToPrimitiveParsers
        {
            get { return _jsonStringToPrimitiveParsers; }
            set
            {
                value.MustNotBeNullOrEmpty(nameof(value));
                _jsonStringToPrimitiveParsers = value;
            }
        }
    }
}