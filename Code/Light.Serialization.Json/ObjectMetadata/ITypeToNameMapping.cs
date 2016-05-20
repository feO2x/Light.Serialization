using System;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents the abstraction of an object that maps a .NET type to a JSON name.
    /// </summary>
    public interface ITypeToNameMapping
    {
        /// <summary>
        ///     Maps the specified .NET type to a string that can be used in the JSON document.
        /// </summary>
        /// <param name="type">The type whose JSON name should be returned.</param>
        /// <returns>The JSON name of the specified type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
        string Map(Type type);

        /// <summary>
        ///     Tries to map the specified type to a string that can be used in the JSON document.
        /// </summary>
        /// <param name="type">The type whose JSON name should be returned.</param>
        /// <returns>The JSON name of the specified type or null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
        string TryMap(Type type);
    }
}