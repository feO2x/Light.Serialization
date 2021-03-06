﻿using System;
using System.Diagnostics;
using System.Reflection;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;

namespace Light.Serialization.Json.ComplexTypeConstruction
{
    /// <summary>
    ///     Represents an object that describes how a value can be injected into a certain type (via constructor, property, or field injection).
    /// </summary>
    public sealed class InjectableValueDescription
    {
        /// <summary>
        ///     Gets the value indicating whether the deserialized value is not a member of the target type.
        /// </summary>
        public readonly bool IsUnknownOnTargetType;

        /// <summary>
        ///     Gets the normalized name that uniquely identifies the injectable value.
        /// </summary>
        public readonly string NormalizedName;

        /// <summary>
        ///     Gets the type of the injectable value.
        /// </summary>
        public readonly Type Type;

        private ParameterInfo _constructorParameterInfo;
        private FieldInfo _fieldInfo;
        private PropertyInfo _propertyInfo;

        private InjectableValueDescription(string normalizedName, Type type, bool isUnknownOnTargetType = false)
        {
            normalizedName.MustNotBeNullOrEmpty(nameof(normalizedName));
            type.MustNotBeNull(nameof(type));

            NormalizedName = normalizedName;
            Type = type;
            IsUnknownOnTargetType = isUnknownOnTargetType;
        }


        /// <summary>
        ///     Gets the parameter info when the injectable value can be passed in via constructor injection.
        /// </summary>
        public ParameterInfo ConstructorParameterInfo => _constructorParameterInfo;

        /// <summary>
        ///     Gets the property info that can be used to set the injectable value via property injection.
        /// </summary>
        public PropertyInfo PropertyInfo => _propertyInfo;

        /// <summary>
        ///     Gets the field info that can be used to set the injectable value via field injection.
        /// </summary>
        public FieldInfo FieldInfo => _fieldInfo;

        /// <summary>
        ///     Adds a constructor parameter info to this injectable value description.
        /// </summary>
        /// <param name="parameterInfo">The info that describes how the value can be injected via constructor injection.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="parameterInfo" /> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="parameterInfo" /> does not describe a parameter of a constructor.</exception>
        public void AddConstructorParameter(ParameterInfo parameterInfo)
        {
            CheckParameterInfo(parameterInfo);

            _constructorParameterInfo = parameterInfo;
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private void CheckParameterInfo(ParameterInfo parameterInfo)
        {
            parameterInfo.MustNotBeNull(nameof(parameterInfo));
            if (parameterInfo.Member is ConstructorInfo == false)
                throw new ArgumentException($"The specified parameterInfo {parameterInfo} does not belong to a constructor.");
            if (parameterInfo.ParameterType != Type)
                throw new ArgumentException($"The specified paramterInfo {parameterInfo} does not have the same type as this injectable value description.");
        }

        /// <summary>
        ///     Adds the specified property info to this injectable value description as a means of property injection.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyInfo" /> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="propertyInfo" /> does not describe a public instance property with a set method and the same type as this injectable value description.</exception>
        public void AddPropertyName(PropertyInfo propertyInfo)
        {
            CheckPropertyInfo(propertyInfo);

            _propertyInfo = propertyInfo;
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private void CheckPropertyInfo(PropertyInfo propertyInfo)
        {
            propertyInfo.MustNotBeNull(nameof(propertyInfo));
            if (propertyInfo.SetMethod == null)
                throw new ArgumentException($"The specified PropertyInfo {propertyInfo} of type {propertyInfo.DeclaringType} does not have a set method.");
            if (propertyInfo.SetMethod.IsPublic == false)
                throw new ArgumentException($"The specified PropertyInfo {propertyInfo} of type {propertyInfo.DeclaringType} has no public set method");
            if (propertyInfo.SetMethod.IsStatic)
                throw new ArgumentException($"The specified PropertyInfo {propertyInfo} of type {propertyInfo.DeclaringType} is static.");
            if (propertyInfo.PropertyType != Type)
                throw new ArgumentException($"The specified PropertyInfo {propertyInfo} of type {propertyInfo.DeclaringType} does not have the same type {Type} as this injectable value description.");
        }

        /// <summary>
        ///     Adds the specified field info to this injectable value description as a means of field injection.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fieldInfo" /> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="fieldInfo" /> does not describe a public instance field with the same type as this injectable value description.</exception>
        public void AddFieldInfo(FieldInfo fieldInfo)
        {
            CheckFieldInfo(fieldInfo);

            _fieldInfo = fieldInfo;
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private void CheckFieldInfo(FieldInfo fieldInfo)
        {
            fieldInfo.MustNotBeNull(nameof(fieldInfo));
            if (fieldInfo.IsStatic)
                throw new ArgumentException($"The specified FieldInfo {fieldInfo} of type {fieldInfo.DeclaringType} is static.");
            if (fieldInfo.IsPublic == false)
                throw new ArgumentException($"The specified FieldInfo {fieldInfo} of type {fieldInfo.DeclaringType} is not public.");
            if (fieldInfo.IsInitOnly)
                throw new ArgumentException($"The specified FieldInfo {fieldInfo} of type {fieldInfo.DeclaringType} is read-only and cannot be used for field injection.");
            if (fieldInfo.FieldType != Type)
                throw new ArgumentException($"The specified FieldInfo {fieldInfo} of type {fieldInfo.DeclaringType} does not have the same type {Type} as this injectable value description.");
        }

        /// <summary>
        ///     Creates a new instance of InjectableValueDescription from a constructor parameter.
        /// </summary>
        /// <param name="normalizedName">The normalized name of the injectable value.</param>
        /// <param name="parameterInfo">The parameter info describing a constructor parameter for this injectable value.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown when parameterInfo is not a constructor parameter.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="normalizedName" />is an empty string.</exception>
        public static InjectableValueDescription FromConstructorParameter(string normalizedName, ParameterInfo parameterInfo)
        {
            var injectableValueInfo = new InjectableValueDescription(normalizedName, parameterInfo.ParameterType);
            injectableValueInfo.AddConstructorParameter(parameterInfo);
            return injectableValueInfo;
        }

        /// <summary>
        ///     Creates a new instance of InjectableValueDescription from a property info.
        /// </summary>
        /// <param name="normalizedName">The normalized name of the injectable value.</param>
        /// <param name="propertyInfo">The property info describing a settable public instance property for this injectable value.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="propertyInfo" /> does not describe a public instance property with a setter.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="normalizedName" />is an empty string.</exception>
        public static InjectableValueDescription FromProperty(string normalizedName, PropertyInfo propertyInfo)
        {
            var injectableValueInfo = new InjectableValueDescription(normalizedName, propertyInfo.PropertyType);
            injectableValueInfo.AddPropertyName(propertyInfo);
            return injectableValueInfo;
        }

        /// <summary>
        ///     Creates a new instance of InjectableValueDescription from a field info.
        /// </summary>
        /// <param name="normalizedName">The normalized name of the injectable value.</param>
        /// <param name="fieldInfo">The field info describing a public instance field that is not read-only.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="fieldInfo" /> does not describe a public instance field that is not read-only.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="normalizedName" />is an empty string.</exception>
        public static InjectableValueDescription FromField(string normalizedName, FieldInfo fieldInfo)
        {
            var injectableValueInfo = new InjectableValueDescription(normalizedName, fieldInfo.FieldType);
            injectableValueInfo.AddFieldInfo(fieldInfo);
            return injectableValueInfo;
        }

        /// <summary>
        ///     Creates a new instance of InjectableValueDescription for an unknown member or parameter.
        ///     Use this if there is no member or parameter that can be used to inject the value on the target type.
        /// </summary>
        /// <param name="normalizedName">The normalized name of the injectable value.</param>
        /// <param name="type">The type of the injectable value.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="normalizedName" />is an empty string.</exception>
        public static InjectableValueDescription FromUnknownValue(string normalizedName, Type type)
        {
            return new InjectableValueDescription(normalizedName, type, true);
        }

        /// <summary>
        ///     Returns the normalized name for this Injectable Value Description.
        /// </summary>
        public override string ToString()
        {
            return NormalizedName;
        }

        /// <summary>
        ///     Sets a value on the given object using the field info that is associated with this injectable value description.
        /// </summary>
        /// <param name="targetObject">The object where the value is set on.</param>
        /// <param name="value">The value that should be set on the target field.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="targetObject" /> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when this injectable value description is not associated with a field info.</exception>
        public void SetFieldValue(object targetObject, object value)
        {
            targetObject.MustNotBeNull(nameof(targetObject));
            CheckIfValueIsInjectableThroughField(targetObject);

            _fieldInfo.SetValue(targetObject, value);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private void CheckIfValueIsInjectableThroughField(object targetObject)
        {
            if (_fieldInfo == null)
                throw new InvalidOperationException($"You try to set a field value on {NormalizedName}, but there is no such field on type {targetObject.GetType().FullName}.");
        }

        /// <summary>
        ///     Sets the value on the given object using the property info that is associated with this injectable value description.
        /// </summary>
        /// <param name="targetObject">The object where the value is set on.</param>
        /// <param name="value">The value that should be set on the target property.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="targetObject" /> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when this injectable value description is not associated with a property info.</exception>
        public void SetPropertyValue(object targetObject, object value)
        {
            targetObject.MustNotBeNull(nameof(targetObject));
            CheckIfValueIsInjectableThroughProperty(targetObject);

            _propertyInfo.SetValue(targetObject, value);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private void CheckIfValueIsInjectableThroughProperty(object targetObject)
        {
            if (_propertyInfo == null)
                throw new InvalidOperationException($"You try to set a property value on {NormalizedName}, but there is no such property on type {targetObject.GetType().FullName}.");
        }

        /// <summary>
        ///     Sets the value on the given object using either the property info or the field info that is associated with this injectable value description.
        /// </summary>
        /// <param name="targetObject">The object the value should be set on.</param>
        /// <param name="value">The value to be set.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="targetObject" /> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when this injectable value description is not associated with a property info or field info.</exception>
        public void SetPropertyOrField(object targetObject, object value)
        {
            if (_propertyInfo != null)
                _propertyInfo.SetValue(targetObject, value);
            else if (_fieldInfo != null)
                _fieldInfo.SetValue(targetObject, value);
            else
                throw new InvalidOperationException($"You try to set a property or field value on {NormalizedName}, but there is no possiblity to do so on type {targetObject.GetType().FullName}.");
        }
    }
}