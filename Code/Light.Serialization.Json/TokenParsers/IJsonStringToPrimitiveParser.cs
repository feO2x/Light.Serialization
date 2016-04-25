using System;
using System.Collections.Generic;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents the abstraction of an extended JSON token parser that is able to deserialize JSON strings
    ///     to .NET primitive types when these values are referenced via a base type (e.g. object).
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
        /// <param name="token">The token to be parsed.</param>
        /// <returns>The parse result indicating success or failure of the parse operation.</returns>
        PrimitiveParseResult TryParse(JsonToken token);
    }
}