using System;
using System.Collections;
using System.Collections.Generic;

namespace Light.Serialization.Json.ComplexTypeConstruction
{
    /// <summary>
    ///     Represents the abstraction of a factory that can create dictionaries, collections or complex objects for a given <see cref="Type" /> instance.
    /// </summary>
    public interface IMetaFactory
    {
        /// <summary>
        ///     Creates a dictionary for the given dictionary type.
        /// </summary>
        IDictionary CreateDictionary(Type dictionaryTypeToConstruct);

        /// <summary>
        ///     Creates a collection for the given collection type.
        /// </summary>
        IList CreateCollection(Type collectionTypeToConstruct);

        /// <summary>
        ///     Creates a complex object using the given <see cref="TypeCreationDescription" />, injecting all values given in the <paramref name="deserializedChildValues" /> dictionary.
        /// </summary>
        object CreateObject(TypeCreationDescription typeCreationDescription, Dictionary<InjectableValueDescription, InjectableValue> deserializedChildValues);
    }
}