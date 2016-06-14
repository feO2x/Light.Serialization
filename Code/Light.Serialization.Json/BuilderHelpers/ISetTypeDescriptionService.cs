using System;
using Light.Serialization.Json.ComplexTypeConstruction;

namespace Light.Serialization.Json.BuilderHelpers
{
    /// <summary>
    ///     Represents the abstraction for setting a <see cref="ISetTypeDescriptionService" /> instance on token parser.
    /// </summary>
    public interface ISetTypeDescriptionService
    {
        /// <summary>
        ///     Sets the specified type description service on the target instance.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        ITypeDescriptionService TypeDescriptionService { set; }
    }
}