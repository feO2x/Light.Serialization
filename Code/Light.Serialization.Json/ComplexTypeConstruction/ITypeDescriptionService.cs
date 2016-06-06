using System;
using Light.GuardClauses.Exceptions;

namespace Light.Serialization.Json.ComplexTypeConstruction
{
    /// <summary>
    ///     Represents the abstraction of an object that can provide a description of how
    ///     a certain type can be created with Constructor, Field, and Property injection.
    /// </summary>
    public interface ITypeDescriptionService
    {
        /// <summary>
        ///     Gets the type creation description for the specified type.
        /// </summary>
        /// <param name="type">The type whose creation description is requested.</param>
        /// <returns>The creation description that corresponds to the given type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
        TypeCreationDescription GetTypeCreationDescription(Type type);

        /// <summary>
        ///     Normalizes the given JSON string so that it can be used to compare it to settable type members and constructor parameters (Injectable Value Info).
        /// </summary>
        /// <param name="jsonName">The JSON string to be normalized.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="jsonName" /> is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="jsonName" /> is an empty string.</exception>
        /// <exception cref="StringIsOnlyWhiteSpaceException">Thrown when <paramref name="jsonName" />contains only whitespace.</exception>
        string NormalizeName(string jsonName);
    }
}