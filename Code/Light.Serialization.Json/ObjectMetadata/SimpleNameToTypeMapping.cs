using System;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents a bi-directional mapping from .NET types to JSON names that uses Assembly Qualified Names.
    /// </summary>
    public sealed class SimpleNameToTypeMapping : INameToTypeMapping, ITypeToNameMapping
    {
        /// <summary>
        ///     Maps the specified JSON type name that must be an Assembly Qualified Name to the corresponding .NET types.
        /// </summary>
        /// <param name="typeName">The JSON type name.</param>
        /// <returns>The .NET type that corresponds to the JSON type name.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeName" /> is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="typeName" /> is an empty string.</exception>
        /// <exception cref="StringIsOnlyWhiteSpaceException">Thrown when <paramref name="typeName" /> contains only whitespace.</exception>
        public Type Map(string typeName)
        {
            typeName.MustNotBeNullOrWhiteSpace(nameof(typeName));

            return Type.GetType(typeName);
        }

        /// <summary>
        ///     Returns the Assembly Qualified Name of the specified .NET type.
        /// </summary>
        /// <param name="type">The .NET type.</param>
        /// <returns>The Assembly Qualified Name of the specified type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
        public string Map(Type type)
        {
            type.MustNotBeNull(nameof(type));

            return type.AssemblyQualifiedName;
        }

        /// <summary>
        ///     Tries to map the specified type to a string that can be used in the JSON document.
        /// </summary>
        /// <param name="type">The type whose JSON name should be returned.</param>
        /// <returns>The JSON name of the specified type or null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
        public string TryMap(Type type)
        {
            return Map(type);
        }
    }
}