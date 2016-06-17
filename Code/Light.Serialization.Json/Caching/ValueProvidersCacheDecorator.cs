using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.Serialization.Json.ComplexTypeDecomposition;

namespace Light.Serialization.Json.Caching
{
    /// <summary>
    ///     Represents a cache decorator for collections of value providers, belonging to a certain type.
    ///     Access to the cache is synchronized, thus the cache can be shared in several instances.
    /// </summary>
    public sealed class ValueProvidersCacheDecorator : IReadableValuesTypeAnalyzer
    {
        private readonly IDictionary<Type, IList<IValueProvider>> _cache;
        private readonly IReadableValuesTypeAnalyzer _decoratedAnalyzer;

        /// <summary>
        ///     Creates a new instance of ValueProvidersCacheDecorator.
        /// </summary>
        /// <param name="decoratedAnalyzer">The type analyzer that is decorated by this cache.</param>
        /// <param name="cache">The dictionary that is used for caching. It may be empty or prefilled.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public ValueProvidersCacheDecorator(IReadableValuesTypeAnalyzer decoratedAnalyzer, IDictionary<Type, IList<IValueProvider>> cache)
        {
            decoratedAnalyzer.MustNotBeNull(nameof(decoratedAnalyzer));
            cache.MustNotBeNull(nameof(cache));

            _decoratedAnalyzer = decoratedAnalyzer;
            _cache = cache;
        }

        /// <summary>
        ///     Checks if the value providers for the given type can be found in the cache, or else forwards the call to the decorated type analyzer.
        /// </summary>
        /// <param name="type">The type whose value providers should be found or created.</param>
        /// <returns>The value providers for the given type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
        public IList<IValueProvider> AnalyzeType(Type type)
        {
            type.MustNotBeNull(nameof(type));

            IList<IValueProvider> targetValueProviders;
            lock (_cache)
            {
                if (_cache.TryGetValue(type, out targetValueProviders))
                    return targetValueProviders;
                targetValueProviders = _decoratedAnalyzer.AnalyzeType(type);
                _cache.Add(type, targetValueProviders);
            }

            return targetValueProviders;
        }
    }
}