using System;
using Light.Serialization.Json.ComplexTypeConstruction;

namespace Light.Serialization.Json.BuilderHelpers
{
    /// <summary>
    ///     Represents the abstraction of setting an <see cref="IMetaFactory" /> instance via property injection.
    /// </summary>
    public interface ISetMetaFactory
    {
        /// <summary>
        ///     Sets the <see cref="IMetaFactory" /> instance.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        IMetaFactory MetaFactory { set; }
    }
}