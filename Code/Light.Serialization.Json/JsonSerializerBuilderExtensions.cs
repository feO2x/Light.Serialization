using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.Serialization.Json.ComplexTypeDecomposition;
using Light.Serialization.Json.ObjectMetadata;
using Light.Serialization.Json.PrimitiveTypeFormatters;
using Light.Serialization.Json.WriterInstructors;

namespace Light.Serialization.Json
{
    /// <summary>
    ///     Provides extension methods to populate collections with the default instances of JSON writer instructors and primitive type formatters.
    /// </summary>
    public static class JsonSerializerBuilderExtensions
    {
        /// <summary>
        ///     Populates the specified list with the default primitive type formatters.
        /// </summary>
        /// <param name="targetList">The collection that will be populated.</param>
        /// <param name="characterEscaper">The character escaper that will be injected into the CharFormatter and StringFormatter.</param>
        /// <returns>The list for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public static IList<IPrimitiveTypeFormatter> AddDefaultPrimitiveTypeFormatters(this IList<IPrimitiveTypeFormatter> targetList, ICharacterEscaper characterEscaper)
        {
            targetList.MustNotBeNull(nameof(targetList));

            targetList.Add(new ToStringFormatter<int>(false));
            targetList.Add(new StringFormatter(characterEscaper));
            targetList.Add(new DoubleFormatter());
            targetList.Add(new DateTimeFormatter());
            targetList.Add(new DateTimeOffsetFormatter());
            targetList.Add(new TimeSpanFormatter());
            targetList.Add(new ToStringWithQuotationMarksFormatter<Guid>(false));
            targetList.Add(new BooleanFormatter());
            targetList.Add(new DecimalFormatter());
            targetList.Add(new ToStringFormatter<long>(false));
            targetList.Add(new FloatFormatter());
            targetList.Add(new CharFormatter(characterEscaper));
            targetList.Add(new ToStringFormatter<short>(false));
            targetList.Add(new ToStringFormatter<byte>(false));
            targetList.Add(new ToStringFormatter<uint>(false));
            targetList.Add(new ToStringFormatter<ulong>(false));
            targetList.Add(new ToStringFormatter<ushort>(false));
            targetList.Add(new ToStringFormatter<sbyte>(false));

            return targetList;
        }

        /// <summary>
        ///     Populates the specified list with the default JSON writer instructors.
        /// </summary>
        /// <typeparam name="TCollection">The type of the collection that will be populated.</typeparam>
        /// <param name="targetList">The collection that will be populated.</param>
        /// <param name="primitiveTypeToFormattersMapping">The dictionary containing the mapping from type to primitive type formatter, which will be injected into the PrimitiveValueInstructor and DictionaryInstructor.</param>
        /// <param name="readableValuesTypeAnalyzer">The type analyzer that will be injected into the ComplexObjectInstructor.</param>
        /// <param name="collectionMetadataInstructor">The instructor used to serialize the metadata section of JSON arrays.</param>
        /// <param name="objectMetadataInstructor">The instructor used to serialize the metadata section of complex JSON objects.</param>
        /// <returns>The collection for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public static TCollection AddDefaultWriterInstructors<TCollection>(this TCollection targetList,
                                                                           IDictionary<Type, IPrimitiveTypeFormatter> primitiveTypeToFormattersMapping,
                                                                           IReadableValuesTypeAnalyzer readableValuesTypeAnalyzer,
                                                                           IMetadataInstructor collectionMetadataInstructor,
                                                                           IMetadataInstructor objectMetadataInstructor)
            where TCollection : class, IList<IJsonWriterInstructor>
        {
            targetList.MustNotBeNull(nameof(targetList));

            targetList.Add(new PrimitiveValueInstructor(primitiveTypeToFormattersMapping));
            targetList.Add(new EnumInstructor());
            targetList.Add(new DictionaryInstructor(primitiveTypeToFormattersMapping, objectMetadataInstructor));
            targetList.Add(new CollectionInstructor(collectionMetadataInstructor));
            targetList.Add(new ComplexObjectInstructor(readableValuesTypeAnalyzer, objectMetadataInstructor));

            return targetList;
        }
    }
}