using System;
using System.Collections.Generic;
using System.Linq;
using Light.GuardClauses;
using Light.Serialization.Json.BuilderInterfaces;
using Light.Serialization.Json.Caching;
using Light.Serialization.Json.ComplexTypeConstruction;
using Light.Serialization.Json.LowLevelReading;
using Light.Serialization.Json.ObjectMetadata;
using Light.Serialization.Json.TokenParserFactories;
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
        private readonly List<IJsonTokenParserFactory> _tokenParserFactories;
        private readonly ITypeDescriptionProvider _typeDescriptionProvider;
        private INameToTypeMapping _nameToTypeMapping;
        private IJsonReaderFactory _readerFactory = new SingleBufferJsonReaderFactory();
        private Dictionary<JsonTokenTypeCombination, IJsonTokenParser> _tokenParserCache = new Dictionary<JsonTokenTypeCombination, IJsonTokenParser>();

        /// <summary>
        ///     Creates a new instance of JsonDeserializerBuilder.
        /// </summary>
        public JsonDeserializerBuilder()
        {
            _nameToTypeMapping = new SimpleNameToTypeMapping();
            _complexObjectMetadataParser = new ComplexObjectMetadataParser(_nameToTypeMapping);
            _arrayMetadataParser = new ArrayMetadataParser(_nameToTypeMapping);
            _nameNormalizer = new ToLowerWithoutSpecialCharactersNormalizer();
            _typeDescriptionProvider = new CreationDescriptionCacheDecorator(new Dictionary<Type, TypeCreationDescription>(), new DefaultTypeDescriptionProvider(_nameNormalizer));

            _tokenParserFactories = new List<IJsonTokenParserFactory>().AddDefaultTokenParserFactories(_metaFactory, _complexObjectMetadataParser, _arrayMetadataParser, _nameNormalizer, _typeDescriptionProvider);
        }

        /// <summary>
        ///     Configures the builder to use the specified name to type mapping.
        /// </summary>
        /// <param name="mapping">The object that can map JSON strings to .NET types.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="mapping" /> is null.</exception>
        public JsonDeserializerBuilder WithNameToTypeMapping(INameToTypeMapping mapping)
        {
            mapping.MustNotBeNull(nameof(mapping));

            _nameToTypeMapping = mapping;
            SetNameToTypeMappingOnMetadataParser(_complexObjectMetadataParser, mapping);
            SetNameToTypeMappingOnMetadataParser(_arrayMetadataParser, mapping);
            return this;
        }

        private static void SetNameToTypeMappingOnMetadataParser(IMetadataParser metadataParser, INameToTypeMapping mapping)
        {
            var setNameToTypeMapping = metadataParser as ISetNameToTypeMapping;
            if (setNameToTypeMapping == null)
                return;

            setNameToTypeMapping.NameToTypeMapping = mapping;
        }

        /// <summary>
        ///     Configures the builder to use the specified JSON reader factory.
        /// </summary>
        /// <param name="readerFactory">The factory that can create IJsonReader instances for the deserializer.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="readerFactory" /> is null.</exception>
        public JsonDeserializerBuilder WithReaderFactory(IJsonReaderFactory readerFactory)
        {
            readerFactory.MustNotBeNull(nameof(readerFactory));

            _readerFactory = readerFactory;
            return this;
        }

        /// <summary>
        ///     Configures the builder to use the specified token parser cache.
        /// </summary>
        /// <param name="cache">The dictionary that acts as the cache for token parsers.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cache" /> is null.</exception>
        public JsonDeserializerBuilder WithTokenParserCache(Dictionary<JsonTokenTypeCombination, IJsonTokenParser> cache)
        {
            cache.MustNotBeNull(nameof(cache));

            _tokenParserCache = cache;
            return this;
        }

        /// <summary>
        ///     Creates the JsonDeserializer with the specified configuration.
        /// </summary>
        public JsonDeserializer Build()
        {
            var parsers = _tokenParserFactories.Select(pp => pp.Create())
                                               .ToList();

            return new JsonDeserializer(_readerFactory, parsers, _tokenParserCache);
        }
    }
}