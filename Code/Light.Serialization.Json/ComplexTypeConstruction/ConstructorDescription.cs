using System;
using System.Collections.Generic;
using System.Reflection;
using Light.GuardClauses;

namespace Light.Serialization.Json.ComplexTypeConstruction
{
    /// <summary>
    ///     Describes a constructor that can be invoked using Injectable Value Descriptions.
    /// </summary>
    public sealed class ConstructorDescription
    {
        /// <summary>
        ///     Gets the described constructor info.
        /// </summary>
        public readonly ConstructorInfo ConstructorInfo;

        /// <summary>
        ///     Gets the list of constructor parameters as injectable value descriptions.
        /// </summary>
        public List<InjectableValueDescription> ConstructorParameters;

        /// <summary>
        ///     Creates a new instance of ConstructorDescription.
        /// </summary>
        /// <param name="constructorInfo">The described constructor info.</param>
        /// <param name="constructorParameters">The parameter infos as injectable value descriptions.</param>
        public ConstructorDescription(ConstructorInfo constructorInfo, List<InjectableValueDescription> constructorParameters)
        {
            constructorInfo.MustNotBeNull(nameof(constructorInfo));
            constructorParameters.MustNotBeNull(nameof(constructorParameters));

            ConstructorInfo = constructorInfo;
            ConstructorParameters = constructorParameters;
        }

        /// <summary>
        ///     Checks if the constructor described by this instance can be invoked with the given values.
        /// </summary>
        /// <param name="deserializedChildValues">The values that can be used to call the constructor.</param>
        /// <returns>True if the call to the constructor would succeed, else false</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="deserializedChildValues" /> is null.</exception>
        public bool CanConstructorBeInvoked(Dictionary<InjectableValueDescription, object> deserializedChildValues)
        {
            deserializedChildValues.MustNotBeNull(nameof(deserializedChildValues));

            if (ConstructorParameters.Count == 0)
                return true;

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var injectableValueDescription in ConstructorParameters)
            {
                if (deserializedChildValues.ContainsKey(injectableValueDescription) == false)
                    return false;
            }
            return true;
        }

        /// <summary>
        ///     Tries to call the constructor with the given values.
        /// </summary>
        /// <param name="deserializedChildValues">The values that can be used to call the constructor.</param>
        /// <returns>The instantiated object or null, when constructor invocation would not be possible.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="deserializedChildValues" /> is null.</exception>
        public object TryCallConstructor(Dictionary<InjectableValueDescription, object> deserializedChildValues)
        {
            if (ConstructorParameters.Count == 0)
                return ConstructorInfo.Invoke(null);

            if (CanConstructorBeInvoked(deserializedChildValues) == false)
                return null;

            var parameters = new object[ConstructorParameters.Count];
            for (var i = 0; i < ConstructorParameters.Count; i++)
            {
                parameters[i] = deserializedChildValues[ConstructorParameters[i]];
            }
            return ConstructorInfo.Invoke(parameters);
        }
    }
}