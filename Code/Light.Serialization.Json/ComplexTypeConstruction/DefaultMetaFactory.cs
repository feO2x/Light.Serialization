using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Light.GuardClauses;
using Light.Serialization.Abstractions;

namespace Light.Serialization.Json.ComplexTypeConstruction
{
    /// <summary>
    ///     Represents the default Metafactory that uses Dictionary of TKey, TValue as default dictionaries, List of T as default collections
    ///     and simple constructor, property, and field injection for complex objects.
    /// </summary>
    public class DefaultMetaFactory : IMetaFactory
    {
        private readonly MethodInfo _createDefaultDictionaryMethod;
        private readonly MethodInfo _createDefaultCollectionMethod;

        public DefaultMetaFactory()
        {
            var typeInfo = GetType().GetTypeInfo();
            _createDefaultCollectionMethod = typeInfo.GetDeclaredMethod(nameof(CreateDefaultCollection));
            _createDefaultDictionaryMethod = typeInfo.GetDeclaredMethod(nameof(CreateDefaultDictionary));
        }

        /// <summary>
        ///     Creates a new dictionary for the given dictionary type.
        /// </summary>
        /// <param name="dictionaryTypeToConstruct">The dictioary abstraction or concrete type that should be instantiated.</param>
        /// <returns>An instance of the given concrete type, or a new instance of the default dictionary type when the requested type is a dictionary abstraction.</returns>
        public IDictionary CreateDictionary(Type dictionaryTypeToConstruct)
        {
            var dictionary = TryToInstantiateWithDefaultConstructor<IDictionary>(dictionaryTypeToConstruct.GetTypeInfo());
            if (dictionary != null)
                return dictionary;

            if (dictionaryTypeToConstruct.IsConstructedGenericType == false)
                throw new ArgumentException("The specified type cannot be associated with a generic dictionary type and therefore, no dictionary can be created.");

            var resolvedMethod = _createDefaultDictionaryMethod.MakeGenericMethod(dictionaryTypeToConstruct.GenericTypeArguments);

            return (IDictionary) resolvedMethod.Invoke(this, null);
        }

        /// <summary>
        ///     Creates a new collection for the given collection type.
        /// </summary>
        /// <param name="collectionTypeToConstruct">The collection abstraction or concrete type that should be instantiated.</param>
        /// <returns>An instance of the given concrete type, or a new instance of the default collection type when the requested type is a collection abstraction.</returns>
        public IList CreateCollection(Type collectionTypeToConstruct)
        {
            var collection = TryToInstantiateWithDefaultConstructor<IList>(collectionTypeToConstruct.GetTypeInfo());
            if (collection != null)
                return collection;

            if (collectionTypeToConstruct.IsConstructedGenericType == false)
                throw new ArgumentException("The specified type cannot be associated with a generic collection type and therefore, no collection can be created.");

            var resolvedMethod = _createDefaultCollectionMethod.MakeGenericMethod(collectionTypeToConstruct.GenericTypeArguments);

            return (IList) resolvedMethod.Invoke(this, null);
        }

        /// <summary>
        ///     Creates an instance of the specified object and performs constructor, property and field injection if possible.
        /// </summary>
        /// <param name="typeCreationDescription">The concrete type to be instantiated.</param>
        /// <param name="deserializedChildValues">The dictionary containing all deserialized values for the object to be created.</param>
        /// <returns>The instantiated object.</returns>
        public object CreateObject(TypeCreationDescription typeCreationDescription, Dictionary<InjectableValueDescription, object> deserializedChildValues)
        {
            typeCreationDescription.MustNotBeNull(nameof(typeCreationDescription));

            if (deserializedChildValues == null || deserializedChildValues.Count == 0)
            {
                var defaultConstructorDescription = typeCreationDescription.ConstructorDescriptions.FirstOrDefault(c => c.ConstructorParameters.Count == 0);
                if (defaultConstructorDescription == null)
                    throw new DeserializationException($"Could not create instance of type {typeCreationDescription.TargetType.FullName} because there was not any JSON data and no default constructor."); // TODO: maybe we can express this a little bit clearer

                return defaultConstructorDescription.TryCallConstructor(null);
            }

            object newObject = null;
            foreach (var constructorDescription in typeCreationDescription.ConstructorDescriptions
                                                                          .OrderByDescending(c => c.ConstructorParameters.Count))
            {
                newObject = constructorDescription.TryCallConstructor(deserializedChildValues);
                if (newObject != null)
                    goto SetPropertiesAndFields;
            }

            if (newObject == null)
                throw new DeserializationException($"The specified type {typeCreationDescription.TargetType.FullName} cannot be created with the given type info."); // TODO: add the deserialized values to this exception message

            SetPropertiesAndFields:
            foreach (var injectablePropertyInfo in deserializedChildValues.Keys.Where(i => i.Kind == InjectableValueKind.PropertySetter))
            {
                injectablePropertyInfo.SetPropertyValue(newObject, deserializedChildValues[injectablePropertyInfo]);
            }

            foreach (var injectableFieldInfo in deserializedChildValues.Keys.Where(i => i.Kind == InjectableValueKind.SettableField))
            {
                injectableFieldInfo.SetFieldValue(newObject, deserializedChildValues[injectableFieldInfo]);
            }

            // TODO: set values that could not be matched with a constructor parameter, property setter or public field to a dictionary of some kind if possible

            return newObject;
        }

        private static T TryToInstantiateWithDefaultConstructor<T>(TypeInfo typeInfo) where T : class
        {
            if (typeInfo.IsClass == false || typeInfo.IsAbstract)
                return null;

            var defaultConstructor = typeInfo.DeclaredConstructors.FirstOrDefault(c => c.GetParameters().Length == 0);
            return (T) defaultConstructor?.Invoke(null);
        }

        protected virtual IDictionary CreateDefaultDictionary<TKey, TValue>()
        {
            return new Dictionary<TKey, TValue>();
        }

        protected virtual IList CreateDefaultCollection<T>()
        {
            return new List<T>();
        }
    }
}