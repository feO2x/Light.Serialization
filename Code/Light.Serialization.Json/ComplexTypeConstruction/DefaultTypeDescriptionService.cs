using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;
using Light.Serialization.Abstractions;
using Light.Serialization.Json.FrameworkExtensions;

namespace Light.Serialization.Json.ComplexTypeConstruction
{
    /// <summary>
    ///     Represents a Type Description Provider that includes all constructors, settable instance properties, and settable instance fields in a type creation description.
    /// </summary>
    public class DefaultTypeDescriptionService : ITypeDescriptionService
    {
        /// <summary>
        ///     Analyzes the specified type and creates a creation description for it containing information about all constructors
        ///     and settable instance properties and fields.
        /// </summary>
        /// <param name="type">The type to be analyzed.</param>
        /// <returns>The type creation description.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
        public virtual TypeCreationDescription GetTypeCreationDescription(Type type)
        {
            type.MustNotBeNull(nameof(type));

            var typeInfo = type.GetTypeInfo();
            CheckIfTypeIsInstantiatable(typeInfo);

            if (typeInfo.IsAbstract || typeInfo.IsInterface)
                throw new DeserializationException($"The specified type {type.FullName} is abstract and cannot be deserialized");

            var typeCreationDescription = new TypeCreationDescription(type);
            foreach (var constructorInfo in typeInfo.DeclaredConstructors)
            {
                if (constructorInfo.IsStatic || constructorInfo.IsPublic == false)
                    continue;

                var parameterDescriptions = new List<InjectableValueDescription>();
                foreach (var parameterInfo in constructorInfo.GetParameters())
                {
                    var normalizedParameterName = NormalizeName(parameterInfo.Name);

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

            foreach (var propertyInfo in type.GetRuntimeProperties())
            {
                var setMethodInfo = propertyInfo.SetMethod;
                if (setMethodInfo == null || setMethodInfo.IsPublic == false || setMethodInfo.IsStatic)
                    continue;

                var normalizedPropertyName = NormalizeName(propertyInfo.Name);
                var targetDescription = typeCreationDescription.GetInjectableValueDescriptionFromNormalizedName(normalizedPropertyName);
                if (targetDescription == null)
                {
                    targetDescription = InjectableValueDescription.FromProperty(normalizedPropertyName, propertyInfo);
                    typeCreationDescription.AddInjectableValueDescription(targetDescription);
                }
                else
                    targetDescription.AddPropertyName(propertyInfo);
            }

            foreach (var fieldInfo in type.GetRuntimeFields())
            {
                if (fieldInfo.IsStatic || fieldInfo.IsPublic == false || fieldInfo.IsInitOnly)
                    continue;

                var normalizedFieldName = NormalizeName(fieldInfo.Name);
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

        /// <summary>
        ///     Normalizes the given JSON string so that it can be compared to the names of settable type members and constructor parameters (for <see cref="InjectableValueDescription" />).
        /// </summary>
        /// <param name="jsonName">The JSON string to be normalized.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="jsonName" /> is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="jsonName" /> is an empty string.</exception>
        /// <exception cref="StringIsOnlyWhiteSpaceException">Thrown when <paramref name="jsonName" />contains only whitespace.</exception>
        public virtual string NormalizeName(string jsonName)
        {
            return jsonName.ToLowerAndRemoveAllSpecialCharacters();
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckIfTypeIsInstantiatable(TypeInfo typeInfo)
        {
            if (typeInfo.IsAbstract || typeInfo.IsInterface)
                throw new DeserializationException($"The specified type {typeInfo.FullName} is abstract and cannot be deserialized");
        }
    }
}