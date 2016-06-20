namespace Light.Serialization.Json.BuilderHelpers
{
    /// <summary>
    ///     Represents the abstraction of retrieving a singleton instance which already is in memory from a factory.
    /// </summary>
    /// <typeparam name="T">The type of the singleton object.</typeparam>
    public interface IGetSingletonInstance<out T>
    {
        /// <summary>
        ///     Gets the singleton instance.
        /// </summary>
        T Instance { get; }
    }
}