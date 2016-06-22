using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Light.GuardClauses;
using Light.Serialization.Abstractions;
using Light.Serialization.Json.BuilderHelpers;
using Light.Serialization.Json.Caching;
using Light.Serialization.Json.ComplexTypeConstruction;
using Light.Serialization.Json.ComplexTypeDecomposition;
using Light.Serialization.Json.LowLevelReading;
using Light.Serialization.Json.LowLevelWriting;
using Light.Serialization.Json.ObjectMetadata;
using Light.Serialization.Json.PrimitiveTypeFormatters;
using Light.Serialization.Json.TokenParsers;
using Light.Serialization.Json.WriterInstructors;
using Microsoft.Practices.Unity;

namespace Light.Serialization.Json.Unity
{
    /// <summary>
    ///     Provides extension methods to register the default serialization and deserilization types with the Unity DI container.
    /// </summary>
    public static class LightSerializationModule
    {
        /// <summary>
        ///     Registers the default serialization types of Light.Serialization.Json with the Unity container.
        /// </summary>
        /// <param name="container">The container that will be populated.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        public static IUnityContainer RegisterDefaultSerializationTypes(this IUnityContainer container)
        {
            container.MustNotBeNull(nameof(container));

            return container

                // JSON Serializer
                .RegisterType<ISerializer, JsonSerializer>()

                // Writer instructor cache for JSON Serializer
                .RegisterType<IDictionary<Type, IJsonWriterInstructor>>(new ContainerControlledLifetimeManager(),
                                                                        new InjectionFactory(c => new Dictionary<Type, IJsonWriterInstructor>()))

                // JSON Writer Factory
                .RegisterType<IJsonWriterFactory, JsonWriterFactory>()
                .RegisterType<IJsonWhitespaceFormatter, WhitespaceFormatterNullObject>()
                .RegisterType<IJsonKeyNormalizer, FirstCharacterToLowerAndRemoveAllSpecialCharactersNormalizer>(new ContainerControlledLifetimeManager())

                // JSON Writer Instructors
                .RegisterType<IReadOnlyList<IJsonWriterInstructor>, IJsonWriterInstructor[]>()
                .RegisterTypeWithTypeName<IJsonWriterInstructor, PrimitiveValueInstructor>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonWriterInstructor, EnumInstructor>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonWriterInstructor, TypeAndTypeInfoInstructor>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonWriterInstructor, DictionaryInstructor>(new InjectionFactory(c => new DictionaryInstructor(c.Resolve<IDictionary<Type, IPrimitiveTypeFormatter>>(),
                                                                                                                                          c.Resolve<IMetadataInstructor>(KnownNames.ObjectMetadataInstructor))),
                                                                                       new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonWriterInstructor, CollectionInstructor>(new InjectionFactory(c => new CollectionInstructor(c.Resolve<IMetadataInstructor>(KnownNames.CollectionMetadataInstructor))),
                                                                                       new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonWriterInstructor, ComplexObjectInstructor>(new InjectionFactory(c => new ComplexObjectInstructor(c.Resolve<IReadableValuesTypeAnalyzer>(),
                                                                                                                                                c.Resolve<IMetadataInstructor>(KnownNames.ObjectMetadataInstructor))),
                                                                                          new ContainerControlledLifetimeManager())

                // Primitive Type Formatters
                .RegisterType<IDictionary<Type, IPrimitiveTypeFormatter>>(new ContainerControlledLifetimeManager(),
                                                                          new InjectionFactory(c => c.ResolveAll<IPrimitiveTypeFormatter>().ToDictionary(f => f.TargetType)))
                .RegisterType<IPrimitiveTypeFormatter>(KnownNames.IntFormatter,
                                                       new ContainerControlledLifetimeManager(),
                                                       new InjectionFactory(c => new ToStringFormatter<int>(false)))
                .RegisterTypeWithTypeName<IPrimitiveTypeFormatter, StringFormatter>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IPrimitiveTypeFormatter, DoubleFormatter>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IPrimitiveTypeFormatter, DateTimeFormatter>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IPrimitiveTypeFormatter, DateTimeOffsetFormatter>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IPrimitiveTypeFormatter, TimeSpanFormatter>(new ContainerControlledLifetimeManager())
                .RegisterType<IPrimitiveTypeFormatter>(KnownNames.GuidFormatter,
                                                       new ContainerControlledLifetimeManager(),
                                                       new InjectionFactory(c => new ToStringWithQuotationMarksFormatter<Guid>(false)))
                .RegisterTypeWithTypeName<IPrimitiveTypeFormatter, BooleanFormatter>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IPrimitiveTypeFormatter, DecimalFormatter>(new ContainerControlledLifetimeManager())
                .RegisterType<IPrimitiveTypeFormatter>(KnownNames.LongFormatter,
                                                       new ContainerControlledLifetimeManager(),
                                                       new InjectionFactory(c => new ToStringFormatter<long>(false)))
                .RegisterTypeWithTypeName<IPrimitiveTypeFormatter, FloatFormatter>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IPrimitiveTypeFormatter, CharFormatter>(new ContainerControlledLifetimeManager())
                .RegisterType<IPrimitiveTypeFormatter>(KnownNames.ShortFormatter,
                                                       new ContainerControlledLifetimeManager(),
                                                       new InjectionFactory(c => new ToStringFormatter<short>(false)))
                .RegisterType<IPrimitiveTypeFormatter>(KnownNames.ByteFormatter,
                                                       new ContainerControlledLifetimeManager(),
                                                       new InjectionFactory(c => new ToStringFormatter<byte>(false)))
                .RegisterType<IPrimitiveTypeFormatter>(KnownNames.UIntFormatter,
                                                       new ContainerControlledLifetimeManager(),
                                                       new InjectionFactory(c => new ToStringFormatter<uint>(false)))
                .RegisterType<IPrimitiveTypeFormatter>(KnownNames.ULongFormatter,
                                                       new ContainerControlledLifetimeManager(),
                                                       new InjectionFactory(c => new ToStringFormatter<ulong>(false)))
                .RegisterType<IPrimitiveTypeFormatter>(KnownNames.UShortFormatter,
                                                       new ContainerControlledLifetimeManager(),
                                                       new InjectionFactory(c => new ToStringFormatter<ushort>(false)))
                .RegisterType<IPrimitiveTypeFormatter>(KnownNames.SByteFormatter,
                                                       new ContainerControlledLifetimeManager(),
                                                       new InjectionFactory(c => new ToStringFormatter<sbyte>(false)))

                // Character Escaper for Char and String Formatters
                .RegisterType<ICharacterEscaper, DefaultCharacterEscaper>(new ContainerControlledLifetimeManager(),
                                                                          new InjectionFactory(c => new DefaultCharacterEscaper()))

                // Type analyzer for complex .NET types
                .RegisterType<IReadableValuesTypeAnalyzer>(new ContainerControlledLifetimeManager(),
                                                           new InjectionFactory(c => new ValueReadersCacheDecorator(new PublicPropertiesAndFieldsAnalyzer(),
                                                                                                                    new Dictionary<Type, List<IValueReader>>())))
                // Metadata instructors for complex .NET types and collections
                .RegisterType<IMetadataInstructor, ObjectMetadataInstructor>(KnownNames.ObjectMetadataInstructor, new ContainerControlledLifetimeManager())
                .RegisterType<ITypeMetadataInstructor>(new InjectionFactory(c => c.Resolve<ObjectMetadataInstructor>(KnownNames.ObjectMetadataInstructor)))
                .RegisterType<IMetadataInstructor, ArrayMetadataInstructor>(KnownNames.CollectionMetadataInstructor, new ContainerControlledLifetimeManager())

                // Type to name mapping
                .RegisterType<ITypeToNameMapping, SimpleNameToTypeMapping>(new ContainerControlledLifetimeManager())
                .RegisterType<INameToTypeMapping, SimpleNameToTypeMapping>(new ContainerControlledLifetimeManager());
        }

        public static IUnityContainer RegisterDefaultDeserializationTypes(this IUnityContainer container)
        {
            container.MustNotBeNull(nameof(container));

            return container
                // JSON Deserializer
                .RegisterType<IDeserializer, JsonDeserializer>()

                // JSON Reader Factory
                .RegisterType<IJsonReaderFactory, JsonReaderFactory>(new ContainerControlledLifetimeManager())

                // Token Parser Cache
                .RegisterType<Dictionary<JsonTokenTypeCombination, IJsonTokenParser>>(new ContainerControlledLifetimeManager(),
                                                                                      new InjectionFactory(c => new Dictionary<JsonTokenTypeCombination, IJsonTokenParser>()))

                // Token Parsers
                .RegisterType<IReadOnlyList<IJsonTokenParser>, IJsonTokenParser[]>()
                .RegisterType<IReadOnlyList<IJsonStringToPrimitiveParser>, IJsonStringToPrimitiveParser[]>()
                .RegisterTypeWithTypeName<IJsonTokenParser, SignedIntegerParser>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonStringToPrimitiveParser, SignedIntegerParser>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonTokenParser, DoubleParser>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonStringToPrimitiveParser, DoubleParser>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonTokenParser, BooleanParser>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonStringToPrimitiveParser, BooleanParser>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonTokenParser, CharacterParser>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonStringToPrimitiveParser, CharacterParser>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonTokenParser, DateTimeParser>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonStringToPrimitiveParser, DateTimeParser>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonTokenParser, DateTimeOffsetParser>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonStringToPrimitiveParser, DateTimeOffsetParser>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonTokenParser, TimeSpanParser>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonStringToPrimitiveParser, TimeSpanParser>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonTokenParser, UnsignedIntegerParser>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonTokenParser, FloatParser>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonTokenParser, DecimalParser>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonTokenParser, NullParser>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonTokenParser, EnumParser>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonTokenParser, StringParser>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonTokenParser, GuidParser>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonStringToPrimitiveParser, GuidParser>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonTokenParser, NullableParser>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonTokenParser, JsonStringInheritanceParser>()
                .RegisterTypeWithTypeName<IJsonTokenParser, TypeAndTypeInfoParser>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonTokenParser, CollectionParser>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonTokenParser, DictionaryParser>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonTokenParser, ComplexObjectParser>(new ContainerControlledLifetimeManager())

                // Meta Factory
                .RegisterType<IMetaFactory, UnityMetaFactoryAdapter>(new ContainerControlledLifetimeManager())

                // Metadata parsers
                .RegisterType<IObjectMetadataParser, ObjectMetadataParser>(new ContainerControlledLifetimeManager())
                .RegisterType<ITypeParser>(new InjectionFactory(c => c.Resolve<ObjectMetadataParser>()))
                .RegisterType<IArrayMetadataParser, ArrayMetadataParser>(new ContainerControlledLifetimeManager())
                .RegisterType<INameToTypeMapping, SimpleNameToTypeMapping>(new ContainerControlledLifetimeManager())

                // Type description service
                .RegisterType<ITypeDescriptionService, DefaultTypeDescriptionServiceWithCaching>(new ContainerControlledLifetimeManager())
                .RegisterType<Dictionary<Type, TypeCreationDescription>>(new ContainerControlledLifetimeManager(),
                                                                         new InjectionFactory(c => new Dictionary<Type, TypeCreationDescription>()));
        }

        /// <summary>
        ///     Registers an indenting whitespace formatter with the specified container so that the JSON serializer produces human-readable documents with newlines and indenting.
        /// </summary>
        /// <param name="container">The container to be populated.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        public static IUnityContainer UserIndentingWhitespaceFormatterForSerialization(this IUnityContainer container)
        {
            container.MustNotBeNull(nameof(container));

            return container.RegisterType<IJsonWhitespaceFormatter, IndentingWhitespaceFormatter>();
        }

        /// <summary>
        ///     Configures the metadata instructors and parsers to use Domain Friendly Names which can be configured by the specified delegate.
        /// </summary>
        /// <param name="container">The container to be populated.</param>
        /// <param name="configureMapping">The delegate that configures the Domain Friendly Name mapping.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> or <paramref name="configureMapping" /> is null.</exception>
        public static IUnityContainer UseDomainFriendlyNames(this IUnityContainer container, Action<TypeNameToJsonNameScanner.IScanningOptions> configureMapping)
        {
            container.MustNotBeNull(nameof(container));
            configureMapping.MustNotBeNull(nameof(configureMapping));

            var domainFriendlyNameMapping = DomainFriendlyNameMapping.CreateWithDefaultTypeMappings()
                                                                     .ScanTypes(configureMapping);

            return container.UseDomainFriendlyNames(domainFriendlyNameMapping);
        }

        /// <summary>
        ///     Configures the metadata instructors for serialization to use the specified Domain Friendly Name mapping.
        /// </summary>
        /// <param name="container">The container to be populated.</param>
        /// <param name="mapping">The mapping that should be used as ITypeToNameMapping and INameToTypeMapping.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> or <paramref name="mapping" /> is null.</exception>
        public static IUnityContainer UseDomainFriendlyNames(this IUnityContainer container, DomainFriendlyNameMapping mapping)
        {
            container.MustNotBeNull(nameof(container));
            mapping.MustNotBeNull(nameof(mapping));

            foreach (var metadataInstructor in container.ResolveAll<IMetadataInstructor>()
                                                        .OfType<ISetTypeToNameMapping>())
            {
                metadataInstructor.TypeToNameMapping = mapping;
            }

            return container.RegisterInstance<ITypeToNameMapping>(mapping)
                            .RegisterInstance<INameToTypeMapping>(mapping);
        }

        /// <summary>
        ///     Configures all metadata instructors for serialization to not serialize object IDs in the JSON document.
        /// </summary>
        /// <param name="container">The container to be configured.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        public static IUnityContainer DisableObjectReferencePreservation(this IUnityContainer container)
        {
            return container.SetObjectReferencePreservation(false);
        }

        /// <summary>
        ///     Configures all metadata instructors for serialization to serialize object IDs in the JSON document and reference already serialized objects.
        ///     By default, this option is turned on, so you do not have to call this method if you have not turned off Object Reference Preservation before.
        /// </summary>
        /// <param name="container">The container to be configured.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        public static IUnityContainer EnableObjectReferencePreservation(this IUnityContainer container)
        {
            return container.SetObjectReferencePreservation(true);
        }

        private static IUnityContainer SetObjectReferencePreservation(this IUnityContainer container, bool value)
        {
            container.MustNotBeNull(nameof(container));

            foreach (var metadataInstructor in container.ResolveAll<IMetadataInstructor>()
                                                        .OfType<ISetObjectReferencePreservationStatus>())
            {
                metadataInstructor.IsSerializingObjectIds = value;
            }

            return container;
        }

        /// <summary>
        ///     Configures all metadata instructors for serialization to not serialize type metadata in the JSON document.
        /// </summary>
        /// <param name="container">The container to be configured.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        public static IUnityContainer DisableTypeMetadata(this IUnityContainer container)
        {
            return container.SetTypeMetadataStatus(false);
        }

        /// <summary>
        ///     Configures all metadata instructors for serialization to serialize type metadata in the JSON document.
        ///     By default, this option is turned on, so you do not have to call this method if you have not turned off Type Metadata before.
        /// </summary>
        /// <param name="container">The container to be configured.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        public static IUnityContainer EnableTypeMetadata(this IUnityContainer container)
        {
            return container.SetTypeMetadataStatus(true);
        }

        private static IUnityContainer SetTypeMetadataStatus(this IUnityContainer container, bool value)
        {
            container.MustNotBeNull(nameof(container));

            foreach (var metadataInstructor in container.ResolveAll<IMetadataInstructor>()
                                                        .OfType<ISetTypeInfoSerializationStatus>())
            {
                metadataInstructor.IsSerializingTypeInfo = value;
            }

            return container;
        }

        /// <summary>
        ///     Registers the specified type with the container using the concrete type name as the name for the registration.
        /// </summary>
        /// <typeparam name="TFrom">The type of the abstraction.</typeparam>
        /// <typeparam name="TTo">The concrete type.</typeparam>
        /// <param name="container">The container to be populated.</param>
        /// <param name="lifetimeManager">The lifetime manager used for the registration (optional). By default, a <see cref="TransientLifetimeManager" /> is used.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        public static IUnityContainer RegisterTypeWithTypeName<TFrom, TTo>(this IUnityContainer container,
                                                                           LifetimeManager lifetimeManager = null) where TTo : TFrom
        {
            container.MustNotBeNull(nameof(container));

            lifetimeManager = lifetimeManager ?? new TransientLifetimeManager();
            return container.RegisterType<TFrom, TTo>(typeof(TTo).Name, lifetimeManager);
        }


        /// <summary>
        ///     Registers the specified type with the container using the concrete type name as the name for the registration.
        /// </summary>
        /// <typeparam name="TFrom">The type of the abstraction.</typeparam>
        /// <typeparam name="TTo">The concrete type.</typeparam>
        /// <param name="container">The container to be populated.</param>
        /// <param name="injectionFactory">The factory that creates the specified object.</param>
        /// <param name="lifetimeManager">The lifetime manager used for the registration (optional). By default, a <see cref="TransientLifetimeManager" /> is used.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> or <paramref name="injectionFactory" /> is null.</exception>
        public static IUnityContainer RegisterTypeWithTypeName<TFrom, TTo>(this IUnityContainer container,
                                                                           InjectionFactory injectionFactory,
                                                                           LifetimeManager lifetimeManager = null) where TTo : TFrom
        {
            container.MustNotBeNull(nameof(container));
            injectionFactory.MustNotBeNull(nameof(injectionFactory));

            lifetimeManager = lifetimeManager ?? new TransientLifetimeManager();
            return container.RegisterType<TFrom, TTo>(typeof(TTo).Name, lifetimeManager, injectionFactory);
        }

        /// <summary>
        ///     Registers <see cref="Dictionary{TKey,TValue}" /> and <see cref="SortedDictionary{TKey,TValue}" /> with the container using their default constructors.
        ///     Also maps <see cref="IDictionary{TKey,TValue}" /> to <see cref="Dictionary{TKey,TValue}" /> by default.
        /// </summary>
        /// <param name="container">The container to be populated.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        public static IUnityContainer RegisterDefaultDictionaryTypes(this IUnityContainer container)
        {
            container.MustNotBeNull(nameof(container));

            return container.RegisterType(typeof(Dictionary<,>), new InjectionConstructor())
                            .RegisterType(typeof(SortedDictionary<,>), new InjectionConstructor())
                            .RegisterType(typeof(IDictionary<,>), typeof(Dictionary<,>));
        }

        /// <summary>
        ///     Registers <see cref="List{T}" />, <see cref="ObservableCollection{T}" />, and <see cref="Collection{T}" /> with the container using their default constructors.
        ///     Also maps <see cref="IList{T}" />, <see cref="ICollection{T}" />, and <see cref="IEnumerable{T}" /> to <see cref="List{T}" /> by default.
        /// </summary>
        /// <param name="container">The container to be populated.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        public static IUnityContainer RegisterDefaultCollectionTypes(this IUnityContainer container)
        {
            container.MustNotBeNull(nameof(container));

            return container.RegisterType(typeof(List<>), new InjectionConstructor())
                            .RegisterType(typeof(ObservableCollection<>), new InjectionConstructor())
                            .RegisterType(typeof(Collection<>), new InjectionConstructor())
                            .RegisterType(typeof(IList<>), typeof(List<>))
                            .RegisterType(typeof(ICollection<>), typeof(List<>))
                            .RegisterType(typeof(IEnumerable<>), typeof(List<>));
        }

        /// <summary>
        ///     Registers <see cref="HashSet{T}" /> and <see cref="SortedSet{T}" /> with the container using their default constructors.
        ///     Also maps <see cref="ISet{T}" /> to <see cref="HashSet{T}" /> by default.
        /// </summary>
        /// <param name="container">The container to be populated.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        public static IUnityContainer RegisterDefaultSetTypes(this IUnityContainer container)
        {
            container.MustNotBeNull(nameof(container));

            return container.RegisterType(typeof(HashSet<>), new InjectionConstructor())
                            .RegisterType(typeof(SortedSet<>), new InjectionConstructor())
                            .RegisterType(typeof(ISet<>), typeof(HashSet<>));
        }
    }
}