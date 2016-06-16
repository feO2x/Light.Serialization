﻿using System;
using Light.Serialization.Json.ObjectMetadata;

namespace Light.Serialization.Json.BuilderHelpers
{
    /// <summary>
    ///     Represents the abstraction of setting a type to name mapping using property injection.
    /// </summary>
    public interface ISetTypeToNameMapping
    {
        /// <summary>
        ///     Sets the specified type to name mapping.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        ITypeToNameMapping TypeToNameMapping { set; }
    }
}