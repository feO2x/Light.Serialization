using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.Serialization.Abstractions;
using Light.Serialization.Json.Caching;
using Light.Serialization.Json.LowLevelReading;
using Light.Serialization.Json.TokenParsers;

namespace Light.Serialization.Json
{
    /// <summary>
    ///     Represents an object that can deserialize JSON documents.
    /// </summary>
    public sealed class JsonDeserializer : IDeserializer
    {
        private readonly Dictionary<JsonTokenTypeCombination, IJsonTokenParser> _cache;
        private readonly IJsonReaderFactory _jsonReaderFactory;
        private readonly IReadOnlyList<IJsonTokenParser> _tokenParsers;
        private List<object> _deserializedObjects;
        private IJsonReader _jsonReader;

        /// <summary>
        ///     Creates a new intance of <see cref="JsonDeserializer" />.
        /// </summary>
        /// <param name="jsonReaderFactory">The factory that creates JSON readers.</param>
        /// <param name="tokenParsers">The token parsers that can deserilize different JSON tokens.</param>
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
        }

        /// <summary>
        ///     Deserializes the given JSON document as the specified type.
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
        ///     Deserializes the given JSON document as the specified type.
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
            _deserializedObjects = new List<object>();
            var returnValue = DeserializeDocument(requestedType);
            _jsonReader = null;
            _deserializedObjects = null;
            return returnValue;
        }

        private object DeserializeDocument(Type requestedType)
        {
            var token = _jsonReader.ReadNextToken();
            return DeserializeJsonToken(token, requestedType);
        }

        private object DeserializeJsonToken(JsonToken token, Type requestedType)
        {
            IJsonTokenParser parser;

            var tokenTypeCombination = new JsonTokenTypeCombination(token.JsonType, requestedType);
            if (_cache.TryGetValue(tokenTypeCombination, out parser) == false)
            {
                foreach (var tokenParser in _tokenParsers)
                {
                    if (tokenParser.IsSuitableFor(token, requestedType) == false)
                        continue;

                    parser = tokenParser;
                    break;
                }

                if (parser == null)
                    throw new DeserializationException($"Cannot deserialize value {token} with requested type {requestedType.FullName} because there is no parser that is suitable for this context.");

                if (parser.CanBeCached)
                    _cache.Add(tokenTypeCombination, parser);
            }

            return parser.ParseValue(new JsonDeserializationContext(token, requestedType, _jsonReader, DeserializeJsonToken, _deserializedObjects));
        }
    }
}