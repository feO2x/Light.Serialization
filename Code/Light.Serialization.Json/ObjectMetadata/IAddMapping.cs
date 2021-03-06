﻿using System;
using Light.GuardClauses.Exceptions;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents the abstraction of adding a bidirectional one-to-one mapping from JSON name to .NET type.
    /// </summary>
    public interface IAddMapping
    {
        /// <summary>
        ///     Adds the specified mapping from JSON name to .NET type.
        /// </summary>
        /// <param name="jsonName">The JSON name that will occur in the document.</param>
        /// <param name="correspondingType">The .NET type that corresponds to the JSON name.</param>
        /// <returns>The mapping for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="jsonName" /> is an empty string.</exception>
        /// <exception cref="StringIsOnlyWhiteSpaceException">Thrown when <paramref name="jsonName" /> contains only whitespace.</exception>
        IAddMapping AddMapping(string jsonName, Type correspondingType);
    }
}