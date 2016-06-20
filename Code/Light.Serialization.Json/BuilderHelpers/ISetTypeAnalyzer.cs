using System;
using Light.Serialization.Json.ComplexTypeDecomposition;

namespace Light.Serialization.Json.BuilderHelpers
{
    /// <summary>
    ///     Represents the abstraction of setting an <see cref="IReadableValuesTypeAnalyzer" /> instance via property injection.
    /// </summary>
    public interface ISetTypeAnalyzer
    {
        /// <summary>
        ///     Sets the specified type analyzer.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        IReadableValuesTypeAnalyzer TypeAnalyzer { set; }
    }
}