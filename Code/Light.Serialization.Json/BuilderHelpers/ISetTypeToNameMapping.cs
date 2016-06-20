using System;
using Light.Serialization.Json.ObjectMetadata;

namespace Light.Serialization.Json.BuilderHelpers
{
    /// <summary>
    ///     Represents the abstraction of setting an <see cref="ITypeToNameMapping" /> instance via property injection.
    /// </summary>
    public interface ISetTypeToNameMapping
    {
        /// <summary>
        ///     Sets the specified <see cref="ITypeToNameMapping" /> instance.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        ITypeToNameMapping TypeToNameMapping { set; }
    }
}