using System.Collections.Generic;
using System.Reflection;
using Light.GuardClauses;

namespace Light.Serialization.Json.ComplexTypeConstruction
{
    public sealed class ConstructorDescription
    {
        public readonly ConstructorInfo ConstructorInfo;

        public List<InjectableValueDescription> ConstructorParameters;

        public ConstructorDescription(ConstructorInfo constructorInfo, List<InjectableValueDescription> constructorParameters)
        {
            constructorInfo.MustNotBeNull(nameof(constructorInfo));
            constructorParameters.MustNotBeNull(nameof(constructorParameters));

            ConstructorInfo = constructorInfo;
            ConstructorParameters = constructorParameters;
        }

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

        public object TryCallConstructor(Dictionary<InjectableValueDescription, object> deserializedChildValues)
        {
            if (CanConstructorBeInvoked(deserializedChildValues) == false)
                return null;

            if (ConstructorParameters.Count == 0)
                return ConstructorInfo.Invoke(null);

            var parameters = new object[ConstructorParameters.Count];
            for (var i = 0; i < ConstructorParameters.Count; i++)
            {
                parameters[i] = deserializedChildValues[ConstructorParameters[i]];
            }
            return ConstructorInfo.Invoke(parameters);
        }
    }
}