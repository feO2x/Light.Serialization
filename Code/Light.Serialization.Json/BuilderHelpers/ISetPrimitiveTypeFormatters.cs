using System;
using System.Collections.Generic;
using Light.Serialization.Json.PrimitiveTypeFormatters;

namespace Light.Serialization.Json.BuilderHelpers
{
    /// <summary>
    ///     Represents the abstraction of setting a primitive type formatters dictionary via property injection.
    /// </summary>
    public interface ISetPrimitiveTypeFormatters
    {
        /// <summary>
        ///     Sets the specified dictionary containing mappings from type to primitive formatters.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        IDictionary<Type, IPrimitiveTypeFormatter> PrimitiveTypeFormattersMapping { set; }
    }
}