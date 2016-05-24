using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.Serialization.Json.ComplexTypeConstruction;

namespace Light.Serialization.Json.Caching
{
    /// <summary>
    ///     Represents a decorator for the ITypeDescriptionProvider interface that caches Type Creation Descriptions.
    /// </summary>
    public sealed class CreationDescriptionCacheDecorator : ITypeDescriptionProvider
    {
        private readonly Dictionary<Type, TypeCreationDescription> _cache;
        private readonly ITypeDescriptionProvider _decoratedProvider;

        /// <summary>
        ///     Creates a new instance of CreationDescriptionCacheDecorator.
        /// </summary>
        /// <param name="cache">The dictionary that serves as the cache.</param>
        /// <param name="decoratedProvider">The provider that is decorated.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cache" /> or <paramref name="decoratedProvider" /> is null.</exception>
        public CreationDescriptionCacheDecorator(Dictionary<Type, TypeCreationDescription> cache, ITypeDescriptionProvider decoratedProvider)
        {
            cache.MustNotBeNull(nameof(cache));
            decoratedProvider.MustNotBeNull(nameof(decoratedProvider));

            _cache = cache;
            _decoratedProvider = decoratedProvider;
        }

        /// <summary>
        ///     Tries to get the creation description from the cache. If this is not possible,
        ///     the call is forwared to the decorated provider.
        /// </summary>
        public TypeCreationDescription GetTypeCreationDescription(Type type)
        {
            TypeCreationDescription creationDescription;
            if (_cache.TryGetValue(type, out creationDescription))
                return creationDescription;

            creationDescription = _decoratedProvider.GetTypeCreationDescription(type);
            _cache.Add(type, creationDescription);
            return creationDescription;
        }
    }
}