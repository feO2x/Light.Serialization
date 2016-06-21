using System;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents the information that is necessary to create .NET collection instances
    ///     from the metadata section of a JSON array.
    /// </summary>
    public struct ArrayMetadataParseResult
    {
        /// <summary>
        ///     Gets the type that should be constructed.
        /// </summary>
        public readonly Type CollectionTypeToConstruct;

        /// <summary>
        ///     Gets the information about Object-Reference-Preservation.
        /// </summary>
        public readonly ReferencePreservationInfo ReferencePreservationInfo;

        /// <summary>
        ///     Gets the lenths of the array.
        /// </summary>
        public readonly int[] ArrayLenghts;

        /// <summary>
        ///     Gets the value indicating whether this parse result describes a .NET array type.
        /// </summary>
        public bool IsDescribingArrayType => ArrayLenghts != null || CollectionTypeToConstruct.IsArray;

        private ArrayMetadataParseResult(Type collectionTypeToConstruct, ReferencePreservationInfo referencePreservationInfo, int[] arrayLenghts)
        {
            CollectionTypeToConstruct = collectionTypeToConstruct;
            ReferencePreservationInfo = referencePreservationInfo;
            ArrayLenghts = arrayLenghts;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="ArrayMetadataParseResult" /> with an object that already has been deserialized.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="refId" /> is less than zero.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="deserializedObject" /> is null.</exception>
        public static ArrayMetadataParseResult FromRetrievedObject(int refId, object deserializedObject)
        {
            return new ArrayMetadataParseResult(null, ReferencePreservationInfo.FromAlreadyDeserializedObject(refId, deserializedObject), null);
        }

        /// <summary>
        ///     Creates a new instance of <see cref="ArrayMetadataParseResult" /> with a deferred reference.
        /// </summary>
        /// <param name="refId">The reference id pointing to another object that has not been fully deserialized yet.</param>
        public static ArrayMetadataParseResult FromDeferredReference(int refId)
        {
            return new ArrayMetadataParseResult(null, ReferencePreservationInfo.FromDeferredReference(refId), null);
        }

        /// <summary>
        ///     Creates a new instance of <see cref="ArrayMetadataParseResult" /> from deserialized JSON metadata information describing a .NET collection.
        /// </summary>
        /// <param name="typeToConstruct">The collection type that should be instantiated and populated.</param>
        /// <param name="referencePreservationInfo">Information about Object Reference Preservation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeToConstruct" /> is null.</exception>
        public static ArrayMetadataParseResult FromCollectionType(Type typeToConstruct, ReferencePreservationInfo referencePreservationInfo)
        {
            typeToConstruct.MustNotBeNull(nameof(typeToConstruct));

            return new ArrayMetadataParseResult(typeToConstruct, referencePreservationInfo, null);
        }

        /// <summary>
        ///     Creates a new instance of <see cref="ArrayMetadataParseResult" /> from deserialized JSON metadata information describing a .NET array.
        /// </summary>
        /// <param name="typeToConstruct">The array type that should be instantiated and populated.</param>
        /// <param name="referencePreservationInfo">Information about Object Reference Preservation.</param>
        /// <param name="arrayLengths">The dimensions of the array.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeToConstruct" /> or <paramref name="arrayLengths" /> is null.</exception>
        /// <exception cref="EmptyCollectionException">Thrown when <paramref name="arrayLengths" /> is an empty array.</exception>
        public static ArrayMetadataParseResult FromArrayType(Type typeToConstruct, ReferencePreservationInfo referencePreservationInfo, int[] arrayLengths)
        {
            typeToConstruct.MustNotBeNull(nameof(typeToConstruct));
            arrayLengths.MustNotBeNullOrEmpty(nameof(arrayLengths));

            return new ArrayMetadataParseResult(typeToConstruct, referencePreservationInfo, arrayLengths);
        }
    }
}