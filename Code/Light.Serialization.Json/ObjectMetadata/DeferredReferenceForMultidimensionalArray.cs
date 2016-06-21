using System;
using Light.GuardClauses;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents an <see cref="IDeferredReference"/> that inserts a value in a .NET multi-dimensional array at the specified indices.
    /// </summary>
    public sealed class DeferredReferenceForMultidimensionalArray : IDeferredReference
    {
        private readonly Array _array;
        private readonly int[] _targetIndices;

        /// <summary>
        ///     Creates a new instance of <see cref="DeferredReferenceForMultidimensionalArray" />.
        /// </summary>
        /// <param name="referenceId">The id of the deferred reference.</param>
        /// <param name="targetIndices">The indices that describe the target position in the array.</param>
        /// <param name="array">The array where the deferred reference will be set on.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="referenceId" /> is less than zero or
        ///     when <paramref name="targetIndices" /> does not have same number of items as there are dimensions in the target array.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="targetIndices" /> or <paramref name="array" /> is null.</exception>
        public DeferredReferenceForMultidimensionalArray(int referenceId, int[] targetIndices, Array array)
        {
            referenceId.MustNotBeLessThan(0, nameof(referenceId));
            array.MustNotBeNull(nameof(array));
            targetIndices.MustHaveCount(array.Rank, nameof(targetIndices), "targetIndices must have the same number of items as there are dimensions in the target array.");

            ReferenceId = referenceId;
            _array = array;
            _targetIndices = new int[targetIndices.Length];
            targetIndices.CopyTo(_targetIndices, 0);
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
            _array.SetValue(reference, _targetIndices);
        }
    }
}