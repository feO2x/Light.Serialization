using System;

namespace Light.Serialization.Json.PrimitiveTypeFormatters
{
    /// <summary>
    ///     Represents the abstraction of an object that can serialize a primitive type to a non-complex JSON value (like a number or a string).
    /// </summary>
    public interface IPrimitiveTypeFormatter
    {
        /// <summary>
        ///     Gets the type that this formatter can serialize.
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        ///     Gets the value indicating whether the returned JSON string should be normalized by the JSON writer when it is used as a key in a complex JSON object.
        /// </summary>
        bool ShouldBeNormalizedKey { get; }

        /// <summary>
        ///     Serializes the specified primitive value to a JSON string.
        /// </summary>
        /// <param name="primitiveValue">The value to be formatted.</param>
        /// <returns>The serialized JSON string.</returns>
        string FormatPrimitiveType(object primitiveValue);
    }
}