using System;
using System.Collections.Generic;
using System.Linq;
using Light.GuardClauses;
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
        private readonly IObjectMetadataInstructor _metadataInstructor = new TypeAndReferenceMetadataInstructor(new SimpleNameToTypeMapping());
        private readonly List<Rule> _rules = new List<Rule>();

        /// <summary>
        ///     Gets the list containing all basic writer instructors.
        /// </summary>
        public readonly List<IJsonWriterInstructor> BasicWriterInstructors;

        private ICharacterEscaper _characterEscaper = new DefaultCharacterEscaper();
        private Func<IList<IJsonWriterInstructor>> _createList = CreateDefaultList;
        private IDictionary<Type, IJsonWriterInstructor> _instructorCache;
        private IReadableValuesTypeAnalyzer _typeAnalyzer = new ValueProvidersCacheDecorator(new PublicPropertiesAndFieldsAnalyzer(), new Dictionary<Type, IList<IValueProvider>>());
        private IJsonWriterFactory _writerFactory;

        /// <summary>
        ///     Initializes a new instance of <see cref="JsonSerializerBuilder" />.
        /// </summary>
        public JsonSerializerBuilder()
        {
            UseDefaultWriterFactory();
            _instructorCache = new Dictionary<Type, IJsonWriterInstructor>();

            BasicWriterInstructors = new List<IJsonWriterInstructor>()
                .AddDefaultWriterInstructors(new List<IPrimitiveTypeFormatter>().AddDefaultPrimitiveTypeFormatters(_characterEscaper)
                                                                                .ToDictionary(f => f.TargetType),
                                             _typeAnalyzer,
                                             _metadataInstructor);
        }

        private static IList<IJsonWriterInstructor> CreateDefaultList()
        {
            return new List<IJsonWriterInstructor>();
        }

        /// <summary>
        ///     Configures to builder to use the specified JSON writer factory for the serializer.
        /// </summary>
        /// <param name="writerFactory">The writer factory to be injected into the serializer.</param>
        /// <returns>The builder for method chaining.</returns>
        public JsonSerializerBuilder WithWriterFactory(IJsonWriterFactory writerFactory)
        {
            _writerFactory = writerFactory;
            return this;
        }

        /// <summary>
        ///     Configures the builder to use the specified delegate to create a list for the inject collection of writer instructors.
        /// </summary>
        /// <param name="createList">The delegate that creates a list.</param>
        /// <returns>The builder for method chaining.</returns>
        public JsonSerializerBuilder WithListCreationFunction(Func<IList<IJsonWriterInstructor>> createList)
        {
            _createList = createList;
            return this;
        }

        /// <summary>
        ///     Configures the builder to use the specified character escaper for primitive type formatters.
        /// </summary>
        /// <param name="characterEscaper">The character escaper used for characters and strings.</param>
        /// <returns>The builder for method chaining.</returns>
        public JsonSerializerBuilder WithCharacterEscaper(ICharacterEscaper characterEscaper)
        {
            _characterEscaper = characterEscaper;

            ConfigureFormatterOfPrimitiveValueInstructor<CharFormatter>(f => f.CharacterEscaper = characterEscaper);
            ConfigureFormatterOfPrimitiveValueInstructor<StringFormatter>(f => f.CharacterEscaper = characterEscaper);

            return this;
        }

        /// <summary>
        ///     Configures the builder to use the specified type analyzer CustomRuleInstructors and the ComplexObjectInstructor.
        /// </summary>
        /// <param name="typeAnalyzer">The object that creates value providers for the given type.</param>
        /// <returns>The builder for method chaining.</returns>
        public JsonSerializerBuilder WithTypeAnalyzer(IReadableValuesTypeAnalyzer typeAnalyzer)
        {
            _typeAnalyzer = typeAnalyzer;

            var complexObjectInstructor = BasicWriterInstructors.OfType<ComplexObjectInstructor>().FirstOrDefault();
            if (complexObjectInstructor != null)
                complexObjectInstructor.TypeAnalyzer = _typeAnalyzer;

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

            var targetIndex = BasicWriterInstructors.IndexOf(BasicWriterInstructors.OfType<T>().First());
            if (targetIndex == -1)
                throw new ArgumentException($"The specified writer instructor {additionalWriterInstructor} cannot be added after the instructor {typeof (T)} because the latter was not found.");

            if (targetIndex == BasicWriterInstructors.Count - 1)
                BasicWriterInstructors.Add(additionalWriterInstructor);
            else
                BasicWriterInstructors.Insert(targetIndex + 1, additionalWriterInstructor);

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

            var targetIndex = BasicWriterInstructors.IndexOf(BasicWriterInstructors.OfType<T>().First());
            if (targetIndex == -1)
                throw new ArgumentException($"The specified writer instructor {additionalWriterInstructor} cannot be added before the instructor {typeof (T)} because the latter was not found.");

            BasicWriterInstructors.Insert(targetIndex, additionalWriterInstructor);
            return this;
        }

        /// <summary>
        ///     Configures the builder to use an instance of <see cref="JsonWriterFactory" />. You do not have to call this method by default (except you have already exchanged the writer factory).
        /// </summary>
        /// <returns>The builder for method chaining.</returns>
        public JsonSerializerBuilder UseDefaultWriterFactory()
        {
            _writerFactory = new JsonWriterFactory();
            return this;
        }

        /// <summary>
        ///     Configures the default <see cref="JsonWriterFactory" /> instance using the specified delegate.
        /// </summary>
        /// <param name="configureFactory">The delegate that configures the JSON writer factory.</param>
        /// <returns>The builder for method chaining.</returns>
        public JsonSerializerBuilder ConfigureDefaultWriterFactory(Action<JsonWriterFactory> configureFactory)
        {
            configureFactory((JsonWriterFactory) _writerFactory);
            return this;
        }

        /// <summary>
        ///     Configures the builder to inject the specified cache for JSON writer instructors into the serialized.
        /// </summary>
        /// <param name="instructorCache">The dictionary used as the instructorCache.</param>
        /// <returns>The builder for method chaining.</returns>
        public JsonSerializerBuilder WithInstructorCache(IDictionary<Type, IJsonWriterInstructor> instructorCache)
        {
            _instructorCache = instructorCache;
            return this;
        }

        /// <summary>
        ///     Configures the JSON writer instructor with the given type using the specified delegate.
        /// </summary>
        /// <typeparam name="T">The type of the writer instructor that should be configured.</typeparam>
        /// <param name="configureInstructor">The delegate that configures the actual instructor.</param>
        /// <returns>The builder for method chaining.</returns>
        public JsonSerializerBuilder ConfigureInstructor<T>(Action<T> configureInstructor)
            where T : IJsonWriterInstructor
        {
            configureInstructor(BasicWriterInstructors.OfType<T>().First());
            return this;
        }

        /// <summary>
        ///     Configures the primitive type formatter with the given type using the specified delegate.
        /// </summary>
        /// <typeparam name="T">The type of the formatter that should be configured.</typeparam>
        /// <param name="configureFormatter">The delegate that configures the actual formatter instance.</param>
        /// <returns>The builder for method chaining.</returns>
        public JsonSerializerBuilder ConfigureFormatterOfPrimitiveValueInstructor<T>(Action<T> configureFormatter)
            where T : IPrimitiveTypeFormatter
        {
            configureFormatter(BasicWriterInstructors.OfType<PrimitiveValueInstructor>()
                                                     .First()
                                                     .PrimitiveTypeToFormattersMapping
                                                     .Values
                                                     .OfType<T>()
                                                     .First());
            return this;
        }

        /// <summary>
        ///     Creates a new serialization rule for the given type that is configured with the specified delegate.
        ///     An existing rule for the specified type will be replaced.
        /// </summary>
        /// <typeparam name="T">The type that should be configured for serialization.</typeparam>
        /// <param name="configureRule">The delegate that configures the serialization rule.</param>
        /// <returns>The builder for method chaining.</returns>
        public JsonSerializerBuilder WithRuleFor<T>(Action<Rule<T>> configureRule)
        {
            var targetType = typeof (T);

            var newRule = new Rule<T>(_typeAnalyzer);
            configureRule(newRule);

            var existingRuleIndex = _rules.FindIndex(r => r.TargetType == targetType);
            if (existingRuleIndex != -1)
                _rules.RemoveAt(existingRuleIndex);

            _rules.Add(newRule);

            return this;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="JsonSerializer" /> using the specified builder settings.
        /// </summary>
        public JsonSerializer Build()
        {
            var writerInstructors = _createList();
            foreach (var rule in _rules)
            {
                writerInstructors.Add(new CustomRuleInstructor(rule.TargetType, rule.CreateValueProviders(), _metadataInstructor));
            }
            foreach (var instructor in BasicWriterInstructors)
            {
                writerInstructors.Add(instructor);
            }
            return new JsonSerializer((IReadOnlyList<IJsonWriterInstructor>) writerInstructors, _writerFactory, _instructorCache);
        }
    }
}