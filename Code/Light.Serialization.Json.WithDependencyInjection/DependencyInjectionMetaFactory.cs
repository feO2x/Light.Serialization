using System;
using System.Collections;
using System.Collections.Generic;
using Light.DependencyInjection;
using Light.GuardClauses;
using Light.Serialization.Json.ComplexTypeConstruction;

namespace Light.Serialization.Json.WithDependencyInjection
{
    /// <summary>
    ///     Represents an <see cref="IMetaFactory" /> that utilizes a <see cref="DependencyInjectionContainer" /> to instantiate dictionaries and collections.
    /// </summary>
    public sealed class DependencyInjectionMetaFactory : IMetaFactory
    {
        private readonly DependencyInjectionContainer _container;

        /// <summary>
        ///     Initializes a new instance of <see cref="DependencyInjectionMetaFactory" />.
        /// </summary>
        /// <param name="container">The DI container that is used to instantiate dictionaries and collections.</param>
        public DependencyInjectionMetaFactory(DependencyInjectionContainer container)
        {
            container.MustNotBeNull(nameof(container));

            _container = container;
        }

        /// <inheritdoc />
        public IDictionary CreateDictionary(Type dictionaryTypeToConstruct)
        {
            return (IDictionary) _container.Resolve(dictionaryTypeToConstruct);
        }

        /// <inheritdoc />
        public IList CreateCollection(Type collectionTypeToConstruct)
        {
            return (IList) _container.Resolve(collectionTypeToConstruct);
        }

        /// <summary>
        ///     Throws a <see cref="NotSupportedException" /> because the <see cref="ComplexObjectParserUsingDi" /> does not call this method.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown when this method is called.</exception>
        public object CreateObject(TypeCreationDescription typeCreationDescription, Dictionary<InjectableValueDescription, InjectableValue> deserializedChildValues)
        {
            throw new NotSupportedException("Objects should not be created using the DependencyInjectionMetaFactory.");
        }
    }
}