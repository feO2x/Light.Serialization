﻿using System;

namespace Light.Serialization.Json
{
    /// <summary>
    ///     Represents the abstraction of an object that is able to instruct the JSON writer how a certain type should be serialized.
    /// </summary>
    public interface IJsonWriterInstructor
    {
        /// <summary>
        ///     Checks if this writer instructor is able to serialize the specified object.
        /// </summary>
        /// <param name="object">The object to be serialized.</param>
        /// <param name="actualType">The actual type of the object (similar to <c>object.GetType()</c>).</param>
        /// <param name="referencedType">The type that was used to reference the object.</param>
        /// <returns>True if this writer instructor can serialize the specified object, else false.</returns>
        bool AppliesToObject(object @object, Type actualType, Type referencedType);

        /// <summary>
        ///     Serializes the object using the specified context. You must only call this method if you ensured that <see cref="AppliesToObject" /> would return true.
        /// </summary>
        /// <param name="serializationContext">The context containing all information necessary to serialize the object.</param>
        void Serialize(JsonSerializationContext serializationContext);
    }
}