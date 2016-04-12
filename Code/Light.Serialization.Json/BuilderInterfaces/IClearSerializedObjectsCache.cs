namespace Light.Serialization.Json.BuilderInterfaces
{
    /// <summary>
    ///     Represents the abstraction used to clear the object cache of a metadata instructor.
    /// </summary>
    public interface IClearSerializedObjectsCache
    {
        /// <summary>
        ///     Clears the list of serialized objects - this should be done when a new object graph is serialized.
        /// </summary>
        void ClearSerializedObjects();
    }
}