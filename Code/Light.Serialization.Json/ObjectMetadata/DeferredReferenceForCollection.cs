using System;
using System.Collections;
using Light.GuardClauses;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents an <see cref="IDeferredReference"/> that inserts a value in a .NET collection at a specific index.
    /// </summary>
    public sealed class DeferredReferenceForCollection : IDeferredReference
    {
        private readonly IList _collection;
        private readonly int _targetIndex;

        /// <summary>
        ///     Creates a new instance of DeferredReferenceForCollection.
        /// </summary>
        /// <param name="referenceId">The id of the deferred reference.</param>
        /// <param name="targetIndex">The index where the deferred reference will be set.</param>
        /// <param name="collection">The collection where the deferred object will be inserted to.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="referenceId" /> or <paramref name="targetIndex" /> is less than zero.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="collection" /> is null.</exception>
        public DeferredReferenceForCollection(int referenceId, int targetIndex, IList collection)
        {
            referenceId.MustNotBeLessThan(0, nameof(referenceId));
            targetIndex.MustNotBeLessThan(0, nameof(targetIndex));
            collection.MustNotBeNull(nameof(collection));

            _targetIndex = targetIndex;
            _collection = collection;
            ReferenceId = referenceId;
        }

        /// <summary>
        ///     Gets the ID of the object that is not fully deserialized yet.
        /// </summary>
        public int ReferenceId { get; }

        /// <summary>
        ///     Inserts the deferred reference on the target collection.
        /// </summary>
        /// <param name="reference">The object that should be inserted to the target collection.</param>
        public void SetDeferredReference(object reference)
        {
            _collection.Insert(_targetIndex, reference);
        }
    }
}