using System;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents the information that is necessary to create a dictionary, a complex .NET object, or a collection
    ///     from the metadata section of a complex JSON object or JSON array.
    /// </summary>
    public struct MetadataParseResult
    {
        /// <summary>
        ///     Gets the type that should be constructed.
        /// </summary>
        public readonly Type TypeToConstruct;

        /// <summary>
        ///     Gets the information about Object-Reference-Preservation.
        /// </summary>
        public readonly ReferencePreservationInfo ReferencePreservationInfo;

        private MetadataParseResult(Type typeToConstruct, ReferencePreservationInfo referencePreservationInfo)
        {
            TypeToConstruct = typeToConstruct;
            ReferencePreservationInfo = referencePreservationInfo;
        }

        /// <summary>
        ///     Creates a new instance of MetadataParseResult with an object that already has been deserialized.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="refId" /> is less than zero.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="deserializedObject" /> is null.</exception>
        public static MetadataParseResult FromRetrievedObject(int refId, object deserializedObject)
        {
            return new MetadataParseResult(null, ReferencePreservationInfo.FromAlreadyDeserializedObject(refId, deserializedObject));
        }

        /// <summary>
        ///     Creates a new instance of MetadataParseResult with a deferred reference.
        /// </summary>
        /// <param name="refId">The reference id pointing to another object that has not been fully deserialized yet.</param>
        public static MetadataParseResult FromDeferredReference(int refId)
        {
            return new MetadataParseResult(null, ReferencePreservationInfo.FromDeferredReference(refId));
        }
    }
}