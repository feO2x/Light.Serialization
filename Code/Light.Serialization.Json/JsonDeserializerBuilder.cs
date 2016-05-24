using System.Collections.Generic;
using Light.Serialization.Json.Caching;
using Light.Serialization.Json.LowLevelReading;
using Light.Serialization.Json.TokenParsers;

namespace Light.Serialization.Json
{
    /// <summary>
    ///     Represents a builder for the JSON Deserializer.
    /// </summary>
    public sealed class JsonDeserializerBuilder
    {
        private IJsonReaderFactory _readerFactory = new SingleBufferJsonReaderFactory();
        private Dictionary<JsonTokenTypeCombination, IJsonTokenParser> _tokenParserCache = new Dictionary<JsonTokenTypeCombination, IJsonTokenParser>();

        public JsonDeserializerBuilder WithReaderFactory(IJsonReaderFactory readerFactory)
        {
            _readerFactory = readerFactory;
            return this;
        }

        public JsonDeserializerBuilder WithTokenParserCache(Dictionary<JsonTokenTypeCombination, IJsonTokenParser> cache)
        {
            _tokenParserCache = cache;
            return this;
        }

        public JsonDeserializer Build()
        {
            List<IJsonTokenParser> parsers = null;

            return new JsonDeserializer(_readerFactory, parsers, _tokenParserCache);
        }
    }
}