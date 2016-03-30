
namespace Light.Serialization.Json
{
    /// <summary>
    /// Represents the abstraction of an object that is able to create a JSON writer and manage the lifetime of the corresponding resources.
    /// </summary>
    public interface IJsonWriterFactory
    {
        /// <summary>
        /// Creates a new JSON writer instance.
        /// </summary>
        /// <returns></returns>
        IJsonWriter Create();

        /// <summary>
        /// Finishes the writing process and releases all corresponding resources. This method should be called after create.
        /// </summary>
        /// <returns>The JSON document as a string.</returns>
        string FinishWriteProcessAndReleaseResources();
    }
}