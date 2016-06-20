using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Light.GuardClauses;
using Light.Serialization.Json.ComplexTypeConstruction;
using Microsoft.Practices.Unity;

namespace Light.Serialization.Json.Unity
{
    /// <summary>
    ///     Represents an adapter that adapts the Unity Container to the IMetaFactory interface.
    /// </summary>
    public sealed class UnityMetaFactoryAdapter : IMetaFactory
    {
        private readonly IUnityContainer _diContainer;

        /// <summary>
        ///     Creates a new instance of UnityMetaFactoryAdapter.
        /// </summary>
        /// <param name="diContainer">The Unity Container that this adapter connects to.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="diContainer" /> is null.</exception>
        public UnityMetaFactoryAdapter(IUnityContainer diContainer)
        {
            diContainer.MustNotBeNull(nameof(diContainer));

            _diContainer = diContainer;
        }

        /// <summary>
        ///     Resolves the requested dictionary type to a dictionary instance using the Unity Container.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dictionaryTypeToConstruct" /> is null.</exception>
        public IDictionary CreateDictionary(Type dictionaryTypeToConstruct)
        {
            dictionaryTypeToConstruct.MustNotBeNull(nameof(dictionaryTypeToConstruct));

            return (IDictionary) _diContainer.Resolve(dictionaryTypeToConstruct);
        }

        /// <summary>
        ///     Resolves the requested collection type to a collection instance using the Unity Container.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="collectionTypeToConstruct" /> is null.</exception>
        public IList CreateCollection(Type collectionTypeToConstruct)
        {
            collectionTypeToConstruct.MustNotBeNull(nameof(collectionTypeToConstruct));

            return (IList) _diContainer.Resolve(collectionTypeToConstruct);
        }

        /// <summary>
        ///     Creates the target object from the specified creation description and the deserialized child values.
        ///     The DI container is used to inject values that are not present in the deserialized child values.
        /// </summary>
        /// <param name="typeCreationDescription">The creation description for the type to be instantiated.</param>
        /// <param name="deserializedChildValues">The child values that were deserialized from the JSON document.</param>
        /// <returns>The instantiated object.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeCreationDescription" /> is null.</exception>
        public object CreateObject(TypeCreationDescription typeCreationDescription, Dictionary<InjectableValueDescription, InjectableValue> deserializedChildValues)
        {
            typeCreationDescription.MustNotBeNull(nameof(typeCreationDescription));

            var targetConstructorDescription = typeCreationDescription.ConstructorDescriptions
                                                                      .OrderByDescending(c => c.ConstructorParameters.Count)
                                                                      .First();

            object newObject;
            if (targetConstructorDescription.ConstructorParameters.Count == 0)
            {
                newObject = targetConstructorDescription.TryCallConstructor(null);
                _diContainer.BuildUp(newObject);
            }
            else
            {
                var parameterOverrides = new ParameterOverrides();
                foreach (var injectableValueDescription in targetConstructorDescription.ConstructorParameters)
                {
                    InjectableValue injectableValue;
                    if (deserializedChildValues == null || deserializedChildValues.TryGetValue(injectableValueDescription, out injectableValue) == false)
                        continue;

                    parameterOverrides.Add(injectableValueDescription.ConstructorParameterInfo.Name, injectableValue.Inject());
                    deserializedChildValues[injectableValueDescription] = injectableValue;
                }

                newObject = _diContainer.Resolve(typeCreationDescription.TargetType, parameterOverrides);
            }

            // Perform property and field injection if necessary
            DefaultMetaFactory.PerformPropertyAndFieldInjection(typeCreationDescription, deserializedChildValues, newObject);

            return newObject;
        }
    }
}