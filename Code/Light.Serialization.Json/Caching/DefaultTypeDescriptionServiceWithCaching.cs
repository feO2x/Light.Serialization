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
        private readonly object _lockObject = new object();
        private Dictionary<Type, TypeCreationDescription> _typeCreationDescriptionCache;

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
        ///     Gets or sets the dictionary that is used as the cache for <see cref="TypeCreationDescription" /> instances.
        /// </summary>
        public Dictionary<Type, TypeCreationDescription> TypeCreationDescriptionCache
        {
            get { return _typeCreationDescriptionCache; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _typeCreationDescriptionCache = value;
            }
        }

        /// <summary>
        ///     Tries to get a <see cref="TypeCreationDescription" /> instance from the cache. If this is not possible,
        ///     the base class is used to create the instance.
        /// </summary>
        public override TypeCreationDescription GetTypeCreationDescription(Type type)
        {
            TypeCreationDescription description;
            // ReSharper disable once InconsistentlySynchronizedField
            if (_typeCreationDescriptionCache.TryGetValue(type, out description))
                return description;

            description = base.GetTypeCreationDescription(type);
            lock (_lockObject)
            {
                if (_typeCreationDescriptionCache.ContainsKey(type) == false)
                    _typeCreationDescriptionCache.Add(type, description);
            }

            return description;
        }
    }
}