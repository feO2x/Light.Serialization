using System;
using System.Collections.Generic;
using System.Reflection;
using Light.GuardClauses;

namespace Light.Serialization.Json.ComplexTypeConstruction
{
    public sealed class DefaultTypeDescriptionProvider : ITypeDescriptionProvider
    {
        private IInjectableValueNameNormalizer _injectableValueNameNormalizer;

        public DefaultTypeDescriptionProvider(IInjectableValueNameNormalizer injectableValueNameNormalizer)
        {
            injectableValueNameNormalizer.MustNotBeNull(nameof(injectableValueNameNormalizer));

            _injectableValueNameNormalizer = injectableValueNameNormalizer;
        }

        public IInjectableValueNameNormalizer InjectableValueNameNormalizer
        {
            get { return _injectableValueNameNormalizer; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _injectableValueNameNormalizer = value;
            }
        }

        public TypeCreationDescription GetTypeCreationDescription(Type typeToAnalyze)
        {
            typeToAnalyze.MustNotBeNull(nameof(typeToAnalyze));

            var typeInfo = typeToAnalyze.GetTypeInfo();

            if (typeInfo.IsAbstract || typeInfo.IsInterface)
                throw new ArgumentException($"The specified type {typeToAnalyze.FullName} is abstract and cannot be deserialized", nameof(typeToAnalyze));

            var typeCreationDescription = new TypeCreationDescription(typeToAnalyze);
            foreach (var constructorInfo in typeInfo.DeclaredConstructors)
            {
                if (constructorInfo.IsStatic || constructorInfo.IsPublic == false)
                    continue;

                var parameterDescriptions = new List<InjectableValueDescription>();
                foreach (var parameterInfo in constructorInfo.GetParameters())
                {
                    var normalizedParameterName = _injectableValueNameNormalizer.Normalize(parameterInfo.Name);

                    var parameterDescription = typeCreationDescription.GetInjectableValueDescriptionFromNormalizedName(normalizedParameterName);
                    if (parameterDescription == null)
                    {
                        parameterDescription = InjectableValueDescription.FromConstructorParameter(normalizedParameterName, parameterInfo);
                        typeCreationDescription.AddInjectableValueDescription(parameterDescription);
                    }
                    parameterDescriptions.Add(parameterDescription);
                }
                typeCreationDescription.AddConstructorDescription(new ConstructorDescription(constructorInfo, parameterDescriptions));
            }

            foreach (var propertyInfo in typeToAnalyze.GetRuntimeProperties())
            {
                var setMethodInfo = propertyInfo.SetMethod;
                if (setMethodInfo == null || setMethodInfo.IsPublic == false || setMethodInfo.IsStatic)
                    continue;

                var normalizedPropertyName = _injectableValueNameNormalizer.Normalize(propertyInfo.Name);
                var targetDescription = typeCreationDescription.GetInjectableValueDescriptionFromNormalizedName(normalizedPropertyName);
                if (targetDescription == null)
                {
                    targetDescription = InjectableValueDescription.FromProperty(normalizedPropertyName, propertyInfo);
                    typeCreationDescription.AddInjectableValueDescription(targetDescription);
                }
                else
                    targetDescription.AddPropertyName(propertyInfo);
            }

            foreach (var fieldInfo in typeToAnalyze.GetRuntimeFields())
            {
                if (fieldInfo.IsStatic || fieldInfo.IsPublic == false || fieldInfo.IsInitOnly)
                    continue;

                var normalizedFieldName = _injectableValueNameNormalizer.Normalize(fieldInfo.Name);
                var targetDescription = typeCreationDescription.GetInjectableValueDescriptionFromNormalizedName(normalizedFieldName);
                if (targetDescription == null)
                {
                    targetDescription = InjectableValueDescription.FromField(normalizedFieldName, fieldInfo);
                    typeCreationDescription.AddInjectableValueDescription(targetDescription);
                }
                else
                    targetDescription.AddFieldInfo(fieldInfo);
            }

            return typeCreationDescription;
        }
    }
}