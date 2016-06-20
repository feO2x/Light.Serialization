using System;
using Light.Serialization.Json.ComplexTypeConstruction;

namespace Light.Serialization.Json.BuilderHelpers
{
    /// <summary>
    ///     Represents the abstraction for setting a <see cref="ISetTypeDescriptionService" /> instance via property injection.
    /// </summary>
    public interface ISetTypeDescriptionService
    {
        /// <summary>
        ///     Sets the specified <see cref="ITypeDescriptionService" /> instance.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        ITypeDescriptionService TypeDescriptionService { set; }
    }
}