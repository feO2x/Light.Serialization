using System;
using Light.GuardClauses.Exceptions;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents the abstraction of an object that maps a JSON name to a .NET type.
    /// </summary>
    public interface INameToTypeMapping
    {
        /// <summary>
        ///     Maps the specified JSON string to a .NET type.
        /// </summary>
        /// <param name="typeName">The string representing a type name in the JSON document.</param>
        /// <returns>The type that corresponds to the specified JSON name.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeName" /> is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="typeName" /> is an empty string.</exception>
        /// <exception cref="StringIsOnlyWhiteSpaceException">Thrown when <paramref name="typeName" /> contains only whitespace.</exception>
        Type Map(string typeName);
    }
}