using System;
using System.Collections.Generic;
using System.Linq;
using Light.GuardClauses;
using Light.Serialization.Json.BuilderHelpers;
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
        private readonly IArrayMetadataParser _arrayMetadataParser;
        private readonly IObjectMetadataParser _complexObjectMetadataParser;
        private readonly IMetaFactory _metaFactory = new DefaultMetaFactory();
        private readonly List<IJsonTokenParserFactory> _tokenParserFactories;
        private INameToTypeMapping _nameToTypeMapping;
        private IJsonReaderFactory _readerFactory = new JsonReaderFactory();
        private Dictionary<JsonTokenTypeCombination, IJsonTokenParser> _tokenParserCache = new Dictionary<JsonTokenTypeCombination, IJsonTokenParser>();
        private ITypeDescriptionService _typeDescriptionService;

        /// <summary>
        ///     Creates a new instance of JsonDeserializerBuilder.
        /// </summary>
        public JsonDeserializerBuilder()
        {
            _nameToTypeMapping = new SimpleNameToTypeMapping();
            _complexObjectMetadataParser = new ComplexObjectMetadataParser(_nameToTypeMapping);
            _arrayMetadataParser = new ArrayMetadataParser(_nameToTypeMapping);
            _typeDescriptionService = new DefaultTypeDescriptionServiceWithCaching(new Dictionary<Type, TypeCreationDescription>());

            _tokenParserFactories = new List<IJsonTokenParserFactory>().AddDefaultTokenParserFactories(_metaFactory, _complexObjectMetadataParser, _arrayMetadataParser, _typeDescriptionService);
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
            SetNameToTypeMappingOnMetadataParser(_complexObjectMetadataParser as ISetNameToTypeMapping, mapping);
            SetNameToTypeMappingOnMetadataParser(_arrayMetadataParser as ISetNameToTypeMapping, mapping);
            return this;
        }

        private static void SetNameToTypeMappingOnMetadataParser(ISetNameToTypeMapping metadataParser, INameToTypeMapping mapping)
        {
            if (metadataParser == null)
                return;

            metadataParser.NameToTypeMapping = mapping;
        }

        /// <summary>
        ///     Configures the builder to use the specified type description service.
        /// </summary>
        /// <param name="typeDescriptionService">The type description service to be used.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeDescriptionService" /> is null.</exception>
        public JsonDeserializerBuilder TypeDescriptionService(ITypeDescriptionService typeDescriptionService)
        {
            typeDescriptionService.MustNotBeNull(nameof(typeDescriptionService));

            _typeDescriptionService = typeDescriptionService;
            ConfigureSingletonParser<ISetTypeDescriptionService>(parser => parser.TypeDescriptionService = typeDescriptionService); // TODO: this wouldn't work if the target parser is not a singleton

            return this;
        }

        /// <summary>
        ///     Configures the token parser with the specified type using the given delegate.
        ///     The parser must be created using a singleton parser factory, i.e. a factory that implements <see cref="IGetSingletonInstance{T}" />
        /// </summary>
        /// <typeparam name="T">The type of the token parser.</typeparam>
        /// <param name="configureParser">The delegate that is used to configure the parser.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configureParser" /> is null.</exception>
        public JsonDeserializerBuilder ConfigureSingletonParser<T>(Action<T> configureParser)
        {
            configureParser.MustNotBeNull(nameof(configureParser));

            var targetParserFactory = _tokenParserFactories.First(f => f.ParserType == typeof(T)) as IGetSingletonInstance<IJsonTokenParser>;
            targetParserFactory.MustNotBeNull(exception: () => new ArgumentException($"The specified parser factory for type \"{typeof(T)}\" could not be found."));

            // ReSharper disable once PossibleNullReferenceException
            configureParser((T) targetParserFactory.Instance);

            return this;
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
            var parsers = _tokenParserFactories.Select(factory => factory.Create())
                                               .ToList();

            return new JsonDeserializer(_readerFactory, parsers, _tokenParserCache);
        }
    }
}