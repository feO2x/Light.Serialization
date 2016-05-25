using System;
using Light.Serialization.Json.ObjectMetadata;

namespace Light.Serialization.Json.BuilderInterfaces
{
    /// <summary>
    ///     Represents the abstraction of setting a name to type mapping using property injection.
    /// </summary>
    public interface ISetNameToTypeMapping
    {
        /// <summary>
        ///     Sets the specified name to type mapping.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        INameToTypeMapping NameToTypeMapping { set; }
    }
}