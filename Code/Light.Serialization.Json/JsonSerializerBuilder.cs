using System;
using System.Collections.Generic;
using System.Linq;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;
using Light.Serialization.Json.BuilderHelpers;
using Light.Serialization.Json.Caching;
using Light.Serialization.Json.ComplexTypeDecomposition;
using Light.Serialization.Json.LowLevelWriting;
using Light.Serialization.Json.ObjectMetadata;
using Light.Serialization.Json.PrimitiveTypeFormatters;
using Light.Serialization.Json.SerializationRules;
using Light.Serialization.Json.WriterInstructors;

namespace Light.Serialization.Json
{
    /// <summary>
    ///     Represents a builder for <see cref="JsonSerializer" /> instances.
    /// </summary>
    public sealed class JsonSerializerBuilder : BaseBuilderWithPropertyInjectionPool<JsonSerializerBuilder>
    {
        private readonly List<IJsonWriterInstructor> _writerInstructors;
        private ICharacterEscaper _characterEscaper = new DefaultCharacterEscaper();
        private IMetadataInstructor _collectionMetadataInstructor;
        private IDictionary<Type, IJsonWriterInstructor> _instructorCache;
        private IJsonKeyNormalizer _keyNormalizer = new FirstCharacterToLowerAndRemoveAllSpecialCharactersNormalizer();
        private ITypeMetadataInstructor _objectMetadataInstructor;
        private IDictionary<Type, IPrimitiveTypeFormatter> _primitiveTypeFormattersMapping;
        private int _recursionLimit = JsonSerializer.DefaultRecursionLevelLimit;
        private IReadableValuesTypeAnalyzer _typeAnalyzer = new ValueProvidersCacheDecorator(new PublicPropertiesAndFieldsAnalyzer(), new Dictionary<Type, IList<IValueProvider>>());
        private ITypeToNameMapping _typeToNameMapping = new SimpleNameToTypeMapping();
        private IJsonWriterFactory _writerFactory;

        /// <summary>
        ///     Initializes a new instance of <see cref="JsonSerializerBuilder" />.
        /// </summary>
        public JsonSerializerBuilder()
        {
            _writerFactory = Pool.Register(JsonWriterFactory.CreateDefault());
            _instructorCache = Pool.Register(new Dictionary<Type, IJsonWriterInstructor>());

            _primitiveTypeFormattersMapping = new List<IPrimitiveTypeFormatter>().AddDefaultPrimitiveTypeFormatters(_characterEscaper)
                                                                                 .ToDictionary(f => f.TargetType);
            Pool.RegisterAll(_primitiveTypeFormattersMapping.Values);

            _collectionMetadataInstructor = Pool.Register(new ArrayMetadataInstructor(_typeToNameMapping));
            _objectMetadataInstructor = Pool.Register(new ObjectMetadataInstructor(_typeToNameMapping));

            _writerInstructors = new List<IJsonWriterInstructor>().AddDefaultWriterInstructors(_primitiveTypeFormattersMapping,
                                                                                               _typeAnalyzer,
                                                                                               _collectionMetadataInstructor,
                                                                                               _objectMetadataInstructor);
            Pool.RegisterAll(_writerInstructors);
        }

        /// <summary>
        ///     Gets the collection containing all writer instructors that will be injected into the serializer.
        /// </summary>
        public IReadOnlyList<IJsonWriterInstructor> WriterInstructors => _writerInstructors;

        /// <summary>
        ///     Configures to builder to use the specified JSON writer factory for the serializer.
        /// </summary>
        /// <param name="writerFactory">the writer factory that is injected into the serializer.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writerFactory" /> is null.</exception>
        public JsonSerializerBuilder WithWriterFactory(IJsonWriterFactory writerFactory)
        {
            writerFactory.MustNotBeNull(nameof(writerFactory));

            _writerFactory = writerFactory;
            return this;
        }

        /// <summary>
        ///     Configures the builder to use the specified character escaper for primitive type formatters.
        /// </summary>
        /// <param name="characterEscaper">The character escaper used for characters and strings.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="characterEscaper" /> is null.</exception>
        public JsonSerializerBuilder WithCharacterEscaper(ICharacterEscaper characterEscaper)
        {
            characterEscaper.MustNotBeNull(nameof(characterEscaper));

            Pool.SetFieldAndReplaceInPool(ref _characterEscaper, characterEscaper);
            return ConfigureAll<ISetCharacterEscaper>(o => o.CharacterEscaper = characterEscaper);
        }

        /// <summary>
        ///     Configures the builder to use the specified type analyzer CustomRuleInstructors and the ComplexObjectInstructor.
        /// </summary>
        /// <param name="typeAnalyzer">The object that creates value providers for the given type.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeAnalyzer" /> is null.</exception>
        public JsonSerializerBuilder WithTypeAnalyzer(IReadableValuesTypeAnalyzer typeAnalyzer)
        {
            typeAnalyzer.MustNotBeNull(nameof(typeAnalyzer));

            Pool.SetFieldAndReplaceInPool(ref _typeAnalyzer, typeAnalyzer);
            return ConfigureAll<ISetTypeAnalyzer>(o => o.TypeAnalyzer = typeAnalyzer);
        }

        /// <summary>
        ///     Configures the builder to use the specified formatters instead of the default ones.
        /// </summary>
        /// <param name="formatters">The list containing all formatters to be used.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatters" /> is null.</exception>
        /// <exception cref="EmptyCollectionException">Thrown when <paramref name="formatters" /> contains no items.</exception>
        public JsonSerializerBuilder WithPrimitiveTypeFormatters(IEnumerable<IPrimitiveTypeFormatter> formatters)
        {
            // ReSharper disable PossibleMultipleEnumeration
            formatters.MustNotBeNullOrEmpty(nameof(formatters));

            return WithPrimitiveTypeFormatters(formatters.ToDictionary(f => f.TargetType));
            // ReSharper restore PossibleMultipleEnumeration
        }

        /// <summary>
        ///     Configures the builder to use the specified formatters instead of the default ones.
        /// </summary>
        /// <param name="formattersMapping">The dictionary containing all formatters to be used.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formattersMapping" /> is null.</exception>
        /// <exception cref="EmptyCollectionException">Thrown when <paramref name="formattersMapping" /> is empty.</exception>
        public JsonSerializerBuilder WithPrimitiveTypeFormatters(IDictionary<Type, IPrimitiveTypeFormatter> formattersMapping)
        {
            formattersMapping.MustNotBeNullOrEmpty(nameof(formattersMapping));

            Pool.RemoveAll(_primitiveTypeFormattersMapping.Values);
            _primitiveTypeFormattersMapping = formattersMapping;
            Pool.RegisterAll(formattersMapping.Values);

            return ConfigureAll<ISetPrimitiveTypeFormatters>(o => o.PrimitiveTypeFormattersMapping = _primitiveTypeFormattersMapping);
        }

        /// <summary>
        ///     Adds the specified formatter to the mapping of all primitive type formatters.
        /// </summary>
        /// <param name="formatter">The primitive type formatter to be added.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter" /> is null.</exception>
        public JsonSerializerBuilder AddPrimitiveTypeFormatter(IPrimitiveTypeFormatter formatter)
        {
            formatter.MustNotBeNull(nameof(formatter));

            _primitiveTypeFormattersMapping.Add(formatter.TargetType, formatter);
            Pool.Register(formatter);

            return this;
        }

        /// <summary>
        ///     Adds the specified JSON writer instructor after the instance with the given type.
        /// </summary>
        /// <typeparam name="T">The type whose instance should be before the given writer instructor.</typeparam>
        /// <param name="additionalWriterInstructor">The writer instructor to be inserted.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="additionalWriterInstructor" /> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the writer instructor with type T cannot be found.</exception>
        public JsonSerializerBuilder AddWriterInstructorAfter<T>(IJsonWriterInstructor additionalWriterInstructor)
            where T : IJsonWriterInstructor
        {
            additionalWriterInstructor.MustNotBeNull(nameof(additionalWriterInstructor));

            var targetIndex = _writerInstructors.IndexOf(_writerInstructors.OfType<T>().First());
            targetIndex.MustNotBe(-1,
                                  exception: () => new ArgumentException($"The specified writer instructor \"{additionalWriterInstructor}\" cannot be added after the instructor \"{typeof(T)}\" because the latter was not found."));

            if (targetIndex == _writerInstructors.Count - 1)
                _writerInstructors.Add(additionalWriterInstructor);
            else
                _writerInstructors.Insert(targetIndex + 1, additionalWriterInstructor);
            Pool.Register(additionalWriterInstructor);

            return this;
        }

        /// <summary>
        ///     Adds the specified JSON writer instructor before the instance with the given type.
        /// </summary>
        /// <typeparam name="T">The type whose instance should be after the given writer instructor.</typeparam>
        /// <param name="additionalWriterInstructor">The writer instructor to be inserted.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="additionalWriterInstructor" /> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the writer instructor with type T cannot be found.</exception>
        public JsonSerializerBuilder AddWriterInstructorBefore<T>(IJsonWriterInstructor additionalWriterInstructor)
            where T : IJsonWriterInstructor
        {
            additionalWriterInstructor.MustNotBeNull(nameof(additionalWriterInstructor));

            var targetIndex = _writerInstructors.IndexOf(_writerInstructors.OfType<T>().First());
            targetIndex.MustNotBe(-1,
                                  exception: () => new ArgumentException($"The specified writer instructor \"{additionalWriterInstructor}\" cannot be added before the instructor \"{typeof(T)}\" because the latter was not found."));

            _writerInstructors.Insert(targetIndex, additionalWriterInstructor);
            Pool.Register(additionalWriterInstructor);
            return this;
        }

        /// <summary>
        ///     Configures the builder to inject the specified cache for JSON writer instructors into the serializer.
        /// </summary>
        /// <param name="instructorCache">The dictionary used as the instructorCache.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instructorCache" /> is null.</exception>
        public JsonSerializerBuilder WithInstructorCache(IDictionary<Type, IJsonWriterInstructor> instructorCache)
        {
            instructorCache.MustNotBeNull(nameof(instructorCache));

            _instructorCache = instructorCache;
            return this;
        }

        /// <summary>
        ///     Configures the JSON writer instructor with the given type using the specified delegate.
        /// </summary>
        /// <typeparam name="T">The type of the writer instructor that should be configured.</typeparam>
        /// <param name="configureInstructor">The delegate that configures the actual instructor.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configureInstructor" /> is null.</exception>
        public JsonSerializerBuilder ConfigureInstructor<T>(Action<T> configureInstructor)
            where T : IJsonWriterInstructor
        {
            configureInstructor.MustNotBeNull(nameof(configureInstructor));

            configureInstructor(_writerInstructors.OfType<T>().First());
            return this;
        }

        /// <summary>
        ///     Configures the primitive type formatter with the given type using the specified delegate.
        /// </summary>
        /// <typeparam name="T">The type of the formatter that should be configured.</typeparam>
        /// <param name="configureFormatter">The delegate that configures the actual formatter instance.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configureFormatter" /> is null.</exception>
        public JsonSerializerBuilder ConfigurePrimitiveTypeFormatter<T>(Action<T> configureFormatter)
            where T : IPrimitiveTypeFormatter
        {
            configureFormatter.MustNotBeNull(nameof(configureFormatter));

            configureFormatter(_primitiveTypeFormattersMapping.Values.OfType<T>().First());
            return this;
        }

        /// <summary>
        ///     Creates a new serialization rule for the given type that is configured with the specified delegate.
        ///     An existing rule for the specified type will be replaced.
        ///     Please note that every rule uses the IReadableValuesTypeAnalyzer instance that is registered with the builder.
        ///     If you do not want to use the default instance, you should exchange it first.
        /// </summary>
        /// <typeparam name="T">The type that should be configured for serialization.</typeparam>
        /// <param name="configureRule">The delegate that configures the serialization rule.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configureRule" /> is null.</exception>
        public JsonSerializerBuilder WithRuleFor<T>(Action<Rule<T>> configureRule)
        {
            configureRule.MustNotBeNull(nameof(configureRule));

            var newRule = new Rule<T>(_typeAnalyzer);
            configureRule(newRule);

            var existingInstructor = _writerInstructors.OfType<CustomRuleInstructor>()
                                                       .FirstOrDefault(i => i.TargetType == typeof(T));

            if (existingInstructor != null)
            {
                _writerInstructors.Remove(existingInstructor);
                Pool.Remove(existingInstructor);
            }

            _writerInstructors.Insert(0, Pool.Register(new CustomRuleInstructor(typeof(T), newRule.CreateValueProviders(), _objectMetadataInstructor)));

            return this;
        }

        /// <summary>
        ///     Exchanges the existing object metadata instructor with the specified one.
        /// </summary>
        /// <param name="metadataInstructor">The new metadata instructor for complex JSON objects.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="metadataInstructor" /> is null.</exception>
        public JsonSerializerBuilder WithObjectMetadataInstructor(ITypeMetadataInstructor metadataInstructor)
        {
            metadataInstructor.MustNotBeNull(nameof(metadataInstructor));

            Pool.SetFieldAndReplaceInPool(ref _objectMetadataInstructor, metadataInstructor);
            return ConfigureAll<ISetTypeInstructor>(o => o.MetadataInstructor = metadataInstructor)
                .ConfigureAll<ISetObjectMetadataInstructor>(o => o.MetadataInstructor = metadataInstructor);
        }

        /// <summary>
        ///     Exchanges the specified collection metadata instructor with the specified one.
        /// </summary>
        /// <param name="metadataInstructor">The new metadata instructor for JSON arrays.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="metadataInstructor" /> is null.</exception>
        public JsonSerializerBuilder WithCollectionMetadataInstructor(IMetadataInstructor metadataInstructor)
        {
            metadataInstructor.MustNotBeNull(nameof(metadataInstructor));

            Pool.SetFieldAndReplaceInPool(ref _collectionMetadataInstructor, metadataInstructor);
            return ConfigureAll<ISetCollectionMetadataInstructor>(o => o.MetadataInstructor = metadataInstructor);
        }

        /// <summary>
        ///     Configures the metadata instructor for complex JSON objects with the specified delegate. The default metadata instructor is of type TypeAndReferenceMetadataInstructor.
        /// </summary>
        /// <typeparam name="T">The actual type of the metadata instructor. This type must derive from IMetadataInstructor.</typeparam>
        /// <param name="configureInstructor">The delegate that configures the instructor.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configureInstructor" /> is null.</exception>
        public JsonSerializerBuilder ConfigureObjectMetadataInstructor<T>(Action<T> configureInstructor) where T : class, IMetadataInstructor
        {
            configureInstructor.MustNotBeNull(nameof(configureInstructor));

            configureInstructor(_objectMetadataInstructor.MustBeOfType<T>());

            return this;
        }

        /// <summary>
        ///     Configures the metadata instructor for JSON arrays with the specified delegate. The default metadata instructor is of type CollectionReferenceMetadataInstructor.
        /// </summary>
        /// <typeparam name="T">The actual type of the metadata instructor. This type must derive from IMetadataInstructor.</typeparam>
        /// <param name="configureInstructor">The delegate that configures the instructor.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configureInstructor" /> is null.</exception>
        public JsonSerializerBuilder ConfigureCollectionMetadataInstructor<T>(Action<T> configureInstructor) where T : class, IMetadataInstructor
        {
            configureInstructor.MustNotBeNull(nameof(configureInstructor));

            configureInstructor(_collectionMetadataInstructor.MustBeOfType<T>());

            return this;
        }

        /// <summary>
        ///     Configures the metadata instructors to not include object IDs in the JSON document.
        /// </summary>
        /// <returns>The builder for method chaining.</returns>
        public JsonSerializerBuilder DisableObjectReferencePreservation()
        {
            return ConfigureAll<ISetObjectReferencePreservationStatus>(o => o.IsSerializingObjectIds = false);
        }

        /// <summary>
        ///     Configures the metadata instructors to include object IDs in the JSON document. This is turned on by default.
        /// </summary>
        /// <returns>The builder for method chaining.</returns>
        public JsonSerializerBuilder EnableObjectReferencePreservation()
        {
            return ConfigureAll<ISetObjectReferencePreservationStatus>(o => o.IsSerializingObjectIds = true);
        }

        /// <summary>
        ///     Configures the object metadata instructor to not include type information of complex .NET types.
        /// </summary>
        /// <returns>The builder for method chaining.</returns>
        public JsonSerializerBuilder DisableTypeMetadata()
        {
            return ConfigureAll<ISetTypeInfoSerializationStatus>(o => o.IsSerializingTypeInfo = false);
        }

        /// <summary>
        ///     Configures the metadata instructor to include type information for complex .NET types. This is turned on by default for the object metadata instructor (the default collection metadata instructor does not support this feature).
        /// </summary>
        /// <returns>The builder for method chaining.</returns>
        public JsonSerializerBuilder EnableTypeMetadata()
        {
            return ConfigureAll<ISetTypeInfoSerializationStatus>(o => o.IsSerializingTypeInfo = true);
        }

        /// <summary>
        ///     Exchanges the existing mapping from .NET types to JSON names with the specified one.
        /// </summary>
        /// <param name="typeToNameMapping">The new mapping from .NET types to JSON names.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeToNameMapping" /> is null.</exception>
        public JsonSerializerBuilder WithTypeToNameMapping(ITypeToNameMapping typeToNameMapping)
        {
            typeToNameMapping.MustNotBeNull(nameof(typeToNameMapping));

            Pool.SetFieldAndReplaceInPool(ref _typeToNameMapping, typeToNameMapping);
            return ConfigureAll<ISetTypeToNameMapping>(o => o.TypeToNameMapping = typeToNameMapping);
        }

        /// <summary>
        ///     Exchanges the currently used keyNormalizer with the specified one. The default key normalizer is an instance of FirstCharacterToLowerAndRemoveAllSpecialCharactersNormalizer.
        /// </summary>
        /// <param name="keyNormalizer">The new key normalizer.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="keyNormalizer" /> is null.</exception>
        public JsonSerializerBuilder WithKeyNormalizer(IJsonKeyNormalizer keyNormalizer)
        {
            keyNormalizer.MustNotBeNull(nameof(keyNormalizer));

            Pool.SetFieldAndReplaceInPool(ref _keyNormalizer, keyNormalizer);
            return ConfigureAll<ISetKeyNormalizer>(o => o.KeyNormalizer = keyNormalizer);
        }

        /// <summary>
        ///     Exchanges the existing delegate that creates an <see cref="IJsonWhitespaceFormatter" /> with an instance creating an
        ///     <see cref="IndentingWhitespaceFormatter" /> so that the resulting JSON document contains new lines and indenting.
        /// </summary>
        /// <returns>The builder for method chaining.</returns>
        public JsonSerializerBuilder EnableHumanReadableJsonDocuments()
        {
            return WithCreateMethodForWhitespaceFormatter(IndentingWhitespaceFormatter.Create);
        }

        /// <summary>
        ///     Exchanges the creation delegate for the whitespace formatter with the specified one.
        /// </summary>
        /// <param name="createWhitespaceFormatter">The delegate that creates a new <see cref="IJsonWhitespaceFormatter" /> instance.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="createWhitespaceFormatter" /> is null.</exception>
        public JsonSerializerBuilder WithCreateMethodForWhitespaceFormatter(Func<IJsonWhitespaceFormatter> createWhitespaceFormatter)
        {
            createWhitespaceFormatter.MustNotBeNull(nameof(createWhitespaceFormatter));

            return ConfigureAll<ISetWhitespaceFormatterCreationDelegate>(o => o.CreateWhitespaceFormatter = createWhitespaceFormatter);
        }

        /// <summary>
        ///     Configures all instances that derive from the <see cref="BaseMetadata" /> class in the serialization object graph with the specified delegate (usually metadata intructors).
        /// </summary>
        /// <param name="configureMetadata">The delegate that configures the <see cref="BaseMetadata" /> instance.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configureMetadata" /> is null.</exception>
        public JsonSerializerBuilder ConfigureMetadataSymbols(Action<BaseMetadata> configureMetadata)
        {
            configureMetadata.MustNotBeNull(nameof(configureMetadata));

            return ConfigureAll(configureMetadata);
        }

        /// <summary>
        ///     Configures the builder to inject the specified recursion level limit into the resulting <see cref="JsonSerializer" /> instance.
        ///     The default recursion level limit is 300.
        /// </summary>
        /// <param name="recursionLimit">The new recursion level limit.</param>
        /// <returns>The builder for method chaininig.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="recursionLimit" /> is less than 3.</exception>
        public JsonSerializerBuilder WithRecursionLevelLimit(int recursionLimit)
        {
            recursionLimit.MustNotBeLessThan(JsonSerializer.MinimumRecursionLevelLimit, nameof(recursionLimit));
            _recursionLimit = recursionLimit;
            return this;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="JsonSerializer" /> using the specified builder settings.
        /// </summary>
        public JsonSerializer Build()
        {
            return new JsonSerializer(_writerInstructors, _writerFactory, _instructorCache) { RecursionLevelLimit = _recursionLimit };
        }
    }
}