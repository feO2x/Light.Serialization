using System;
using Light.GuardClauses;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents a structure encompassing all information possible for Object Reference Preservation
    /// </summary>
    public struct ReferencePreservationInfo
    {
        /// <summary>
        ///     Gets the ID in the JSON document for the object to be deserialized.
        /// </summary>
        public readonly int Id;

        /// <summary>
        ///     Gets the reference to the object that was deserialized before.
        /// </summary>
        public readonly object RetrievedObject;

        /// <summary>
        ///     Gets the value indicating whether the object is a reference to another object that has not been deserialized yet.
        ///     If this value is true, then setting the object should be deferred.
        /// </summary>
        public readonly bool IsDeferredReference;

        /// <summary>
        ///     Gets the value indicating whether an object has been retrieved.
        /// </summary>
        public bool WasObjectRetrieved => RetrievedObject != null;

        /// <summary>
        ///     Gets the value indicating that no Object Reference Preservation metadata is available.
        /// </summary>
        public bool IsNotContainingObjectReferencePreservationInfos => Id == -1;

        private ReferencePreservationInfo(int id, object retrievedObject, bool isDeferredReference)
        {
            Id = id;
            RetrievedObject = retrievedObject;
            IsDeferredReference = isDeferredReference;
        }

        /// <summary>
        ///     Creates a new instance of ReferencePreservationInfo from a reference to another object that already has been deserialized.
        ///     Usually, you want to use this info in Metadata Parsers when you hit a $ref metadata symbol.
        /// </summary>
        /// <param name="refId">The reference id to the other object.</param>
        /// <param name="deserializedObject">The reference to the already deseriailzed object.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="refId" /> is less than zero.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="deserializedObject" /> is null.</exception>
        public static ReferencePreservationInfo FromAlreadyDeserializedObject(int refId, object deserializedObject)
        {
            refId.MustNotBeLessThan(0, message: "An object ID in the JSON document cannot be less than zero.");
            deserializedObject.MustNotBeNull(nameof(deserializedObject));

            return new ReferencePreservationInfo(refId, deserializedObject, false);
        }

        /// <summary>
        ///     Creates a new instance of ReferencePreservationInfo from a reference to another object that has not been fully deserialized yet.
        ///     Usually, you want to use this info in a Metadata Parser when a complex JSON object has a reference to its parent complex JSON object.
        /// </summary>
        /// <param name="refId">The reference id to the other object that is not fully deserialized yet.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="refId" /> is less than zero.</exception>
        public static ReferencePreservationInfo FromDeferredReference(int refId)
        {
            refId.MustNotBeLessThan(0, message: "An object ID in the JSON document cannot be less than zero.");

            return new ReferencePreservationInfo(refId, null, true);
        }

        /// <summary>
        ///     Creates a new instance of ReferencePreservationInfo for an object that has not been deserialized before.
        /// </summary>
        /// <param name="id">The id of the object.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="id" /> is less than zero.</exception>
        public static ReferencePreservationInfo FromNewObjectInJsonDocument(int id)
        {
            id.MustNotBeLessThan(0, message: "An object ID in the JSON document cannot be less than zero.");

            return new ReferencePreservationInfo(id, null, false);
        }

        /// <summary>
        ///     Creates a new instance of ReferencePreservationInfo for a JSON object that contains no Object Reference Preservation metadata.
        /// </summary>
        public static ReferencePreservationInfo Empty => new ReferencePreservationInfo(-1, null, false);
    }
}