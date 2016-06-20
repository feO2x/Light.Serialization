using System;

namespace Light.Serialization.Json.ComplexTypeDecomposition
{
    /// <summary>
    ///     Represents the abstraction of an object that can be used to read a single value from another object.
    /// </summary>
    public interface IValueReader
    {
        /// <summary>
        ///     Gets the name of the member whose value is read.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the type of the member that is used to reference the actual value.
        /// </summary>
        Type ReferenceType { get; }

        /// <summary>
        ///     Reads the value from the specified object and returns it.
        /// </summary>
        /// <param name="object">The objects whose child value will be read.</param>
        /// <returns>The read value of the object.</returns>
        object ReadValue(object @object);
    }
}