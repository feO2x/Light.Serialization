using System;
using System.Collections;
using System.Collections.Generic;
using Light.DependencyInjection;
using Light.GuardClauses;
using Light.Serialization.Json.ComplexTypeConstruction;

namespace Light.Serialization.Json.WithDependencyInjection
{
    public sealed class DependencyInjectionMetaFactory : IMetaFactory
    {
        private readonly DependencyInjectionContainer _container;

        public DependencyInjectionMetaFactory(DependencyInjectionContainer container)
        {
            container.MustNotBeNull(nameof(container));

            _container = container;
        }

        public IDictionary CreateDictionary(Type dictionaryTypeToConstruct)
        {
            return (IDictionary) _container.Resolve(dictionaryTypeToConstruct);
        }

        public IList CreateCollection(Type collectionTypeToConstruct)
        {
            return (IList) _container.Resolve(collectionTypeToConstruct);
        }

        public object CreateObject(TypeCreationDescription typeCreationDescription, Dictionary<InjectableValueDescription, InjectableValue> deserializedChildValues)
        {
            throw new NotSupportedException("Objects should not be created using the DependencyInjectionMetaFactory.");
        }
    }
}