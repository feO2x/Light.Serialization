using System;
using Light.GuardClauses;
using Light.Serialization.Json.LowLevelWriting;

namespace Light.Serialization.Json
{
    /// <summary>
    /// Represents all information necessary to serialize a single object of a graph.
    /// </summary>
    public struct JsonSerializationContext
    {
        /// <summary>
        /// Gets the object that should be serialized. This value is never null.
        /// </summary>
        public readonly object ObjectToBeSerialized;
        /// <summary>
        /// Gets the actual type of the object (similar to <c>ObjectToBeSerialized.GetType()</c>).
        /// </summary>
        public readonly Type ActualType;
        /// <summary>
        /// Gets the type that was used to reference the object (this might be a base class / an interface that the ActualType implements).
        /// </summary>
        public readonly Type ReferencedType;
        /// <summary>
        /// Gets the delegate you can use to serialize a child object
        /// </summary>
        public readonly SerializeChildMethod SerializeChild;
        /// <summary>
        /// Gets the writer that creates the actual JSON document.
        /// </summary>
        public readonly IJsonWriter Writer;

        /// <summary>
        /// Creates a new instance of <see cref="JsonSerializationContext"/>.
        /// </summary>
        /// <param name="objectToBeSerialized">The object to be serialized.</param>
        /// <param name="actualType">The actual type of the object to be serialized.</param>
        /// <param name="referencedType">The type that is used to reference the object to be serialized.</param>
        /// <param name="serializeChild">The delegate that should be used to serialize child objects.</param>
        /// <param name="writer">The object that writes the actual JSON document.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the specified parameters is null.</exception>
        public JsonSerializationContext(object objectToBeSerialized,
                                        Type actualType,
                                        Type referencedType,
                                        SerializeChildMethod serializeChild,
                                        IJsonWriter writer)
        {
            objectToBeSerialized.MustNotBeNull(nameof(objectToBeSerialized));
            actualType.MustNotBeNull(nameof(actualType));
            referencedType.MustNotBeNull(nameof(referencedType));
            serializeChild.MustNotBeNull(nameof(serializeChild));
            writer.MustNotBeNull(nameof(writer));
            
            ObjectToBeSerialized = objectToBeSerialized;
            ActualType = actualType;
            ReferencedType = referencedType;
            SerializeChild = serializeChild;
            Writer = writer;
        }
    }
}