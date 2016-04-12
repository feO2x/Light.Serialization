using System;
using Light.Serialization.Json.ComplexTypeDecomposition;

namespace Light.Serialization.Json.BuilderInterfaces
{
    /// <summary>
    ///     Represents the abstraction to inject a readable values type analyzer via a property set method.
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