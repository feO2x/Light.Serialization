using System;
using System.Collections.Generic;

namespace Light.Serialization.Json.ComplexTypeDecomposition
{
    /// <summary>
    /// Represents the abstraction of an object that can analyze a type for members that read values from an object.
    /// </summary>
    public interface IReadableValuesTypeAnalyzer
    {
        /// <summary>
        /// Analyzes the specified type and returns a list of value providers that can be used to read values from an object.
        /// </summary>
        /// <param name="type">The type to be analyzed.</param>
        /// <returns>The list of value providers.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
        IList<IValueProvider> AnalyzeType(Type type);
    }
}