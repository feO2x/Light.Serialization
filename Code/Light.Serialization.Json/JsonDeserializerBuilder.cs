using System;
using System.Collections.Generic;
using System.Linq;
using Light.Serialization.Json.Builders;
using Light.Serialization.Json.Caching;
using Light.Serialization.Json.ComplexTypeConstruction;
using Light.Serialization.Json.LowLevelReading;
using Light.Serialization.Json.ObjectMetadata;
using Light.Serialization.Json.TokenParsers;

namespace Light.Serialization.Json
{
    /// <summary>
    ///     Represents a builder for the JSON Deserializer.
    /// </summary>
    public sealed class JsonDeserializerBuilder
    {
        private readonly IMetadataParser _arrayMetadataParser;
        private readonly IMetadataParser _complexObjectMetadataParser;
        private readonly IMetaFactory _metaFactory = new DefaultMetaFactory();
        private readonly INameNormalizer _nameNormalizer;
        private readonly INameToTypeMapping _nameToTypeMapping;
        private IJsonReaderFactory _readerFactory = new SingleBufferJsonReaderFactory();
        private Dictionary<JsonTokenTypeCombination, IJsonTokenParser> _tokenParserCache = new Dictionary<JsonTokenTypeCombination, IJsonTokenParser>();
        private readonly List<IJsonTokenParserFactory> _tokenParserFactories;
        private readonly ITypeDescriptionProvider _typeDescriptionProvider;

        public JsonDeserializerBuilder()
        {
            _nameToTypeMapping = new SimpleNameToTypeMapping();
            _complexObjectMetadataParser = new ComplexObjectMetadataParser(_nameToTypeMapping);
            _arrayMetadataParser = new ArrayMetadataParser(_nameToTypeMapping);
            _nameNormalizer = new ToLowerWithoutSpecialCharactersNormalizer();
            _typeDescriptionProvider = new CreationDescriptionCacheDecorator(new Dictionary<Type, TypeCreationDescription>(), new DefaultTypeDescriptionProvider(_nameNormalizer));

            _tokenParserFactories = new List<IJsonTokenParserFactory>().AddDefaultTokenParserFactories(_metaFactory, _complexObjectMetadataParser, _arrayMetadataParser, _nameNormalizer, _typeDescriptionProvider);
        }

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
            var parsers = _tokenParserFactories.Select(pp => pp.Create())
                                               .ToList();

            return new JsonDeserializer(_readerFactory, parsers, _tokenParserCache);
        }
    }
}