using System;

namespace Light.Serialization.Abstractions
{
    /// <summary>
    ///     Represents an object that can deserialize a string to an object graph.
    /// </summary>
    public interface IDeserializer
    {
        /// <summary>
        ///     Deserializes the specified string to an object graph.
        /// </summary>
        /// <typeparam name="T">The type used to reference the root of the deserialized object graph.</typeparam>
        /// <param name="serializedString">The string to be deserialized.</param>
        /// <returns>The deserialized object graph.</returns>
        /// <exception cref="DeserializationException">Thrown when the specified string is malformed or some part of it could not be deserialized correctly.</exception>
        T Deserialize<T>(string serializedString);

        /// <summary>
        ///     Deserializes the specified string to an object graph.
        /// </summary>
        /// <param name="serializedString">The string to be deserialized.</param>
        /// <param name="requestedType">The type used to reference the root of the deserialized object.</param>
        /// <returns>The deserialized object graph.</returns>
        /// <exception cref="DeserializationException">Thrown when the specified string is malformed or some part of it could not be deserialized correctly.</exception>
        object Deserialize(string serializedString, Type requestedType);
    }
}