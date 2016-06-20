using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;
using Light.Serialization.Abstractions;
using Light.Serialization.Json.FrameworkExtensions;

namespace Light.Serialization.Json.ComplexTypeConstruction
{
    /// <summary>
    ///     Represents the default <see cref="ITypeDescriptionService" /> that includes all constructors, settable instance properties, and settable instance fields in a <see cref="TypeCreationDescription" />.
    /// </summary>
    public class DefaultTypeDescriptionService : ITypeDescriptionService
    {
        /// <summary>
        ///     Analyzes the specified type and creates a <see cref="TypeCreationDescription" /> for it containing information about all constructors
        ///     and settable instance properties and fields.
        /// </summary>
        /// <param name="type">The type to be analyzed.</param>
        /// <returns>The type creation description.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
        public virtual TypeCreationDescription GetTypeCreationDescription(Type type)
        {
            type.MustNotBeNull(nameof(type));

            // Check if the target type can be instantiated
            var typeInfo = type.GetTypeInfo();
            CheckIfTypeIsInstantiatable(typeInfo);

            var injectableValueDescriptions = new List<InjectableValueDescription>();
            var constructorDescriptions = new List<ConstructorDescription>();

            // Check the constructors of the type and produce constructor descriptions out of them
            // TODO: types might not solely be constructed via one of their constructors, but maybe via static methods or in other ways
            foreach (var constructorInfo in typeInfo.DeclaredConstructors)
            {
                if (constructorInfo.IsStatic || constructorInfo.IsPublic == false)
                    continue;

                var parameterDescriptions = new List<InjectableValueDescription>();
                foreach (var parameterInfo in constructorInfo.GetParameters())
                {
                    var normalizedParameterName = NormalizeName(parameterInfo.Name);

                    var parameterDescription = injectableValueDescriptions.FirstOrDefault(ivd => ivd.NormalizedName == normalizedParameterName);
                    if (parameterDescription == null)
                    {
                        parameterDescription = InjectableValueDescription.FromConstructorParameter(normalizedParameterName, parameterInfo);
                        injectableValueDescriptions.Add(parameterDescription);
                    }
                    parameterDescriptions.Add(parameterDescription);
                }
                constructorDescriptions.Add(new ConstructorDescription(constructorInfo, parameterDescriptions));
            }

            CheckIfTypeHasConstructor(constructorDescriptions, type);

            // Check the properties of the target type
            foreach (var propertyInfo in type.GetRuntimeProperties())
            {
                var setMethodInfo = propertyInfo.SetMethod;
                if (setMethodInfo == null || setMethodInfo.IsPublic == false || setMethodInfo.IsStatic)
                    continue;

                var normalizedPropertyName = NormalizeName(propertyInfo.Name);
                var targetDescription = injectableValueDescriptions.FirstOrDefault(ivd => ivd.NormalizedName == normalizedPropertyName);
                if (targetDescription == null)
                {
                    targetDescription = InjectableValueDescription.FromProperty(normalizedPropertyName, propertyInfo);
                    injectableValueDescriptions.Add(targetDescription);
                }
                else
                    targetDescription.AddPropertyName(propertyInfo);
            }

            // Check the field infos of the target type
            foreach (var fieldInfo in type.GetRuntimeFields())
            {
                if (fieldInfo.IsStatic || fieldInfo.IsPublic == false || fieldInfo.IsInitOnly)
                    continue;

                var normalizedFieldName = NormalizeName(fieldInfo.Name);
                var targetDescription = injectableValueDescriptions.FirstOrDefault(ivd => ivd.NormalizedName == normalizedFieldName);
                if (targetDescription == null)
                {
                    targetDescription = InjectableValueDescription.FromField(normalizedFieldName, fieldInfo);
                    injectableValueDescriptions.Add(targetDescription);
                }
                else
                    targetDescription.AddFieldInfo(fieldInfo);
            }

            return new TypeCreationDescription(type, constructorDescriptions, injectableValueDescriptions);
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
                throw new DeserializationException($"The specified type {typeInfo.FullName} is abstract and cannot be deserialized.");
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckIfTypeHasConstructor(List<ConstructorDescription> constructorDescriptions, Type type)
        {
            if (constructorDescriptions.Count == 0)
                throw new DeserializationException($"The specified type {type.FullName} does not have a public non-static constructor that can be called by the deserializer.");
        }
    }
}