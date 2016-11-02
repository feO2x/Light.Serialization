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
using Light.Serialization.Json.TokenParsers;
using Light.Serialization.Json.WriterInstructors;

namespace Light.Serialization.Json.WithDependencyInjection
{
    /// <summary>
    ///     Provides extension methods to easily register the default serialization and deserialization types with the <see cref="DependencyInjectionContainer" />.
    /// </summary>
    public static class DependencyInjectionExtensions
    {
        public const string IsSerializingObjectIds = "IsSerializingObjectIds";
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
            container.RegisterTransient<ISerializer, JsonSerializer>(options => options.ResolveAllForInstantiationParameter<IReadOnlyList<IJsonWriterInstructor>>());

            // Writer instructor cache for JSON serializer
            container.RegisterSingleton<IDictionary<Type, IJsonWriterInstructor>, Dictionary<Type, IJsonWriterInstructor>>(options => options.UseDefaultConstructor());

            // JSON writer factory
            container.RegisterTransient<IJsonWriterFactory, JsonWriterFactory>()
                     .RegisterTransient<IJsonWhitespaceFormatter, WhitespaceFormatterNullObject>()
                     .RegisterSingleton<IJsonKeyNormalizer, FirstCharacterToLowerAndRemoveAllSpecialCharactersNormalizer>();

            // JSON writer instructors
            container.RegisterSingleton<IJsonWriterInstructor, PrimitiveValueInstructor>(options => options.UseTypeNameAsRegistrationName())
                     .RegisterSingleton<IJsonWriterInstructor, EnumInstructor>(options => options.UseTypeNameAsRegistrationName())
                     .RegisterSingleton<IJsonWriterInstructor, TypeAndTypeInfoInstructor>(options => options.UseTypeNameAsRegistrationName())
                     .RegisterSingleton<IJsonWriterInstructor, DictionaryInstructor>(options => options.UseTypeNameAsRegistrationName()
                                                                                                       .ResolveInstantiationParameter<IMetadataInstructor>().WithName(typeof(ObjectMetadataInstructor).Name))
                     .RegisterSingleton<IJsonWriterInstructor, CollectionInstructor>(options => options.UseTypeNameAsRegistrationName()
                                                                                                       .ResolveInstantiationParameter<IMetadataInstructor>().WithName(typeof(ArrayMetadataInstructor).Name))
                     .RegisterSingleton<IJsonWriterInstructor, ComplexObjectInstructor>(options => options.UseTypeNameAsRegistrationName()
                                                                                                          .ResolveInstantiationParameter<IMetadataInstructor>().WithName(typeof(ObjectMetadataInstructor).Name));

            // Primitive type formatters
            container.RegisterSingleton<IDictionary<Type, IPrimitiveTypeFormatter>>(options => options.InstantiateWith(() => container.ResolveAll<IPrimitiveTypeFormatter>().ToDictionary(formatter => formatter.TargetType)))
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

        public static DependencyInjectionContainer RegisterDefaultDeserializationTypes(this DependencyInjectionContainer container)
        {
            container.MustNotBeNull(nameof(container));

            // JSON deserializer
            container.RegisterTransient<IDeserializer, JsonDeserializer>(options => options.ResolveAllForInstantiationParameter<IReadOnlyList<IJsonTokenParser>>());

            // JSON reader factory
            container.RegisterSingleton<IJsonReaderFactory, JsonReaderFactory>();

            // Token Parser Cache
            container.RegisterSingleton<Dictionary<JsonTokenTypeCombination, IJsonTokenParser>>(options => options.UseDefaultConstructor());

            // Token parsers
            var tokenParserTypes = new[]
                                   {
                                       typeof(SignedIntegerParser),
                                       typeof(DecimalParser),
                                       typeof(BooleanParser),
                                       typeof(CharacterParser),
                                       typeof(DateTimeParser),
                                       typeof(DateTimeOffsetParser),
                                       typeof(TimeSpanParser),
                                       typeof(UnsignedIntegerParser),
                                       typeof(FloatParser),
                                       typeof(DecimalParser),
                                       typeof(NullParser),
                                       typeof(EnumParser),
                                       typeof(StringParser),
                                       typeof(GuidParser),
                                       typeof(NullableParser),
                                       typeof(JsonStringInheritanceParser),
                                       typeof(TypeAndTypeInfoParser),
                                       typeof(CollectionParser),
                                       typeof(DictionaryParser),
                                       typeof(ComplexObjectParserUsingDi)
                                   };

            foreach (var tokenParserType in tokenParserTypes)
            {
                if (tokenParserType != typeof(JsonStringInheritanceParser))
                    container.RegisterSingleton(tokenParserType, options => options.UseTypeNameAsRegistrationName()
                                                                                   .MapToAllImplementedInterfaces());
                else
                    container.RegisterTransient<JsonStringInheritanceParser>(options => options.UseTypeNameAsRegistrationName()
                                                                                               .ResolveAllForInstantiationParameter<IReadOnlyList<IJsonStringToPrimitiveParser>>()
                                                                                               .MapToAllImplementedInterfaces());
            }

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

        public static DependencyInjectionContainer UseIndentingWhitespaceFormatterForSerialization(this DependencyInjectionContainer container)
        {
            container.MustNotBeNull(nameof(container));

            return container.RegisterTransient<IJsonWhitespaceFormatter, IndentingWhitespaceFormatter>();
        }

        public static DependencyInjectionContainer UseDomainFriendlyNames(this DependencyInjectionContainer container, Action<TypeNameToJsonNameScanner.IScanningOptions> configureMapping)
        {
            container.MustNotBeNull(nameof(container));
            configureMapping.MustNotBeNull(nameof(configureMapping));

            var domainFriendlyNameMapping = DomainFriendlyNameMapping.CreateWithDefaultTypeMappings()
                                                                     .ScanTypes(configureMapping);

            return container.UseDomainFriendlyNames(domainFriendlyNameMapping);
        }

        public static DependencyInjectionContainer UseDomainFriendlyNames(this DependencyInjectionContainer container, DomainFriendlyNameMapping mapping)
        {
            container.MustNotBeNull(nameof(container));
            container.MustNotBeNull(nameof(mapping));

            return container.RegisterInstance(mapping, options => options.MapToAllImplementedInterfaces());
        }

        public static DependencyInjectionContainer DisableObjectReferencePreservation(this DependencyInjectionContainer container)
        {
            return container.SetObjectReferencePreservationStatus(false);
        }

        public static DependencyInjectionContainer EnableObjectReferencePreservation(this DependencyInjectionContainer container)
        {
            return container.SetObjectReferencePreservationStatus(true);
        }

        public static DependencyInjectionContainer SetObjectReferencePreservationStatus(this DependencyInjectionContainer container, bool value)
        {
            container.MustNotBeNull(nameof(container));

            container.RegisterInstance(value, options => options.UseRegistrationName(IsSerializingObjectIds));
            
            return container;
        }

        public static DependencyInjectionContainer DisableTypeMetadata(this DependencyInjectionContainer container)
        {
            return container.SetTypeMetadataStatus(false);
        }

        public static DependencyInjectionContainer EnableTypeMetadata(this DependencyInjectionContainer container)
        {
            return container.SetTypeMetadataStatus(true);
        }

        public static DependencyInjectionContainer SetTypeMetadataStatus(this DependencyInjectionContainer container, bool value)
        {
            container.MustNotBeNull(nameof(container));



            return container;
        }

        public static DependencyInjectionContainer RegisterDefaultCollectionTypes(this DependencyInjectionContainer container)
        {
            container.MustNotBeNull(nameof(container));

            return container.RegisterTransient(typeof(List<>), options => options.UseDefaultConstructor()
                                                                                 .MapToAbstractions(typeof(IList<>), typeof(ICollection<>)))
                            .RegisterTransient(typeof(ObservableCollection<>), options => options.UseDefaultConstructor())
                            .RegisterTransient(typeof(Collection<>), options => options.UseDefaultConstructor());
        }

        public static DependencyInjectionContainer RegisterDefaultDictionaryTypes(this DependencyInjectionContainer container)
        {
            container.MustNotBeNull(nameof(container));

            return container.RegisterTransient(typeof(Dictionary<,>), options => options.UseDefaultConstructor()
                                                                                        .MapToAbstractions(typeof(IDictionary<,>)))
                            .RegisterTransient(typeof(SortedDictionary<,>), options => options.UseDefaultConstructor());
        }

        public static DependencyInjectionContainer RegisterDefaultSetTypes(this DependencyInjectionContainer container)
        {
            container.MustNotBeNull(nameof(container));

            return container.RegisterTransient(typeof(HashSet<>), options => options.UseDefaultConstructor()
                                                                                    .MapToAbstractions(typeof(ISet<>)))
                            .RegisterTransient(typeof(SortedSet<>), options => options.UseDefaultConstructor());
        }
    }
}