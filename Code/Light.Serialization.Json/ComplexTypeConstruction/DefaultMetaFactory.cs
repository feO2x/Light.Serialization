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
        /// <summary>
        ///     Creates a new dictionary for the given dictionary type.
        /// </summary>
        /// <param name="requestedDictionaryType">The dictioary abstraction or concrete type that should be instantiated.</param>
        /// <returns>An instance of the given concrete type, or a new instance of the default dictionary type when the requested type is a dictionary abstraction.</returns>
        public IDictionary CreateDictionary(Type requestedDictionaryType)
        {
            var dictionary = TryToInstantiateWithDefaultConstructor(requestedDictionaryType);
            if (dictionary != null)
                return (IDictionary) dictionary;

            var createDefaultDictionaryMethod = GetType().GetTypeInfo().GetDeclaredMethod(nameof(CreateDefaultDictionary));
            var resolvedMethod = createDefaultDictionaryMethod.MakeGenericMethod(requestedDictionaryType.GenericTypeArguments);

            return (IDictionary) resolvedMethod.Invoke(this, null);
        }

        /// <summary>
        ///     Creates a new collection for the given collection type.
        /// </summary>
        /// <param name="requestedCollectionType">The collection abstraction or concrete type that should be instantiated.</param>
        /// <returns>An instance of the given concrete type, or a new instance of the default collection type when the requested type is a collection abstraction.</returns>
        public IList CreateCollection(Type requestedCollectionType)
        {
            var collection = TryToInstantiateWithDefaultConstructor(requestedCollectionType);

            if (collection != null)
                return (IList) collection;

            var createDefaultCollectionMethod = GetType().GetTypeInfo().GetDeclaredMethod(nameof(CreateDefaultCollection));
            var resolvedMethod = createDefaultCollectionMethod.MakeGenericMethod(requestedCollectionType.GenericTypeArguments);

            return (IList) resolvedMethod.Invoke(this, null);
        }

        /// <summary>
        ///     Creates an instance of the specified object and performs constructor, property and field injection if possible.
        /// </summary>
        /// <param name="typeCreationDescription">The concrete type to be instantiated.</param>
        /// <param name="deserializedChildValues">The dictionary containing all deserialized values for the object to be created.</param>
        /// <returns>The instantiated object.</returns>
        public object Create(TypeCreationDescription typeCreationDescription, Dictionary<InjectableValueDescription, object> deserializedChildValues)
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

        private static object TryToInstantiateWithDefaultConstructor(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsClass == false || typeInfo.IsAbstract)
                return null;

            var defaultConstructor = typeInfo.DeclaredConstructors.First(c => c.GetParameters().Length == 0);
            return defaultConstructor?.Invoke(null);
        }

        protected virtual IDictionary<TKey, TValue> CreateDefaultDictionary<TKey, TValue>()
        {
            return new Dictionary<TKey, TValue>();
        }

        protected virtual ICollection<T> CreateDefaultCollection<T>()
        {
            return new List<T>();
        }
    }
}