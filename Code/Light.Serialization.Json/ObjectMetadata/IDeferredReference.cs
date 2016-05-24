namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents the abstraction of a deferred reference that can be set on target objects once the referenced object is fully deserialized.
    /// </summary>
    public interface IDeferredReference
    {
        /// <summary>
        ///     Gets the JSON document ID of the object that should be set later once it is fully deserialized.
        /// </summary>
        int ReferenceId { get; }

        /// <summary>
        ///     Sets the now fully deserialized object for this deferred reference.
        /// </summary>
        /// <param name="reference">The object that was deferred.</param>
        void SetDeferredReference(object reference);
    }
}