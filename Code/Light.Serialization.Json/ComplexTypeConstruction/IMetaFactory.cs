using System;
using System.Collections;
using System.Collections.Generic;

namespace Light.Serialization.Json.ComplexTypeConstruction
{
    /// <summary>
    ///     Represents the abstraction of a factory that can create dictionaries, collections or complex objects for a given type.
    /// </summary>
    public interface IMetaFactory
    {
        /// <summary>
        ///     Creates a dictionary for the given dictionary type.
        /// </summary>
        IDictionary CreateDictionary(Type requestedDictionaryType);

        /// <summary>
        ///     Creates a collection for the given collection type.
        /// </summary>
        IList CreateCollection(Type requestedCollectionType);

        /// <summary>
        ///     Creates a complex object using the given typeCreationDescription, injecting all values given in the deserializedChildValues.
        /// </summary>
        object Create(TypeCreationDescription typeCreationDescription, Dictionary<InjectableValueDescription, object> deserializedChildValues);
    }
}