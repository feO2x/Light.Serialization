using System;
using System.Linq.Expressions;
using System.Reflection;
using Light.GuardClauses;

namespace Light.Serialization.Json.ComplexTypeDecomposition
{
    /// <summary>
    ///     Represents a value provider factory that dynamically creates Expression trees and compiles them for ValueProviderUsingLambda instances to read values from properties or fields.
    /// </summary>
    public sealed class ValueProviderFactoryUsingLambdas : IValueProviderFactory
    {
        /// <summary>
        ///     Creates a ValueProviderUsingLambda for the specified property info.
        /// </summary>
        /// <param name="targetType">The type that has the specified property as a member.</param>
        /// <param name="propertyInfo">The info describing the property.</param>
        /// <returns>The <see cref="ValueProviderUsingLambda" /> instance that can be used to read the value from the specified property.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public IValueProvider Create(Type targetType, PropertyInfo propertyInfo)
        {
            targetType.MustNotBeNull(nameof(targetType));
            propertyInfo.MustNotBeNull(nameof(propertyInfo));

            var parameterExpression = Expression.Parameter(typeof (object));

            Expression bodyExpression = Expression.Property(Expression.ConvertChecked(parameterExpression, targetType), propertyInfo);

            bodyExpression = CheckForPossiblyNeededValueTypeConversion(bodyExpression, propertyInfo.PropertyType.GetTypeInfo());

            var lambda = Expression.Lambda<Func<object, object>>(bodyExpression, parameterExpression).Compile();

            return new ValueProviderUsingLambda(propertyInfo.Name, lambda, propertyInfo.PropertyType);
        }

        /// <summary>
        ///     Creates a ValueProviderUsingLambda for the specified property info.
        /// </summary>
        /// <param name="targetType">The type that has the specified field as a member.</param>
        /// <param name="fieldInfo">The info describing the field.</param>
        /// <returns>The <see cref="ValueProviderUsingLambda" /> instance that can be used to read the value from the specified field.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public IValueProvider Create(Type targetType, FieldInfo fieldInfo)
        {
            targetType.MustNotBeNull(nameof(targetType));
            fieldInfo.MustNotBeNull(nameof(fieldInfo));

            var parameterExpression = Expression.Parameter(typeof (object));

            Expression bodyExpression = Expression.Field(Expression.ConvertChecked(parameterExpression, targetType), fieldInfo);

            bodyExpression = CheckForPossiblyNeededValueTypeConversion(bodyExpression, fieldInfo.FieldType.GetTypeInfo());

            var lambda = Expression.Lambda<Func<object, object>>(bodyExpression, parameterExpression).Compile();

            return new ValueProviderUsingLambda(fieldInfo.Name, lambda, fieldInfo.FieldType);
        }

        private static Expression CheckForPossiblyNeededValueTypeConversion(Expression bodyExpression, TypeInfo returnedSubType)
        {
            return returnedSubType.IsValueType ? Expression.Convert(bodyExpression, typeof (object)) : bodyExpression;
        }
    }
}