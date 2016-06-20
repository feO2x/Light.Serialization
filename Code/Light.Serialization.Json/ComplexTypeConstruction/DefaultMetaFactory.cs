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
    ///     Represents the default <see cref="IMetaFactory" /> that uses <see cref="Dictionary{TKey,TValue}" /> instances as default dictionaries,
    ///     <see cref="List{T}" /> instances as default collections, and simple constructor, property, and field injection for complex objects.
    /// </summary>
    public class DefaultMetaFactory : IMetaFactory
    {
        private readonly MethodInfo _createDefaultCollectionMethod;
        private readonly MethodInfo _createDefaultDictionaryMethod;

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
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dictionaryTypeToConstruct" /> is null.</exception>
        public IDictionary CreateDictionary(Type dictionaryTypeToConstruct)
        {
            dictionaryTypeToConstruct.MustNotBeNull(nameof(dictionaryTypeToConstruct));

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
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="collectionTypeToConstruct" /> is null.</exception>
        public IList CreateCollection(Type collectionTypeToConstruct)
        {
            collectionTypeToConstruct.MustNotBeNull(nameof(collectionTypeToConstruct));

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
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeCreationDescription" /> is null.</exception>
        public object CreateObject(TypeCreationDescription typeCreationDescription, Dictionary<InjectableValueDescription, InjectableValue> deserializedChildValues)
        {
            typeCreationDescription.MustNotBeNull(nameof(typeCreationDescription));

            if (deserializedChildValues == null || deserializedChildValues.Count == 0)
            {
                var defaultConstructorDescription = typeCreationDescription.ConstructorDescriptions.FirstOrDefault(c => c.ConstructorParameters.Count == 0);
                if (defaultConstructorDescription == null)
                    throw new DeserializationException($"Could not create instance of type {typeCreationDescription.TargetType.FullName} because there was no JSON data and no default constructor."); // TODO: maybe we can express this a little bit clearer

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
            PerformPropertyAndFieldInjection(typeCreationDescription, deserializedChildValues, newObject);

            // TODO: set values that could not be matched with a constructor parameter, property setter or public field to a dictionary of some kind if possible

            return newObject;
        }

        /// <summary>
        ///     Performs property and field injection on the <paramref name="newObject" /> using the <paramref name="typeCreationDescription" /> and the <paramref name="deserializedChildValues" />.
        /// </summary>
        /// <param name="typeCreationDescription">The object describing how a type can be created with constructor, property and field injection.</param>
        /// <param name="deserializedChildValues">The deserialized child values from the JSON document.</param>
        /// <param name="newObject">The object which will be populated via property and field injection.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeCreationDescription"/> or <paramref name="newObject"/> is null.</exception>
        public static void PerformPropertyAndFieldInjection(TypeCreationDescription typeCreationDescription, Dictionary<InjectableValueDescription, InjectableValue> deserializedChildValues, object newObject)
        {
            if (deserializedChildValues == null)
                return;
            typeCreationDescription.MustNotBeNull(nameof(typeCreationDescription));
            newObject.MustNotBeNull(nameof(newObject));

            foreach (var injectablePropertyInfo in typeCreationDescription.PropertyDescriptions)
            {
                InjectableValue injectableValue;
                if (deserializedChildValues.TryGetValue(injectablePropertyInfo, out injectableValue) == false)
                    continue;

                if (injectableValue.HasBeenInjectedBefore)
                    continue;

                injectablePropertyInfo.SetPropertyValue(newObject, injectableValue.Inject());
                deserializedChildValues[injectablePropertyInfo] = injectableValue;
            }

            foreach (var injectableFieldInfo in typeCreationDescription.FieldDescriptions)
            {
                InjectableValue injectableValue;
                if (deserializedChildValues.TryGetValue(injectableFieldInfo, out injectableValue) == false)
                    continue;

                if (injectableValue.HasBeenInjectedBefore)
                    continue;

                injectableFieldInfo.SetFieldValue(newObject, injectableValue.Inject());
                deserializedChildValues[injectableFieldInfo] = injectableValue;
            }
        }

        private static T TryToInstantiateWithDefaultConstructor<T>(TypeInfo typeInfo) where T : class
        {
            if (typeInfo.IsClass == false || typeInfo.IsAbstract)
                return null;

            var defaultConstructor = typeInfo.DeclaredConstructors.FirstOrDefault(c => c.GetParameters().Length == 0);
            return (T) defaultConstructor?.Invoke(null);
        }

        /// <summary>
        ///     Creates the default <see cref="Dictionary{TKey,TValue}" /> instance.
        /// </summary>
        protected virtual IDictionary CreateDefaultDictionary<TKey, TValue>()
        {
            return new Dictionary<TKey, TValue>();
        }

        /// <summary>
        ///     Creates the default <see cref="List{T}" /> instance.
        /// </summary>
        protected virtual IList CreateDefaultCollection<T>()
        {
            return new List<T>();
        }
    }
}