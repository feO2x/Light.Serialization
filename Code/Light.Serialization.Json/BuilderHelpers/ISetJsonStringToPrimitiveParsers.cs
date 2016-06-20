using System;
using System.Collections.Generic;
using Light.GuardClauses.Exceptions;
using Light.Serialization.Json.TokenParsers;

namespace Light.Serialization.Json.BuilderHelpers
{
    /// <summary>
    ///     Represents the abstraction of setting a list of <see cref="IJsonStringToPrimitiveParser" /> instances via property injection.
    /// </summary>
    public interface ISetJsonStringToPrimitiveParsers
    {
        /// <summary>
        ///     Sets the list of <see cref="IJsonStringToPrimitiveParser" /> instances.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        /// <exception cref="EmptyCollectionException">Thrown when <paramref name="value" /> contains no items.</exception>
        List<IJsonStringToPrimitiveParser> JsonStringToPrimitiveParsers { set; }
    }
}