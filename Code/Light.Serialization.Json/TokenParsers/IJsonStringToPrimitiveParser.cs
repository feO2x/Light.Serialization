using System;
using System.Collections.Generic;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents the abstraction of an extended <see cref="IJsonTokenParser" /> that is able to deserialize JSON strings
    ///     to .NET primitive types when these values are referenced via a base type (e.g. <see cref="object" /> or <see cref="ValueType" />).
    /// </summary>
    public interface IJsonStringToPrimitiveParser : IJsonTokenParser
    {
        /// <summary>
        ///     The collection containing all interfaces and base classes that the .NET
        ///     primitive type implements or derives from.
        /// </summary>
        IReadOnlyList<Type> AssociatedInterfacesAndBaseClasses { get; }

        /// <summary>
        ///     Tries to parse the specified token as a .NET primitive type.
        /// </summary>
        /// <param name="context">The deserialization context for the JSON string that might represent a primitive type other than <see cref="string" />.</param>
        /// <param name="deserializedString">The string representation of the JSON string without the outer quotation marks.</param>
        /// <returns>The parse result indicating success or failure of the parse operation.</returns>
        JsonStringParseResult TryParse(JsonDeserializationContext context, string deserializedString);
    }
}