using System;
using System.Collections.Generic;
using Light.Serialization.Json.PrimitiveTypeFormatters;

namespace Light.Serialization.Json.BuilderInjection
{
    /// <summary>
    ///     Represents the abstraction to inject a primitive type formatters dictionary via a property set method.
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