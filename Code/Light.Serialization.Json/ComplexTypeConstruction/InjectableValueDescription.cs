using System;
using System.Reflection;
using Light.GuardClauses;

namespace Light.Serialization.Json.ComplexTypeConstruction
{
    public sealed class InjectableValueDescription
    {
        public readonly string NormalizedName;
        public readonly Type Type;

        private ParameterInfo _constructorParameterInfo;
        private FieldInfo _fieldInfo;
        private InjectableValueKind _kind;
        private PropertyInfo _propertyInfo;

        private InjectableValueDescription(string normalizedName, Type type)
        {
            normalizedName.MustNotBeNullOrEmpty(nameof(normalizedName));
            type.MustNotBeNull(nameof(type));

            NormalizedName = normalizedName;
            Type = type;
        }

        public ParameterInfo ConstructorParameterInfo => _constructorParameterInfo;
        public PropertyInfo PropertyInfo => _propertyInfo;
        public FieldInfo FieldInfo => _fieldInfo;
        public InjectableValueKind Kind => _kind;

        public void AddConstructorParameter(ParameterInfo parameterInfo)
        {
            parameterInfo.MustNotBeNull(nameof(parameterInfo));
            Check.Against(parameterInfo.Member is ConstructorInfo == false, () => new ArgumentException($"The specified parameterInfo {parameterInfo} does not belong to a constructor."));

            _kind |= InjectableValueKind.ConstructorParameter;
            _constructorParameterInfo = parameterInfo;
        }

        public void AddPropertyName(PropertyInfo propertyInfo)
        {
            // ReSharper disable PossibleNullReferenceException
            Check.Against(propertyInfo.SetMethod == null, () => new ArgumentException($"The specified PropertyInfo {propertyInfo} of type {propertyInfo.DeclaringType} does not have a set method."));
            Check.Against(propertyInfo.SetMethod.IsPublic == false, () => new ArgumentException($"The specified PropertyInfo {propertyInfo} of type {propertyInfo.DeclaringType} has no public set method"));
            Check.Against(propertyInfo.SetMethod.IsStatic, () => new ArgumentException($"The specified PropertyInfo {propertyInfo} of type {propertyInfo.DeclaringType} is static."));
            Check.Against(propertyInfo.PropertyType != Type, () => new ArgumentException($"The specified PropertyInfo {propertyInfo} of type {propertyInfo.DeclaringType} does not have the same type {Type} as this injectable value info."));
            // ReSharper restore PossibleNullReferenceException

            _kind |= InjectableValueKind.PropertySetter;
            _propertyInfo = propertyInfo;
        }

        public void AddFieldInfo(FieldInfo fieldInfo)
        {
            fieldInfo.MustNotBeNull(nameof(fieldInfo));
            Check.Against(fieldInfo.IsStatic, () => new ArgumentException($"The specified FieldInfo {fieldInfo} of type {fieldInfo.DeclaringType} is static."));
            Check.Against(fieldInfo.IsPublic == false, () => new ArgumentException($"The specified FieldInfo {fieldInfo} of type {fieldInfo.DeclaringType} is not public."));
            Check.Against(fieldInfo.FieldType != Type, () => new ArgumentException($"The specified FieldInfo {fieldInfo} of type {fieldInfo.DeclaringType} does not have the same type {Type} as this injectable value info."));

            _kind |= InjectableValueKind.SettableField;
            _fieldInfo = fieldInfo;
        }

        private void AddUnknownValue()
        {
            _kind = InjectableValueKind.UnknownOnTargetObject;
        }

        public static InjectableValueDescription FromConstructorParameter(string normalizedName, ParameterInfo parameterInfo)
        {
            parameterInfo.MustNotBeNull(nameof(parameterInfo));

            var injectableValueInfo = new InjectableValueDescription(normalizedName, parameterInfo.ParameterType);
            injectableValueInfo.AddConstructorParameter(parameterInfo);
            return injectableValueInfo;
        }

        public static InjectableValueDescription FromProperty(string normalizedName, PropertyInfo propertyInfo)
        {
            var injectableValueInfo = new InjectableValueDescription(normalizedName, propertyInfo.PropertyType);
            injectableValueInfo.AddPropertyName(propertyInfo);
            return injectableValueInfo;
        }

        public static InjectableValueDescription FromField(string normalizedName, FieldInfo fieldInfo)
        {
            var injectableValueInfo = new InjectableValueDescription(normalizedName, fieldInfo.FieldType);
            injectableValueInfo.AddFieldInfo(fieldInfo);
            return injectableValueInfo;
        }

        public static InjectableValueDescription FromUnknownValue(string normalizedName, Type type)
        {
            var injectableValueDescription = new InjectableValueDescription(normalizedName, type);
            injectableValueDescription.AddUnknownValue();
            return injectableValueDescription;
        }

        public override string ToString()
        {
            return NormalizedName;
        }

        public void SetFieldValue(object targetObject, object value)
        {
            targetObject.MustNotBeNull(nameof(targetObject));
            Check.Against((_kind & InjectableValueKind.SettableField) != 0,
                          () => new InvalidOperationException($"You try to set a field value on {NormalizedName}, but there is no such field on type {targetObject.GetType().FullName}."));

            _fieldInfo.SetValue(targetObject, value);
        }

        public void SetPropertyValue(object targetObject, object value)
        {
            targetObject.MustNotBeNull(nameof(targetObject));
            Check.Against((_kind & InjectableValueKind.PropertySetter) != 0,
                          () => new InvalidOperationException($"You try to set a property value on {NormalizedName}, but there is no such property on type {targetObject.GetType().FullName}."));

            PropertyInfo.SetValue(targetObject, value);
        }
    }
}