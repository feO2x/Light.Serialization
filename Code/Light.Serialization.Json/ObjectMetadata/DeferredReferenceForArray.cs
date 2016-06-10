using System;
using Light.GuardClauses;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents a deferred reference that inserts a value in a .NET one-dimensional array at a specific index.
    /// </summary>
    public sealed class DeferredReferenceForArray : IDeferredReference
    {
        private readonly Array _array;
        private readonly int _targetIndex;

        /// <summary>
        ///     Creates a new instance of <see cref="DeferredReferenceForArray" />.
        /// </summary>
        /// <param name="referenceId">The id of the deferred reference.</param>
        /// <param name="targetIndex">The index where the deferred reference will be set in the array.</param>
        /// <param name="array">The array where the deferred object will be set on.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="referenceId" /> or <paramref name="targetIndex" /> is less than zero or
        ///     <paramref name="targetIndex" /> is greater than or equal to the length of the array.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="array" /> is null.</exception>
        public DeferredReferenceForArray(int referenceId, int targetIndex, Array array)
        {
            referenceId.MustNotBeLessThan(0, nameof(referenceId));
            targetIndex.MustNotBeLessThan(0, nameof(targetIndex));
            array.MustNotBeNull(nameof(array));
            targetIndex.MustNotBeGreaterThanOrEqualTo(array.Length, nameof(targetIndex), "The target index must not be greater than the length of the array.");

            _targetIndex = targetIndex;
            _array = array;
            ReferenceId = referenceId;
        }

        /// <summary>
        ///     Gets the ID of the object that is not fully deserialized yet.
        /// </summary>
        public int ReferenceId { get; }

        /// <summary>
        ///     Inserts the deferred reference on the target array.
        /// </summary>
        /// <param name="reference">The object that should be inserted to the target array.</param>
        public void SetDeferredReference(object reference)
        {
            _array.SetValue(reference, _targetIndex);
        }
    }
}