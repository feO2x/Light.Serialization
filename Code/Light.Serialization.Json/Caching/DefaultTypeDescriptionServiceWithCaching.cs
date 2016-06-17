using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.Serialization.Json.ComplexTypeConstruction;

namespace Light.Serialization.Json.Caching
{
    /// <summary>
    ///     Represents a <see cref="DefaultTypeDescriptionService" /> that uses caching.
    /// </summary>
    public class DefaultTypeDescriptionServiceWithCaching : DefaultTypeDescriptionService
    {
        private readonly Dictionary<Type, TypeCreationDescription> _typeCreationDescriptionCache;

        /// <summary>
        ///     Creates a new instance of <see cref="DefaultTypeDescriptionServiceWithCaching" />.
        /// </summary>
        /// <param name="typeCreationDescriptionCache">The dictionary that is used as a cache for <see cref="TypeCreationDescription" /> instances.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeCreationDescriptionCache" /> is null.</exception>
        public DefaultTypeDescriptionServiceWithCaching(Dictionary<Type, TypeCreationDescription> typeCreationDescriptionCache)
        {
            typeCreationDescriptionCache.MustNotBeNull(nameof(typeCreationDescriptionCache));

            _typeCreationDescriptionCache = typeCreationDescriptionCache;
        }

        /// <summary>
        ///     Tries to get a <see cref="TypeCreationDescription" /> instance from the cache. If this is not possible,
        ///     the base class is used to create the instance.
        /// </summary>
        public override TypeCreationDescription GetTypeCreationDescription(Type type)
        {
            TypeCreationDescription description;

            lock (_typeCreationDescriptionCache)
            {
                if (_typeCreationDescriptionCache.TryGetValue(type, out description))
                    return description;

                description = base.GetTypeCreationDescription(type);
                if (_typeCreationDescriptionCache.ContainsKey(type) == false)
                    _typeCreationDescriptionCache.Add(type, description);
            }

            return description;
        }
    }
}