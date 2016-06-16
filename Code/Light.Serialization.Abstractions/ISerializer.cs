using System.IO;

namespace Light.Serialization.Abstractions
{
    /// <summary>
    ///     Represents an object that can be used to serialize object graphs to strings.
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        ///     Serializes the specified object graph to a string.
        /// </summary>
        /// <param name="objectGraphRoot">The object that is the starting point of the object graph.</param>
        /// <returns>The serialized object graph as a string.</returns>
        /// <exception cref="SerializationException">Thrown when any part of the object graph could not be serialized.</exception>
        string Serialize(object objectGraphRoot);

        /// <summary>
        ///     Serializes the specified object graph to the stream.
        /// </summary>
        /// <param name="objectGraphRoot">The object that is the starting point of the object graph.</param>
        /// <param name="textWriter">The text writer encapsulating the stream that the serialized document is written to.</param>
        /// <exception cref="SerializationException">Thrown when any part of the object graph could not be serialized.</exception>
        void Serialize(object objectGraphRoot, TextWriter textWriter);
    }
}