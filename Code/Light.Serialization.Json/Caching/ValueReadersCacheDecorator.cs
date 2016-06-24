using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.Serialization.Json.ComplexTypeDecomposition;

namespace Light.Serialization.Json.Caching
{
    /// <summary>
    ///     Represents a cache decorator for collections of value readers, belonging to a certain type.
    ///     Access to the cache is synchronized, thus the cache can be shared in several serialization object graphs.
    /// </summary>
    public sealed class ValueReadersCacheDecorator : IReadableValuesTypeAnalyzer
    {
        private readonly Dictionary<Type, List<IValueReader>> _cache;
        private readonly IReadableValuesTypeAnalyzer _decoratedAnalyzer;

        /// <summary>
        ///     Creates a new instance of <see cref="ValueReadersCacheDecorator" />.
        /// </summary>
        /// <param name="decoratedAnalyzer">The type analyzer that is decorated by this cache.</param>
        /// <param name="cache">The dictionary that is used for caching. It may be empty or prefilled.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public ValueReadersCacheDecorator(IReadableValuesTypeAnalyzer decoratedAnalyzer, Dictionary<Type, List<IValueReader>> cache)
        {
            decoratedAnalyzer.MustNotBeNull(nameof(decoratedAnalyzer));
            cache.MustNotBeNull(nameof(cache));

            _decoratedAnalyzer = decoratedAnalyzer;
            _cache = cache;
        }

        /// <summary>
        ///     Checks if the value readers for the given type can be found in the cache, or else forwards the call to the decorated type analyzer.
        /// </summary>
        /// <param name="type">The type whose value readers should be found or created.</param>
        /// <returns>The value readers for the given type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
        public List<IValueReader> AnalyzeType(Type type)
        {
            type.MustNotBeNull(nameof(type));

            List<IValueReader> targetValueReaders;
            lock (_cache)
            {
                if (_cache.TryGetValue(type, out targetValueReaders))
                    return targetValueReaders;
                targetValueReaders = _decoratedAnalyzer.AnalyzeType(type);
                _cache.Add(type, targetValueReaders);
            }

            return targetValueReaders;
        }
    }
}