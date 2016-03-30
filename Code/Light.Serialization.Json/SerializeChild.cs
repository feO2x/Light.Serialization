using System;

namespace Light.Serialization.Json
{
    /// <summary>
    ///     Represents a delegate that should be called to serialize a child object in the graph.
    /// </summary>
    /// <param name="child">The child object to be serialized.</param>
    /// <param name="actualType">The actual type of the child object.</param>
    /// <param name="referencedType">The type that is used to reference the child object.</param>
    public delegate void SerializeChildMethod(object child, Type actualType, Type referencedType);
}