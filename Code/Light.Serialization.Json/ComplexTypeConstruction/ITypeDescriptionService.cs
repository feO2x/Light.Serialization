using System;
using Light.GuardClauses.Exceptions;

namespace Light.Serialization.Json.ComplexTypeConstruction
{
    /// <summary>
    ///     Represents the abstraction of an object that can provide <see cref="TypeCreationDescription" /> instances describing how
    ///     a certain <see cref="Type" /> can be instantiated with constructor, property, and field injection.
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
        ///     Normalizes the given JSON string so that it can be compared to settable type members and constructor parameters (i.e. the normalized name of <see cref="InjectableValueDescription" /> instances).
        /// </summary>
        /// <param name="jsonName">The JSON string to be normalized.</param>
        /// <returns>The normalized name of the JSON string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="jsonName" /> is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="jsonName" /> is an empty string.</exception>
        /// <exception cref="StringIsOnlyWhiteSpaceException">Thrown when <paramref name="jsonName" />contains only whitespace.</exception>
        string NormalizeName(string jsonName);
    }
}