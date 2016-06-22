using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Light.GuardClauses;
using Light.Serialization.Abstractions;
using Light.Serialization.Json.Caching;
using Light.Serialization.Json.LowLevelReading;
using Light.Serialization.Json.ObjectMetadata;
using Light.Serialization.Json.TokenParsers;

namespace Light.Serialization.Json
{
    /// <summary>
    ///     Represents the default <see cref="IDeserializer" /> implementation for JSON documents that uses
    ///     a recursive algorithm incorporating <see cref="IJsonTokenParser" /> instances. Each <see cref="IJsonTokenParser" />
    ///     is designed for a specific JSON token and requested type situation and knows how to instruct a <see cref="IJsonReader" />
    ///     to progress through the JSON document.
    /// </summary>
    public sealed class JsonDeserializer : IDeserializer, IRecursiveDeserializer
    {
        private readonly Dictionary<JsonTokenTypeCombination, IJsonTokenParser> _cache;
        private readonly IJsonReaderFactory _jsonReaderFactory;
        private readonly List<ISwitchParserForComplexObject> _switchableParsersForComplexObjects;
        private readonly IReadOnlyList<IJsonTokenParser> _tokenParsers;
        private IJsonReader _jsonReader;
        private ObjectReferencePreserver _objectReferencePreserver;

        /// <summary>
        ///     Creates a new intance of <see cref="JsonDeserializer" />.
        /// </summary>
        /// <param name="jsonReaderFactory">The factory that creates JSON readers.</param>
        /// <param name="tokenParsers">The token parsers that can deserialize different JSON tokens.</param>
        /// <param name="cache">The cache for JSON token parsers for fast access.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public JsonDeserializer(IJsonReaderFactory jsonReaderFactory,
                                IReadOnlyList<IJsonTokenParser> tokenParsers,
                                Dictionary<JsonTokenTypeCombination, IJsonTokenParser> cache)
        {
            jsonReaderFactory.MustNotBeNull(nameof(jsonReaderFactory));
            tokenParsers.MustNotBeNull(nameof(tokenParsers));
            cache.MustNotBeNull(nameof(cache));

            _jsonReaderFactory = jsonReaderFactory;
            _tokenParsers = tokenParsers;
            _cache = cache;
            _switchableParsersForComplexObjects = _tokenParsers.OfType<ISwitchParserForComplexObject>().ToList();
        }

        /// <summary>
        ///     Deserializes the given JSON document to the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the object graph root.</typeparam>
        /// <param name="json">The JSON document to be deserialized.</param>
        /// <returns>The deserialized JSON document.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json" /> is null.</exception>
        /// <exception cref="DeserializationException">Thrown when the specified document could not be deserialized to the requested object graph.</exception>
        public T Deserialize<T>(string json)
        {
            var result = Deserialize(json, typeof(T));
            return (T) result;
        }

        /// <summary>
        ///     Deserializes the given JSON document to the specified type.
        /// </summary>
        /// <param name="json">The JSON document to be deserialized.</param>
        /// <param name="requestedType">The type of the object graph root.</param>
        /// <returns>The deserialized JSON document.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json" /> or <paramref name="requestedType" /> is null.</exception>
        /// <exception cref="DeserializationException">Thrown when the specified document could not be deserialized to the requested object graph.</exception>
        public object Deserialize(string json, Type requestedType)
        {
            json.MustNotBeNull(nameof(json));
            requestedType.MustNotBeNull(nameof(requestedType));

            _jsonReader = _jsonReaderFactory.CreateFromString(json);
            _objectReferencePreserver = new ObjectReferencePreserver();
            var returnValue = DeserializeDocument(requestedType);
            _jsonReader = null;
            _objectReferencePreserver = null;
            return returnValue;
        }

        /// <summary>
        ///     Deserializes the given JSON stream encapsulated by the <paramref name="textReader" /> to the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the object graph root.</typeparam>
        /// <param name="textReader">The text reader that encapsulates the JSON stream.</param>
        /// <returns>The deserialized object graph.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="textReader" /> is null.</exception>
        /// <exception cref="DeserializationException">Thrown when the specified document could not be deserialized to the requested object graph.</exception>
        public T Deserialize<T>(TextReader textReader)
        {
            return (T) Deserialize(textReader, typeof(T));
        }

        /// <summary>
        ///     Deserializes the given JSON stream encapsulated by the <paramref name="textReader" /> to the specified type.
        /// </summary>
        /// <param name="textReader">The text reader that encapsulates the JSON stream.</param>
        /// <param name="requestedType">The type of the object graph root.</param>
        /// <returns>The deserialized object graph.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="textReader" /> or <paramref name="requestedType" /> is null.</exception>
        /// <exception cref="DeserializationException">Thrown when the specified document could not be deserialized to the requested object graph.</exception>
        public object Deserialize(TextReader textReader, Type requestedType)
        {
            textReader.MustNotBeNull(nameof(textReader));
            requestedType.MustNotBeNull(nameof(requestedType));

            _jsonReader = _jsonReaderFactory.CreateFromTextReader(textReader);
            _objectReferencePreserver = new ObjectReferencePreserver();
            var returnValue = DeserializeDocument(requestedType);
            _jsonReader.Dispose();
            _jsonReader = null;
            _objectReferencePreserver = null;
            return returnValue;
        }

        /// <summary>
        ///     Deserializes the given JSON stream encapsulated by the <paramref name="binaryReader" /> to the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the object graph root.</typeparam>
        /// <param name="binaryReader">The binary reader that encapsulates the JSON stream.</param>
        /// <returns>The deserialized object graph.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="binaryReader" /> is null.</exception>
        /// <exception cref="DeserializationException">Thrown when the specified document could not be deserialized to the requested object graph.</exception>
        public T Deserialize<T>(BinaryReader binaryReader)
        {
            return (T) Deserialize(binaryReader, typeof(T));
        }

        /// <summary>
        ///     Deserializes the given JSON stream encapsulated by the <paramref name="binaryReader" /> to the specified type.
        /// </summary>
        /// <param name="binaryReader">The text reader that encapsulates the JSON stream.</param>
        /// <param name="requestedType">The type of the object graph root.</param>
        /// <returns>The deserialized object graph.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="binaryReader" /> or <paramref name="requestedType" /> is null.</exception>
        /// <exception cref="DeserializationException">Thrown when the specified document could not be deserialized to the requested object graph.</exception>
        public object Deserialize(BinaryReader binaryReader, Type requestedType)
        {
            binaryReader.MustNotBeNull(nameof(binaryReader));
            requestedType.MustNotBeNull(nameof(requestedType));

            _jsonReader = _jsonReaderFactory.CreateFromBinaryReader(binaryReader);
            _objectReferencePreserver = new ObjectReferencePreserver();
            var returnValue = DeserializeDocument(requestedType);
            _jsonReader.Dispose();
            _jsonReader = null;
            _objectReferencePreserver = null;
            return returnValue;
        }

        ParseResult IRecursiveDeserializer.DeserializeToken(JsonToken token, Type requestedType)
        {
            return DeserializeJsonToken(token, requestedType);
        }

        ISwitchParserForComplexObject IRecursiveDeserializer.FindParserForType(Type typeToBeConstructed)
        {
            foreach (var switchableParser in _switchableParsersForComplexObjects)
            {
                if (switchableParser.ShouldDeserialize(typeToBeConstructed))
                    return switchableParser;
            }

            return null;
        }

        private object DeserializeDocument(Type requestedType)
        {
            var token = _jsonReader.ReadNextToken();
            var parseResult = DeserializeJsonToken(token, requestedType);
            parseResult.IsDeferredReference.MustBeFalse(exception: () => new DeserializationException("The specified JSON document contains only a deferred reference."));

            return parseResult.ParsedValue;
        }

        private ParseResult DeserializeJsonToken(JsonToken token, Type requestedType)
        {
            IJsonTokenParser parser;
            var tokenTypeCombination = new JsonTokenTypeCombination(token.JsonType, requestedType);
            var deserializationContext = new JsonDeserializationContext(token, requestedType, _jsonReader, this, _objectReferencePreserver);

            lock (_cache)
            {
                if (_cache.TryGetValue(tokenTypeCombination, out parser) == false)
                {
                    foreach (var tokenParser in _tokenParsers)
                    {
                        if (tokenParser.IsSuitableFor(deserializationContext) == false)
                            continue;

                        parser = tokenParser;
                        goto CacheParserIfNecessary;
                    }

                    throw new DeserializationException($"Cannot deserialize value {token} with requested type {requestedType.FullName} because there is no parser that is suitable for this context.");

                    CacheParserIfNecessary:
                    if (parser.CanBeCached)
                        _cache.Add(tokenTypeCombination, parser);
                }
            }

            return parser.ParseValue(deserializationContext);
        }
    }
}