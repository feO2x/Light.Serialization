using System;
using System.Collections.Generic;

namespace Light.Serialization.Json.ComplexTypeDecomposition
{
    /// <summary>
    ///     Represents the abstraction of an object that can analyze a type for members that grant reading access to the data of the object.
    /// </summary>
    public interface IReadableValuesTypeAnalyzer
    {
        /// <summary>
        ///     Analyzes the specified type and returns a list of value readers that can be used to read values from an object.
        /// </summary>
        /// <param name="type">The type to be analyzed.</param>
        /// <returns>The list of <see cref="IValueReader"/> instances.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
        List<IValueReader> AnalyzeType(Type type);
    }
}