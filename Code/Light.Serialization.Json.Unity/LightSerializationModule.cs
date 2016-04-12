using System;
using System.Collections.Generic;
using System.Linq;
using Light.GuardClauses;
using Light.Serialization.Abstractions;
using Light.Serialization.Json.Caching;
using Light.Serialization.Json.ComplexTypeDecomposition;
using Light.Serialization.Json.LowLevelWriting;
using Light.Serialization.Json.ObjectMetadata;
using Light.Serialization.Json.PrimitiveTypeFormatters;
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
                .RegisterTypeWithTypeName<IJsonWriterInstructor, DictionaryInstructor>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonWriterInstructor, CollectionInstructor>(new ContainerControlledLifetimeManager())
                .RegisterTypeWithTypeName<IJsonWriterInstructor, ComplexObjectInstructor>(new ContainerControlledLifetimeManager())

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
                                                           new InjectionFactory(c => new ValueProvidersCacheDecorator(new PublicPropertiesAndFieldsAnalyzer(),
                                                                                                                      new Dictionary<Type, IList<IValueProvider>>())))
                // Metadata instructor for complex .NET types
                .RegisterType<IMetadataInstructor, TypeAndReferenceMetadataInstructor>(new PerResolveLifetimeManager())

                // Type to name mapping
                .RegisterType<ITypeToNameMapping, SimpleNameToTypeMapping>(new ContainerControlledLifetimeManager());
        }

        /// <summary>
        ///     Registers an indenting whitespace formatter with the specified container so that the JSON serializer produces human-readable documents with newlines and indenting.
        /// </summary>
        /// <param name="container">The container to be populated.</param>
        /// <returns>The container for method-chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        public static IUnityContainer UserIndentingWhitespaceFormatterForSerialization(this IUnityContainer container)
        {
            container.MustNotBeNull(nameof(container));

            return container.RegisterType<IJsonWhitespaceFormatter, IndentingWhitespaceFormatter>();
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
            return container.RegisterType<TFrom, TTo>(typeof (TTo).Name, lifetimeManager);
        }
    }
}