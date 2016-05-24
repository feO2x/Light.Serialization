using System;

namespace Light.Serialization.Json.ComplexTypeConstruction
{
    /// <summary>
    ///     Represents the abstraction of an object that can provide a description of how
    ///     a certain type can be created with Constructor, Field, and Property injection.
    /// </summary>
    public interface ITypeDescriptionProvider
    {
        /// <summary>
        ///     Gets the type creation description for the specified type.
        /// </summary>
        /// <param name="type">The type whose creation description is requested.</param>
        /// <returns>The creation description that corresponds to the given type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
        TypeCreationDescription GetTypeCreationDescription(Type type);
    }
}