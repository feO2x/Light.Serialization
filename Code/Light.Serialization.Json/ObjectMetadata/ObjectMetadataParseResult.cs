using System;
using Light.GuardClauses;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents the information that is necessary to create a dictionary, a complex .NET object
    ///     from the metadata section of a complex JSON object.
    /// </summary>
    public struct ObjectMetadataParseResult
    {
        /// <summary>
        ///     Gets the type that should be constructed.
        /// </summary>
        public readonly Type TypeToConstruct;

        /// <summary>
        ///     Gets the information about Object-Reference-Preservation.
        /// </summary>
        public readonly ReferencePreservationInfo ReferencePreservationInfo;

        private ObjectMetadataParseResult(Type typeToConstruct, ReferencePreservationInfo referencePreservationInfo)
        {
            TypeToConstruct = typeToConstruct;
            ReferencePreservationInfo = referencePreservationInfo;
        }

        /// <summary>
        ///     Creates a new instance of MetadataParseResult with an object that already has been deserialized.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="refId" /> is less than zero.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="deserializedObject" /> is null.</exception>
        public static ObjectMetadataParseResult FromRetrievedObject(int refId, object deserializedObject)
        {
            return new ObjectMetadataParseResult(null, ReferencePreservationInfo.FromAlreadyDeserializedObject(refId, deserializedObject));
        }

        /// <summary>
        ///     Creates a new instance of MetadataParseResult with a deferred reference.
        /// </summary>
        /// <param name="refId">The reference id pointing to another object that has not been fully deserialized yet.</param>
        public static ObjectMetadataParseResult FromDeferredReference(int refId)
        {
            return new ObjectMetadataParseResult(null, ReferencePreservationInfo.FromDeferredReference(refId));
        }

        /// <summary>
        ///     Creates a new instance of MetadataParseResult from deserialized JSON metadata information.
        /// </summary>
        /// <param name="typeToConstruct">The type that should be instantiated and populated.</param>
        /// <param name="referencePreservationInfo">Information about Object Reference Preservation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeToConstruct" /> is null.</exception>
        public static ObjectMetadataParseResult FromMetadata(Type typeToConstruct, ReferencePreservationInfo referencePreservationInfo)
        {
            typeToConstruct.MustNotBeNull(nameof(typeToConstruct));

            return new ObjectMetadataParseResult(typeToConstruct, referencePreservationInfo);
        }
    }
}