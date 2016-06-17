using System;
using Light.Serialization.Json.ComplexTypeConstruction;

namespace Light.Serialization.Json.BuilderHelpers
{
    /// <summary>
    ///     Represents the abstraction to set an <see cref="IMetaFactory" /> instance on the target object through property injection.
    /// </summary>
    public interface ISetMetaFactory
    {
        /// <summary>
        ///     Sets the <see cref="IMetaFactory" /> instance on the target object.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        IMetaFactory MetaFactory { set; }
    }
}