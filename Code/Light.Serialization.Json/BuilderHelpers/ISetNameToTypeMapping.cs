using System;
using Light.Serialization.Json.ObjectMetadata;

namespace Light.Serialization.Json.BuilderHelpers
{
    /// <summary>
    ///     Represents the abstraction of setting an <see cref="INameToTypeMapping" /> via property injection.
    /// </summary>
    public interface ISetNameToTypeMapping
    {
        /// <summary>
        ///     Sets the specified <see cref="INameToTypeMapping"/> instance.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        INameToTypeMapping NameToTypeMapping { set; }
    }
}