using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Light.DependencyInjection;
using Light.GuardClauses;
using Light.Serialization.Abstractions;
using Light.Serialization.Json.Caching;
using Light.Serialization.Json.ComplexTypeConstruction;
using Light.Serialization.Json.ComplexTypeDecomposition;
using Light.Serialization.Json.LowLevelReading;
using Light.Serialization.Json.LowLevelWriting;
using Light.Serialization.Json.ObjectMetadata;
using Light.Serialization.Json.PrimitiveTypeFormatters;
using Light.Serialization.Json.SerializationRules;
using Light.Serialization.Json.TokenParsers;
using Light.Serialization.Json.WriterInstructors;

namespace Light.Serialization.Json.WithDependencyInjection
{
    /// <summary>
    ///     Provides extension methods to easily register the default serialization and deserialization types with the <see cref="DependencyInjectionContainer" />.
    /// </summary>
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        ///     Gets the registration name of the boolean value that identifies if the metadata instructors serialize object ids.
        /// </summary>
        public const string IsSerializingObjectIds = "IsSerializingObjectIds";

        /// <summary>
        ///     Gets the registration name of the boolean value that identifies if the metadata instructors serialize type metadata.
        /// </summary>
        public const string IsSerializingTypeInfo = "IsSerializingTypeInfo";

        /// <summary>
        ///     Registers the default serialization types of Light.Serialization.Json with the <see cref="DependencyInjectionContainer" />.
        /// </summary>
        /// <param name="container">The container that will be populated.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        public static DependencyInjectionContainer RegisterDefaultSerializationTypes(this DependencyInjectionContainer container)
        {
            container.MustNotBeNull(nameof(container));

            // JSON serializer
            container.RegisterTransient<ISerializer, JsonSerializer>();

            // Writer instructor cache for JSON serializer
            container.RegisterSingleton<IDictionary<Type, IJsonWriterInstructor>, Dictionary<Type, IJsonWriterInstructor>>(options => options.UseDefaultConstructor());

            // JSON writer factory
            container.RegisterSingleton<IJsonWriterFactory, JsonWriterFactoryUsingDi>()
                     .RegisterTransient<IJsonWhitespaceFormatter, WhitespaceFormatterNullObject>()
                     .RegisterSingleton<IJsonKeyNormalizer, FirstCharacterToLowerAndRemoveAllSpecialCharactersNormalizer>();

            // JSON writer instructors
            container.RegisterTransient<IReadOnlyList<IJsonWriterInstructor>, IJsonWriterInstructor[]>(options => options.InstantiateWith(() => new IJsonWriterInstructor[]
                                                                                                                                                {
                                                                                                                                                    container.Resolve<PrimitiveValueInstructor>(),
                                                                                                                                                    container.Resolve<EnumInstructor>(),
                                                                                                                                                    container.Resolve<TypeAndTypeInfoInstructor>(),
                                                                                                                                                    container.Resolve<DictionaryInstructor>(),
                                                                                                                                                    container.Resolve<CollectionInstructor>(),
                                                                                                                                                    container.Resolve<ComplexObjectInstructor>()
                                                                                                                                                }));
            container.RegisterSingleton<PrimitiveValueInstructor>()
                     .RegisterSingleton<EnumInstructor>()
                     .RegisterSingleton<TypeAndTypeInfoInstructor>(options => options.ResolveInstantiationParameter<ITypeMetadataInstructor>().WithName(typeof(ObjectMetadataInstructor).Name))
                     .RegisterSingleton<DictionaryInstructor>(options => options.ResolveInstantiationParameter<IMetadataInstructor>().WithName(typeof(ObjectMetadataInstructor).Name))
                     .RegisterSingleton<CollectionInstructor>(options => options.ResolveInstantiationParameter<IMetadataInstructor>().WithName(typeof(ArrayMetadataInstructor).Name))
                     .RegisterSingleton<ComplexObjectInstructor>(options => options.ResolveInstantiationParameter<IMetadataInstructor>().WithName(typeof(ObjectMetadataInstructor).Name));

            // Primitive type formatters
            container.RegisterSingleton<IDictionary<Type, IPrimitiveTypeFormatter>, Dictionary<Type, IPrimitiveTypeFormatter>>(options => options.InstantiateWith(() => container.ResolveAll<IPrimitiveTypeFormatter>().ToDictionary(formatter => formatter.TargetType)))
                     .RegisterSingleton<IPrimitiveTypeFormatter, ToStringFormatter<int>>(options => options.UseRegistrationName("IntFormatter")
                                                                                                           .InstantiateWith(() => new ToStringFormatter<int>(false)))
                     .RegisterSingleton<IPrimitiveTypeFormatter, StringFormatter>(options => options.UseTypeNameAsRegistrationName())
                     .RegisterSingleton<IPrimitiveTypeFormatter, DoubleFormatter>(options => options.UseTypeNameAsRegistrationName())
                     .RegisterSingleton<IPrimitiveTypeFormatter, DateTimeFormatter>(options => options.UseTypeNameAsRegistrationName())
                     .RegisterSingleton<IPrimitiveTypeFormatter, DateTimeOffsetFormatter>(options => options.UseTypeNameAsRegistrationName())
                     .RegisterSingleton<IPrimitiveTypeFormatter, TimeSpanFormatter>(options => options.UseTypeNameAsRegistrationName())
                     .RegisterSingleton<IPrimitiveTypeFormatter, ToStringWithQuotationMarksFormatter<Guid>>(options => options.UseRegistrationName("GuidFormatter")
                                                                                                                              .InstantiateWith(() => new ToStringWithQuotationMarksFormatter<Guid>(false)))
                     .RegisterSingleton<IPrimitiveTypeFormatter, BooleanFormatter>(options => options.UseTypeNameAsRegistrationName())
                     .RegisterSingleton<IPrimitiveTypeFormatter, DecimalFormatter>(options => options.UseTypeNameAsRegistrationName())
                     .RegisterSingleton<IPrimitiveTypeFormatter, ToStringFormatter<long>>(options => options.UseRegistrationName("LongFormatter")
                                                                                                            .InstantiateWith(() => new ToStringFormatter<long>(false)))
                     .RegisterSingleton<IPrimitiveTypeFormatter, FloatFormatter>(options => options.UseTypeNameAsRegistrationName())
                     .RegisterSingleton<IPrimitiveTypeFormatter, CharFormatter>(options => options.UseTypeNameAsRegistrationName())
                     .RegisterSingleton<IPrimitiveTypeFormatter, ToStringFormatter<short>>(options => options.UseRegistrationName("ShortFormatter")
                                                                                                             .InstantiateWith(() => new ToStringFormatter<short>(false)))
                     .RegisterSingleton<IPrimitiveTypeFormatter, ToStringFormatter<byte>>(options => options.UseRegistrationName("ByteFormatter")
                                                                                                            .InstantiateWith(() => new ToStringFormatter<byte>(false)))
                     .RegisterSingleton<IPrimitiveTypeFormatter, ToStringFormatter<uint>>(options => options.UseRegistrationName("UIntFormatter")
                                                                                                            .InstantiateWith(() => new ToStringFormatter<uint>(false)))
                     .RegisterSingleton<IPrimitiveTypeFormatter, ToStringFormatter<ulong>>(options => options.UseRegistrationName("ULongFormatter")
                                                                                                             .InstantiateWith(() => new ToStringFormatter<ulong>(false)))
                     .RegisterSingleton<IPrimitiveTypeFormatter, ToStringFormatter<ushort>>(options => options.UseRegistrationName("UShortFormatter")
                                                                                                              .InstantiateWith(() => new ToStringFormatter<ushort>(false)))
                     .RegisterSingleton<IPrimitiveTypeFormatter, ToStringFormatter<sbyte>>(options => options.UseRegistrationName("SByteFormatter")
                                                                                                             .InstantiateWith(() => new ToStringFormatter<sbyte>(false)));

            // Character escaper for char and string formatters
            container.RegisterSingleton<ICharacterEscaper, DefaultCharacterEscaper>(options => options.UseDefaultConstructor());

            // Type analyzer for complex .NET types
            container.RegisterSingleton<IReadableValuesTypeAnalyzer, ValueReadersCacheDecorator>(options => options.InstantiateWith(() => new ValueReadersCacheDecorator(new PublicPropertiesAndFieldsAnalyzer(),
                                                                                                                                                                         new Dictionary<Type, List<IValueReader>>())));

            // Metadata instructors for complex .NET types and collections
            container.RegisterSingleton<ObjectMetadataInstructor>(options => options.UseTypeNameAsRegistrationName()
                                                                                    .MapToAllImplementedInterfaces()
                                                                                    .AddPropertyInjection(o => o.IsSerializingObjectIds, IsSerializingObjectIds)
                                                                                    .AddPropertyInjection(o => o.IsSerializingTypeInfo, IsSerializingTypeInfo))
                     .RegisterSingleton<ArrayMetadataInstructor>(options => options.UseTypeNameAsRegistrationName()
                                                                                   .MapToAllImplementedInterfaces()
                                                                                   .AddPropertyInjection(o => o.IsSerializingObjectIds, IsSerializingObjectIds)
                                                                                   .AddPropertyInjection(o => o.IsSerializingTypeInfo, IsSerializingTypeInfo))
                     .RegisterInstance(true, options => options.UseRegistrationName(IsSerializingObjectIds))
                     .RegisterInstance(true, options => options.UseRegistrationName(IsSerializingTypeInfo));


            // Type to name mapping
            container.RegisterSingleton<SimpleNameToTypeMapping>(options => options.MapToAllImplementedInterfaces());

            return container;
        }

        /// <summary>
        ///     Registers the default deserialization types with the DI container.
        /// </summary>
        /// <param name="container">The container that will be populated.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        public static DependencyInjectionContainer RegisterDefaultDeserializationTypes(this DependencyInjectionContainer container)
        {
            container.MustNotBeNull(nameof(container));

            // JSON deserializer
            container.RegisterTransient<IDeserializer, JsonDeserializer>();

            // JSON reader factory
            container.RegisterSingleton<IJsonReaderFactory, JsonReaderFactory>();

            // Token Parser Cache
            container.RegisterSingleton<Dictionary<JsonTokenTypeCombination, IJsonTokenParser>>(options => options.UseDefaultConstructor());

            // Token parsers
            container.RegisterSingleton<SignedIntegerParser>()
                     .RegisterSingleton<DoubleParser>()
                     .RegisterSingleton<BooleanParser>()
                     .RegisterSingleton<CharacterParser>()
                     .RegisterSingleton<DateTimeParser>()
                     .RegisterSingleton<DateTimeOffsetParser>()
                     .RegisterSingleton<TimeSpanParser>()
                     .RegisterSingleton<UnsignedIntegerParser>()
                     .RegisterSingleton<FloatParser>()
                     .RegisterSingleton<DecimalParser>()
                     .RegisterSingleton<NullParser>()
                     .RegisterSingleton<EnumParser>()
                     .RegisterSingleton<StringParser>()
                     .RegisterSingleton<GuidParser>()
                     .RegisterSingleton<NullableParser>()
                     .RegisterTransient<JsonStringInheritanceParser>()
                     .RegisterSingleton<TypeAndTypeInfoParser>()
                     .RegisterSingleton<CollectionParser>()
                     .RegisterSingleton<DictionaryParser>()
                     .RegisterSingleton<ComplexObjectParserUsingDi>();

            container.RegisterTransient<IReadOnlyList<IJsonTokenParser>, IJsonTokenParser[]>(options => options.InstantiateWith(() => new IJsonTokenParser[]
                                                                                                                                      {
                                                                                                                                          container.Resolve<SignedIntegerParser>(),
                                                                                                                                          container.Resolve<DoubleParser>(),
                                                                                                                                          container.Resolve<BooleanParser>(),
                                                                                                                                          container.Resolve<CharacterParser>(),
                                                                                                                                          container.Resolve<DateTimeParser>(),
                                                                                                                                          container.Resolve<DateTimeOffsetParser>(),
                                                                                                                                          container.Resolve<TimeSpanParser>(),
                                                                                                                                          container.Resolve<UnsignedIntegerParser>(),
                                                                                                                                          container.Resolve<FloatParser>(),
                                                                                                                                          container.Resolve<DecimalParser>(),
                                                                                                                                          container.Resolve<NullParser>(),
                                                                                                                                          container.Resolve<EnumParser>(),
                                                                                                                                          container.Resolve<StringParser>(),
                                                                                                                                          container.Resolve<GuidParser>(),
                                                                                                                                          container.Resolve<NullableParser>(),
                                                                                                                                          container.Resolve<JsonStringInheritanceParser>(),
                                                                                                                                          container.Resolve<TypeAndTypeInfoParser>(),
                                                                                                                                          container.Resolve<CollectionParser>(),
                                                                                                                                          container.Resolve<DictionaryParser>(),
                                                                                                                                          container.Resolve<ComplexObjectParserUsingDi>()
                                                                                                                                      }));
            container.RegisterTransient<IReadOnlyList<IJsonStringToPrimitiveParser>, IJsonStringToPrimitiveParser[]>(options => options.InstantiateWith(() => new IJsonStringToPrimitiveParser[]
                                                                                                                                                              {
                                                                                                                                                                  container.Resolve<SignedIntegerParser>(),
                                                                                                                                                                  container.Resolve<DoubleParser>(),
                                                                                                                                                                  container.Resolve<BooleanParser>(),
                                                                                                                                                                  container.Resolve<CharacterParser>(),
                                                                                                                                                                  container.Resolve<DateTimeParser>(),
                                                                                                                                                                  container.Resolve<DateTimeOffsetParser>(),
                                                                                                                                                                  container.Resolve<TimeSpanParser>(),
                                                                                                                                                                  container.Resolve<GuidParser>()
                                                                                                                                                              }));

            // Meta factory
            container.RegisterSingleton<IMetaFactory, DependencyInjectionMetaFactory>();

            // Metadata parsers


            container.RegisterSingleton<ObjectMetadataParser>(options => options.MapToAllImplementedInterfaces())
                     .RegisterSingleton<IArrayMetadataParser, ArrayMetadataParser>()
                     .RegisterSingleton<SimpleNameToTypeMapping>(options => options.MapToAllImplementedInterfaces());

            // Type description service
            container.RegisterSingleton<ITypeDescriptionService, DefaultTypeDescriptionServiceWithCaching>()
                     .RegisterSingleton<Dictionary<Type, TypeCreationDescription>>(options => options.UseDefaultConstructor());

            return container;
        }

        /// <summary>
        ///     Replaces the currently registered <see cref="IJsonWhitespaceFormatter" /> with the <see cref="IndentingWhitespaceFormatter" />, producing human-readable indented JSON documents.
        /// </summary>
        /// <param name="container">The container that will be populated.</param>
        /// <returns>The container for method-chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        public static DependencyInjectionContainer UseIndentingWhitespaceFormatterForSerialization(this DependencyInjectionContainer container)
        {
            container.MustNotBeNull(nameof(container));

            return container.RegisterTransient<IJsonWhitespaceFormatter, IndentingWhitespaceFormatter>();
        }

        /// <summary>
        ///     Configures the JSON serializer and deserializer to use a <see cref="DomainFriendlyNameMapping" /> for the specified types.
        /// </summary>
        /// <param name="container">The container to be populated.</param>
        /// <param name="configureMapping">The delegate that configures which types will be scanned.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> or <paramref name="configureMapping" /> is null.</exception>
        public static DependencyInjectionContainer UseDomainFriendlyNames(this DependencyInjectionContainer container, Action<TypeNameToJsonNameScanner.IScanningOptions> configureMapping)
        {
            container.MustNotBeNull(nameof(container));
            configureMapping.MustNotBeNull(nameof(configureMapping));

            var domainFriendlyNameMapping = DomainFriendlyNameMapping.CreateWithDefaultTypeMappings()
                                                                     .ScanTypes(configureMapping);

            return container.UseDomainFriendlyNames(domainFriendlyNameMapping);
        }

        /// <summary>
        ///     Configures the JSON serializer and deserializer to use the specified <see cref="DomainFriendlyNameMapping" />.
        /// </summary>
        /// <param name="container">The container to be populated.</param>
        /// <param name="mapping">The mapping that replaces the current one.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> or <paramref name="mapping" /> is null.</exception>
        public static DependencyInjectionContainer UseDomainFriendlyNames(this DependencyInjectionContainer container, DomainFriendlyNameMapping mapping)
        {
            container.MustNotBeNull(nameof(container));
            mapping.MustNotBeNull(nameof(mapping));

            return container.RegisterInstance(mapping, options => options.MapToAllImplementedInterfaces());
        }

        /// <summary>
        ///     Configures the container to disable object reference preservation on the JSON serializer.
        /// </summary>
        /// <param name="container">The container to be populated.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        public static DependencyInjectionContainer DisableObjectReferencePreservation(this DependencyInjectionContainer container)
        {
            return container.SetObjectReferencePreservationStatus(false);
        }

        /// <summary>
        ///     Configures the container to enable object reference preservation on the JSON serializer. This feature is turned on by default.
        /// </summary>
        /// <param name="container">The container to be populated.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        public static DependencyInjectionContainer EnableObjectReferencePreservation(this DependencyInjectionContainer container)
        {
            return container.SetObjectReferencePreservationStatus(true);
        }

        /// <summary>
        ///     Sets the given boolean as the object reference preservation status for the JSON serializer.
        /// </summary>
        /// <param name="container">The container to be populated.</param>
        /// <param name="value">The value for object reference preservation.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        public static DependencyInjectionContainer SetObjectReferencePreservationStatus(this DependencyInjectionContainer container, bool value)
        {
            container.MustNotBeNull(nameof(container));

            return container.RegisterInstance(value, options => options.UseRegistrationName(IsSerializingObjectIds));
        }

        /// <summary>
        ///     The JSON serializer will not emit no type metadata in the resulting JSON document if this option is disabled.
        /// </summary>
        /// <param name="container">The container to be populated.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        public static DependencyInjectionContainer DisableTypeMetadata(this DependencyInjectionContainer container)
        {
            return container.SetTypeMetadataStatus(false);
        }

        /// <summary>
        ///     The JSON serializer will emit type metadata in the resulting JSON document if this option is enabled.
        /// </summary>
        /// <param name="container">The container to be populated.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        public static DependencyInjectionContainer EnableTypeMetadata(this DependencyInjectionContainer container)
        {
            return container.SetTypeMetadataStatus(true);
        }

        /// <summary>
        ///     Sets the boolean value indicating whether the JSON serializer will emit type metadata or not.
        /// </summary>
        /// <param name="container">The container to be populated.</param>
        /// <param name="value">The value for type metadata status.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        public static DependencyInjectionContainer SetTypeMetadataStatus(this DependencyInjectionContainer container, bool value)
        {
            container.MustNotBeNull(nameof(container));

            return container.RegisterInstance(value, options => options.UseRegistrationName(IsSerializingTypeInfo));
        }

        /// <summary>
        ///     Registers <see cref="List{T}" />, <see cref="ObservableCollection{T}" />, and <see cref="Collection{T}" /> with the DI container.
        ///     <see cref="IList{T}" />, <see cref="ICollection{T}" />, and <see cref="IEnumerable{T}" /> are mapped to <see cref="List{T}" />.
        /// </summary>
        /// <param name="container">The container to be populated.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        public static DependencyInjectionContainer RegisterDefaultCollectionTypes(this DependencyInjectionContainer container)
        {
            container.MustNotBeNull(nameof(container));

            return container.RegisterTransient(typeof(List<>), options => options.UseDefaultConstructor()
                                                                                 .MapToAbstractions(typeof(IList<>), typeof(ICollection<>), typeof(IEnumerable<>)))
                            .RegisterTransient(typeof(ObservableCollection<>), options => options.UseDefaultConstructor())
                            .RegisterTransient(typeof(Collection<>), options => options.UseDefaultConstructor());
        }

        /// <summary>
        ///     Registers <see cref="Dictionary{TKey,TValue}" /> and <see cref="SortedDictionary{TKey,TValue}" /> with the DI container.
        ///     <see cref="IDictionary{TKey,TValue}" /> is mapped to <see cref="Dictionary{TKey,TValue}" />.
        /// </summary>
        /// <param name="container">The container to be populated.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        public static DependencyInjectionContainer RegisterDefaultDictionaryTypes(this DependencyInjectionContainer container)
        {
            container.MustNotBeNull(nameof(container));

            return container.RegisterTransient(typeof(Dictionary<,>), options => options.UseDefaultConstructor()
                                                                                        .MapToAbstractions(typeof(IDictionary<,>)))
                            .RegisterTransient(typeof(SortedDictionary<,>), options => options.UseDefaultConstructor());
        }

        /// <summary>
        ///     Registers <see cref="HashSet{T}" /> and <see cref="SortedSet{T}" /> with the DI container.
        ///     <see cref="ISet{T}" /> is mapped to <see cref="HashSet{T}" />.
        /// </summary>
        /// <param name="container">The container to be populated.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        public static DependencyInjectionContainer RegisterDefaultSetTypes(this DependencyInjectionContainer container)
        {
            container.MustNotBeNull(nameof(container));

            return container.RegisterTransient(typeof(HashSet<>), options => options.UseDefaultConstructor()
                                                                                    .MapToAbstractions(typeof(ISet<>)))
                            .RegisterTransient(typeof(SortedSet<>), options => options.UseDefaultConstructor());
        }

        /// <summary>
        ///     Creates a serialization rule for the given type that is configured with the specified delegate.
        /// </summary>
        /// <typeparam name="T">The type that should be configured for serialization.</typeparam>
        /// <param name="container">The dependency injection container.</param>
        /// <param name="configureRule">The delegate that configures the actual formatter instance.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public static DependencyInjectionContainer WithSerializationRuleFor<T>(this DependencyInjectionContainer container, Action<Rule<T>> configureRule)
        {
            container.MustNotBeNull(nameof(container));
            configureRule.MustNotBeNull(nameof(configureRule));

            var newRule = container.Resolve<Rule<T>>();
            configureRule(newRule);
            var customInstructor = new CustomRuleInstructor(newRule.TargetType,
                                                            newRule.CreateValueReaders(),
                                                            container.Resolve<IMetadataInstructor>(typeof(ObjectMetadataInstructor).Name));
            container.RegisterInstance(customInstructor, options => options.UseRegistrationName(typeof(T).FullName));
            return container;
        }

        /// <summary>
        ///     Configures the container to use the specified rule for serialization.
        /// </summary>
        /// <typeparam name="T">The type that should be configured for serialization.</typeparam>
        /// <param name="container">The dependency injection container.</param>
        /// <param name="rule">The rule that describes how the type is serialized.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters is null.</exception>
        public static DependencyInjectionContainer WithSerializationRuleFor<T>(this DependencyInjectionContainer container, Rule<T> rule)
        {
            container.MustNotBeNull(nameof(container));
            rule.MustNotBeNull(nameof(rule));

            var customInstructor = new CustomRuleInstructor(rule.TargetType, rule.CreateValueReaders(), container.Resolve<IMetadataInstructor>(typeof(ObjectMetadataInstructor).Name));
            container.RegisterInstance(customInstructor, options => options.UseRegistrationName(typeof(T).FullName));
            return container;
        }
    }
}