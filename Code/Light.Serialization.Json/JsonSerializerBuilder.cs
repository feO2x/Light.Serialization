using System;
using System.Collections.Generic;
using System.Linq;
using Light.GuardClauses;
using Light.Serialization.Json.BuilderInterfaces;
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
    ///     Represents a builder for JSON Serializer instances.
    /// </summary>
    public class JsonSerializerBuilder
    {
        /// <summary>
        ///     Gets the list containing all writer instructors.
        /// </summary>
        public readonly List<IJsonWriterInstructor> WriterInstructors;

        private ICharacterEscaper _characterEscaper = new DefaultCharacterEscaper();
        private IMetadataInstructor _collectionMetadataInstructor = new CollectionReferenceMetadataInstructor();
        private Func<IJsonWriterFactory> _createWriterFactory;
        private IDictionary<Type, IJsonWriterInstructor> _instructorCache;
        private IMetadataInstructor _objectMetadataInstructor = new TypeAndReferenceMetadataInstructor(new SimpleNameToTypeMapping());
        private IDictionary<Type, IPrimitiveTypeFormatter> _primitiveTypeFormattersMapping;
        private IReadableValuesTypeAnalyzer _typeAnalyzer = new ValueProvidersCacheDecorator(new PublicPropertiesAndFieldsAnalyzer(), new Dictionary<Type, IList<IValueProvider>>());

        /// <summary>
        ///     Initializes a new instance of <see cref="JsonSerializerBuilder" />.
        /// </summary>
        public JsonSerializerBuilder()
        {
            UseDefaultWriterFactory();
            _instructorCache = new Dictionary<Type, IJsonWriterInstructor>();

            _primitiveTypeFormattersMapping = new List<IPrimitiveTypeFormatter>().AddDefaultPrimitiveTypeFormatters(_characterEscaper)
                                                                                 .ToDictionary(f => f.TargetType);

            WriterInstructors = new List<IJsonWriterInstructor>().AddDefaultWriterInstructors(_primitiveTypeFormattersMapping,
                                                                                              _typeAnalyzer,
                                                                                              _collectionMetadataInstructor,
                                                                                              _objectMetadataInstructor);
        }

        /// <summary>
        ///     Configures to builder to use the specified JSON writer factory for the serializer.
        /// </summary>
        /// <param name="createWriterFactory">The delegate that creates the writer factory being injected into the serializer.</param>
        /// <returns>The builder for method chaining.</returns>
        public JsonSerializerBuilder WithWriterFactory(Func<IJsonWriterFactory> createWriterFactory)
        {
            _createWriterFactory = createWriterFactory;
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

            _characterEscaper = characterEscaper;

            foreach (var primitiveTypeFormatter in _primitiveTypeFormattersMapping.Values.OfType<ISetCharacterEscaper>())
            {
                primitiveTypeFormatter.CharacterEscaper = _characterEscaper;
            }

            return this;
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

            _typeAnalyzer = typeAnalyzer;

            foreach (var instructor in WriterInstructors.OfType<ISetTypeAnalyzer>())
            {
                instructor.TypeAnalyzer = typeAnalyzer;
            }

            return this;
        }

        /// <summary>
        ///     Configures the builder to use the specified formatters instead of the default ones.
        /// </summary>
        /// <param name="formatters">The list containing all formatters to be used.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatters" /> is null.</exception>
        public JsonSerializerBuilder WithPrimitiveTypeFormatters(IList<IPrimitiveTypeFormatter> formatters)
        {
            formatters.MustNotBeNull(nameof(formatters));

            _primitiveTypeFormattersMapping = formatters.ToDictionary(f => f.TargetType);

            foreach (var instructor in WriterInstructors.OfType<ISetPrimitiveTypeFormatters>())
            {
                instructor.PrimitiveTypeFormattersMapping = _primitiveTypeFormattersMapping;
            }

            return this;
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

            return this;
        }

        /// <summary>
        ///     Configures the builder to use the specified formatters instead of the default ones.
        /// </summary>
        /// <param name="formattersMapping">The dictionary containing all formatters to be used.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formattersMapping" /> is null.</exception>
        public JsonSerializerBuilder WithPrimitiveTypeFormatters(IDictionary<Type, IPrimitiveTypeFormatter> formattersMapping)
        {
            formattersMapping.MustNotBeNull(nameof(formattersMapping));

            _primitiveTypeFormattersMapping = formattersMapping;

            foreach (var instructor in WriterInstructors.OfType<ISetPrimitiveTypeFormatters>())
            {
                instructor.PrimitiveTypeFormattersMapping = _primitiveTypeFormattersMapping;
            }

            return this;
        }

        /// <summary>
        ///     Adds the specified JSON writer instructor after the instance with the given type.
        /// </summary>
        /// <typeparam name="T">The type whose instance should be before the given writer instructor.</typeparam>
        /// <param name="additionalWriterInstructor">The writer instructor to be inserted.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="additionalWriterInstructor" /> is null.</exception>
        public JsonSerializerBuilder AddWriterInstructorAfter<T>(IJsonWriterInstructor additionalWriterInstructor)
            where T : IJsonWriterInstructor
        {
            additionalWriterInstructor.MustNotBeNull(nameof(additionalWriterInstructor));

            var targetIndex = WriterInstructors.IndexOf(WriterInstructors.OfType<T>().First());
            if (targetIndex == -1)
                throw new ArgumentException($"The specified writer instructor {additionalWriterInstructor} cannot be added after the instructor {typeof (T)} because the latter was not found.");

            if (targetIndex == WriterInstructors.Count - 1)
                WriterInstructors.Add(additionalWriterInstructor);
            else
                WriterInstructors.Insert(targetIndex + 1, additionalWriterInstructor);

            return this;
        }

        /// <summary>
        ///     Adds the specified JSON writer instructor before the instance with the given type.
        /// </summary>
        /// <typeparam name="T">The type whose instance should be after the given writer instructor.</typeparam>
        /// <param name="additionalWriterInstructor">The writer instructor to be inserted.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="additionalWriterInstructor" /> is null.</exception>
        public JsonSerializerBuilder AddWriterInstructorBefore<T>(IJsonWriterInstructor additionalWriterInstructor)
            where T : IJsonWriterInstructor
        {
            additionalWriterInstructor.MustNotBeNull(nameof(additionalWriterInstructor));

            var targetIndex = WriterInstructors.IndexOf(WriterInstructors.OfType<T>().First());
            if (targetIndex == -1)
                throw new ArgumentException($"The specified writer instructor {additionalWriterInstructor} cannot be added before the instructor {typeof (T)} because the latter was not found.");

            WriterInstructors.Insert(targetIndex, additionalWriterInstructor);
            return this;
        }

        /// <summary>
        ///     Configures the builder to use an instance of JsonWriterFactory. You do not have to call this method by default (except you have already exchanged the writer factory).
        /// </summary>
        /// <returns>The builder for method chaining.</returns>
        public JsonSerializerBuilder UseDefaultWriterFactory()
        {
            _createWriterFactory = JsonWriterFactory.CreateDefault;
            return this;
        }

        /// <summary>
        ///     Configures the builder to inject the specified cache for JSON writer instructors into the serialized.
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

            configureInstructor(WriterInstructors.OfType<T>().First());
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

            configureFormatter(_primitiveTypeFormattersMapping.Values
                                                              .OfType<T>()
                                                              .First());

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

            var existingInstructor = WriterInstructors.OfType<CustomRuleInstructor>()
                                                      .First(i => i.TargetType == typeof (T));

            if (existingInstructor != null)
                WriterInstructors.Remove(existingInstructor);

            WriterInstructors.Insert(0, new CustomRuleInstructor(typeof (T), newRule.CreateValueProviders(), _objectMetadataInstructor));

            return this;
        }

        /// <summary>
        ///     Exchanges the existing object metadata instructor with the specified one.
        /// </summary>
        /// <param name="metadataInstructor">The new metadata instructor for complex JSON objects.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="metadataInstructor" /> is null.</exception>
        public JsonSerializerBuilder WithObjectMetadataInstructor(IMetadataInstructor metadataInstructor)
        {
            metadataInstructor.MustNotBeNull(nameof(metadataInstructor));

            _objectMetadataInstructor = metadataInstructor;

            foreach (var instructor in WriterInstructors.OfType<ISetObjectMetadataInstructor>())
            {
                instructor.MetadataInstructor = metadataInstructor;
            }

            return this;
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

            _collectionMetadataInstructor = metadataInstructor;

            foreach (var instructor in WriterInstructors.OfType<ISetCollectionMetadataInstructor>())
            {
                instructor.MetadataInstructor = _collectionMetadataInstructor;
            }

            return this;
        }

        /// <summary>
        ///     Configures the metadata instructor for complex JSON objects with the specified delegate. The default metadata instructor is of type TypeAndReferenceMetadataInstructor.
        /// </summary>
        /// <typeparam name="T">The actual type of the metadata instructor. This type must derive from IMetadataInstructor.</typeparam>
        /// <param name="configureInstructor">The delegate that configures the instructor.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configureInstructor" /> is null.</exception>
        public JsonSerializerBuilder ConfigureObjectMetadataInstructor<T>(Action<T> configureInstructor)
            where T : IMetadataInstructor
        {
            configureInstructor.MustNotBeNull(nameof(configureInstructor));

            configureInstructor((T) _objectMetadataInstructor);

            return this;
        }

        /// <summary>
        ///     Configures the metadata instructor for JSON arrays with the specified delegate. The default metadata instructor is of type CollectionReferenceMetadataInstructor.
        /// </summary>
        /// <typeparam name="T">The actual type of the metadata instructor. This type must derive from IMetadataInstructor.</typeparam>
        /// <param name="configureInstructor">The delegate that configures the instructor.</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configureInstructor" /> is null.</exception>
        public JsonSerializerBuilder ConfigureCollectionMetadataInstructor<T>(Action<T> configureInstructor)
        {
            configureInstructor.MustNotBeNull(nameof(configureInstructor));

            configureInstructor((T) _collectionMetadataInstructor);

            return this;
        }

        /// <summary>
        ///     Configures the metadata instructors to not include object IDs in the JSON document.
        /// </summary>
        /// <returns>The builder for method chaining.</returns>
        public JsonSerializerBuilder DisableObjectReferencePreservation()
        {
            SetObjectReferencePreservationIfPossible(_objectMetadataInstructor, false);
            SetObjectReferencePreservationIfPossible(_collectionMetadataInstructor, false);
            return this;
        }

        /// <summary>
        ///     Configures the metadata instructors to include object IDs in the JSON document. This is turned on by default.
        /// </summary>
        /// <returns>The builder for method chaining.</returns>
        public JsonSerializerBuilder EnableObjectReferencePreservation()
        {
            SetObjectReferencePreservationIfPossible(_objectMetadataInstructor, true);
            SetObjectReferencePreservationIfPossible(_collectionMetadataInstructor, true);
            return this;
        }

        private static void SetObjectReferencePreservationIfPossible(IMetadataInstructor instructor, bool value)
        {
            var metadataInstructor = instructor as ISetObjectReferencePreservationStatus;
            if (metadataInstructor == null)
                return;

            metadataInstructor.IsSerializingObjectIds = value;
        }

        /// <summary>
        ///     Configures the object metadata instructor to not include type information of complex .NET types.
        /// </summary>
        /// <returns>The builder for method chaining.</returns>
        public JsonSerializerBuilder DisableTypeMetadata()
        {
            SetTypeInfoSerializationStatus(_objectMetadataInstructor, false);
            SetTypeInfoSerializationStatus(_collectionMetadataInstructor, false);

            return this;
        }

        /// <summary>
        ///     Configures the metadata instructor to include type information for complex .NET types. This is turned on by default for the object metadata instructor (the default collection metadata instructor does not support this feature).
        /// </summary>
        /// <returns>The builder for method chaining.</returns>
        public JsonSerializerBuilder EnableTypeMetadata()
        {
            SetTypeInfoSerializationStatus(_objectMetadataInstructor, true);
            SetTypeInfoSerializationStatus(_collectionMetadataInstructor, true);

            return this;
        }

        private static void SetTypeInfoSerializationStatus(IMetadataInstructor instructor, bool value)
        {
            var metadataInstructor = instructor as ISetTypeInfoSerializationStatus;
            if (metadataInstructor == null)
                return;

            metadataInstructor.IsSerializingTypeInfo = value;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="JsonSerializer" /> using the specified builder settings.
        /// </summary>
        public JsonSerializer Build()
        {
            return new JsonSerializer(WriterInstructors, _createWriterFactory(), _instructorCache);
        }
    }
}