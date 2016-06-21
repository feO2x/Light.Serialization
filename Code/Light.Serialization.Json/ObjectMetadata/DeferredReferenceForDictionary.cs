using System.Collections;
using Light.GuardClauses;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents an <see cref="IDeferredReference"/> that inserts a value in a .NET dictionary with a specified key.
    /// </summary>
    public sealed class DeferredReferenceForDictionary : IDeferredReference
    {
        private readonly IDictionary _dictionary;
        private readonly object _key;

        public DeferredReferenceForDictionary(int referenceId, object key, IDictionary dictionary)
        {
            referenceId.MustNotBeLessThan(0, nameof(referenceId));
            dictionary.MustNotBeNull(nameof(dictionary));

            ReferenceId = referenceId;
            _key = key;
            _dictionary = dictionary;
        }

        /// <summary>
        ///     Gets the ID of the object that is not fully deserialized yet.
        /// </summary>
        public int ReferenceId { get; }

        /// <summary>
        ///     Inserts the deferred reference on the target dictionary.
        /// </summary>
        /// <param name="reference">The object that should be inserted to the target dictionary.</param>
        public void SetDeferredReference(object reference)
        {
            _dictionary.Add(_key, reference);
        }
    }
}