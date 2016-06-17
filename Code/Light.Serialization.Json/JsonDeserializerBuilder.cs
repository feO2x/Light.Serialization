using System;
using System.Collections.Generic;
using System.Linq;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;
using Light.Serialization.Json.BuilderHelpers;
using Light.Serialization.Json.Caching;
using Light.Serialization.Json.ComplexTypeConstruction;
using Light.Serialization.Json.FrameworkExtensions;
using Light.Serialization.Json.LowLevelReading;
using Light.Serialization.Json.ObjectMetadata;
using Light.Serialization.Json.TokenParsers;

namespace Light.Serialization.Json
{
    /// <summary>
    ///     Represents a builder for the JSON Deserializer.
    /// </summary>
    public sealed class JsonDeserializerBuilder : BaseBuilderWithPropertyInjectionPool<JsonDeserializerBuilder>
    {
        private IArrayMetadataParser _arrayMetadataParser;
        private IMetaFactory _metaFactory = new DefaultMetaFactory();
        private INameToTypeMapping _nameToTypeMapping;
        private IObjectMetadataParser _objectMetadataParser;
        private IJsonReaderFactory _readerFactory = new JsonReaderFactory();
        private Dictionary<JsonTokenTypeCombination, IJsonTokenParser> _tokenParserCache = new Dictionary<JsonTokenTypeCombination, IJsonTokenParser>();
        private List<IJsonTokenParserFactory> _tokenParserFactories;
        private ITypeDescriptionService _typeDescriptionService;

        /// <summary>
        ///     Creates a new instance of <see cref="JsonDeserializerBuilder" />.
        /// </summary>
        public JsonDeserializerBuilder()
        {
            _nameToTypeMapping = new SimpleNameToTypeMapping();
            _objectMetadataParser = Pool.Register(new ComplexObjectMetadataParser(_nameToTypeMapping));
            _arrayMetadataParser = Pool.Register(new ArrayMetadataParser(_nameToTypeMapping));
            _typeDescriptionService = Pool.Register(new DefaultTypeDescriptionServiceWithCaching(new Dictionary<Type, TypeCreationDescription>()));

            _tokenParserFactories = new List<IJsonTokenParserFactory>().AddDefaultTokenParserFactories(_metaFactory, _objectMetadataParser, _arrayMetadataParser, _typeDescriptionService);
            Pool.RegisterAll(_tokenParserFactories.OfType<SingletonFactory>().Select(f => f.Instance));
            Pool.RegisterAll(_tokenParserFactories.Where(f => f.GetType() != typeof(SingletonFactory)));
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

            Pool.SetFieldAndReplaceInPool(ref _nameToTypeMapping, mapping);
            return ConfigureAll<ISetNameToTypeMapping>(o => o.NameToTypeMapping = mapping);
        }

        /// <summary>
        ///     Configures the builder to inject the specified <see cref="IObjectMetadataParser" /> instance in the resulting deserializer object graph.
        /// </summary>
        /// <param name="metadataParser">The parser for metadata sections in complex JSON objects.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="metadataParser" /> is null.</exception>
        public JsonDeserializerBuilder WithObjectMetadataParser(IObjectMetadataParser metadataParser)
        {
            metadataParser.MustNotBeNull(nameof(metadataParser));

            Pool.SetFieldAndReplaceInPool(ref _objectMetadataParser, metadataParser);
            return ConfigureAll<ISetObjectMetadataParser>(o => o.MetadataParser = metadataParser);
        }

        /// <summary>
        ///     Configures the builder to inject the specified <see cref="IArrayMetadataParser" /> instance in the resulting deserializer object graph.
        /// </summary>
        /// <param name="metadataParser">The parser for metadata sections in JSON arrays.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="metadataParser" /> is null.</exception>
        public JsonDeserializerBuilder WithArrayMetadataParser(IArrayMetadataParser metadataParser)
        {
            Pool.SetFieldAndReplaceInPool(ref _arrayMetadataParser, metadataParser);
            return ConfigureAll<ISetArrayMetadataParser>(o => o.MetadataParser = metadataParser);
        }

        /// <summary>
        ///     Configures the builder to use the specified type description service.
        /// </summary>
        /// <param name="typeDescriptionService">The type description service to be used.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeDescriptionService" /> is null.</exception>
        public JsonDeserializerBuilder WithTypeDescriptionService(ITypeDescriptionService typeDescriptionService)
        {
            typeDescriptionService.MustNotBeNull(nameof(typeDescriptionService));

            Pool.SetFieldAndReplaceInPool(ref _typeDescriptionService, typeDescriptionService);
            return ConfigureAll<ISetTypeDescriptionService>(o => o.TypeDescriptionService = typeDescriptionService);
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
        ///     Configures the JSON reader factory using the specified delegate.
        /// </summary>
        /// <typeparam name="T">The actual type of the JSON reader factory.</typeparam>
        /// <param name="configureFactory">The delegate that is used to configure the instance.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configureFactory" /> is null.</exception>
        public JsonDeserializerBuilder ConfigureReaderFactory<T>(Action<T> configureFactory) where T : class, IJsonReaderFactory
        {
            configureFactory.MustNotBeNull(nameof(configureFactory));

            var readerFactory = _readerFactory.MustBeOfType<T>();
            configureFactory(readerFactory);

            return this;
        }

        /// <summary>
        ///     Configures the default instance of <see cref="JsonReaderFactory" />. Please note that you should not
        ///     call this method when you replaced the default reader factory with an instance of another type.
        /// </summary>
        /// <param name="configureFactory">The delegate that configures the factory.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configureFactory" /> is null.</exception>
        public JsonDeserializerBuilder ConfigureDefaultReaderFactory(Action<JsonReaderFactory> configureFactory)
        {
            return ConfigureReaderFactory(configureFactory);
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
        ///     Configures the builder to use the specified factories to create <see cref="IJsonTokenParser" /> instances.
        /// </summary>
        /// <param name="factories">The list containing all factories for the JSON token parsers. Keep in mind that the order of the parsers usually matter for the deserialization process.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="factories" /> is null.</exception>
        /// <exception cref="EmptyCollectionException">Thrown when <paramref name="factories" /> has no items.</exception>
        public JsonDeserializerBuilder WithTokenParserFactories(List<IJsonTokenParserFactory> factories)
        {
            factories.MustNotBeNullOrEmpty(nameof(factories));

            Pool.RemoveAll(_tokenParserFactories);
            Pool.RemoveAll(_tokenParserFactories.OfType<SingletonFactory>().Select(f => f.Instance));
            _tokenParserFactories = factories;
            Pool.RegisterAll(factories.OfType<SingletonFactory>().Select(f => f.Instance));
            Pool.RegisterAll(factories.Where(f => f.GetType() != typeof(SingletonFactory)));
            UpdateJsonStringToPrimitiveParsers();

            return this;
        }

        private void UpdateJsonStringToPrimitiveParsers()
        {
            var jsonStringToPrimitiveParser = _tokenParserFactories.OfType<SingletonFactory>()
                                                                   .Select(f => f.Instance)
                                                                   .OfType<IJsonStringToPrimitiveParser>()
                                                                   .ToList();
            ConfigureAll<ISetJsonStringToPrimitiveParsers>(o => o.JsonStringToPrimitiveParsers = jsonStringToPrimitiveParser);
        }

        /// <summary>
        ///     Adds the specified factory after the one that creates the token parser with the given type T.
        /// </summary>
        /// <typeparam name="T">The type of the token parser the factory creates. The specified factory is placed after the selected one.</typeparam>
        /// <param name="factory">The factory that will be inserted in the list of token parser factories.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory" /> is null.</exception>
        public JsonDeserializerBuilder WithTokenParserFactoryAfter<T>(IJsonTokenParserFactory factory) where T : IJsonTokenParser
        {
            factory.MustNotBeNull(nameof(factory));

            var targetIndex = _tokenParserFactories.IndexOf(f => f.ParserType == typeof(T));
            targetIndex.MustNotBe(-1, exception: () => new ArgumentException($"A factory that creates a token parser of type \"{typeof(T)}\" could not be found."));

            if (targetIndex == _tokenParserFactories.Count - 1)
                _tokenParserFactories.Add(factory);
            else
                _tokenParserFactories.Insert(targetIndex + 1, factory);

            RegisterFactory(factory);
            return this;
        }

        /// <summary>
        ///     Adds the specified factory before the one that creates the token parser with the given type T.
        /// </summary>
        /// <typeparam name="T">The type of the token parser the factory creates. The specified factory is placed after the selected one.</typeparam>
        /// <param name="factory">The factory that will be inserted in the list of token parser factories.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory" /> is null.</exception>
        public JsonDeserializerBuilder WithTokenParserFactoryBefore<T>(IJsonTokenParserFactory factory) where T : IJsonTokenParser
        {
            factory.MustNotBeNull(nameof(factory));

            var targetIndex = _tokenParserFactories.IndexOf(f => f.ParserType == typeof(T));
            targetIndex.MustNotBe(-1, exception: () => new ArgumentException($"A factory that creates a token parser of type \"{typeof(T)}\" could not be found."));

            _tokenParserFactories.Insert(targetIndex, factory);

            RegisterFactory(factory);
            return this;
        }

        private void RegisterFactory(IJsonTokenParserFactory factory)
        {
            var singletonFactory = factory as SingletonFactory;
            if (singletonFactory == null)
            {
                Pool.Register(factory);
                return;
            }

            Pool.Register(singletonFactory.Instance);
            if (singletonFactory.Instance is IJsonStringToPrimitiveParser)
                UpdateJsonStringToPrimitiveParsers();
        }

        /// <summary>
        ///     Configures the builder to inject the specified <see cref="IMetaFactory" /> instance in the resulting deserialization object graph.
        /// </summary>
        /// <param name="metaFactory">The new meta factory.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="metaFactory" /> is null.</exception>
        public JsonDeserializerBuilder WithMetaFactory(IMetaFactory metaFactory)
        {
            metaFactory.MustNotBeNull(nameof(metaFactory));

            Pool.SetFieldAndReplaceInPool(ref _metaFactory, metaFactory);
            return ConfigureAll<ISetMetaFactory>(o => o.MetaFactory = metaFactory);
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