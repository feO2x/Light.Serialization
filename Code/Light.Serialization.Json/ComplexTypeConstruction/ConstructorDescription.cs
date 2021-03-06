﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Light.GuardClauses;

namespace Light.Serialization.Json.ComplexTypeConstruction
{
    /// <summary>
    ///     Describes a constructor that can be invoked using <see cref="InjectableValue" /> instances.
    /// </summary>
    public sealed class ConstructorDescription
    {
        /// <summary>
        ///     Gets the described constructor info.
        /// </summary>
        public readonly ConstructorInfo ConstructorInfo;

        /// <summary>
        ///     Gets the list of constructor parameters as instances of <see cref="InjectableValueDescription" />.
        /// </summary>
        public readonly List<InjectableValueDescription> ConstructorParameters;

        /// <summary>
        ///     Creates a new instance of <see cref="ConstructorDescription" />.
        /// </summary>
        /// <param name="constructorInfo">The described constructor info.</param>
        /// <param name="constructorParameters">The parameter infos as instances of <see cref="InjectableValueDescription" />.</param>
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
        public bool CanConstructorBeInvoked(Dictionary<InjectableValueDescription, InjectableValue> deserializedChildValues)
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
        public object TryCallConstructor(Dictionary<InjectableValueDescription, InjectableValue> deserializedChildValues)
        {
            if (ConstructorParameters.Count == 0)
                return ConstructorInfo.Invoke(null);

            if (CanConstructorBeInvoked(deserializedChildValues) == false)
                return null;

            var parameters = new object[ConstructorParameters.Count];

            for (var i = 0; i < ConstructorParameters.Count; i++)
            {
                var injectableValue = deserializedChildValues[ConstructorParameters[i]];
                parameters[i] = injectableValue.Inject();
                deserializedChildValues[ConstructorParameters[i]] = injectableValue;
            }
            return ConstructorInfo.Invoke(parameters);
        }
    }
}