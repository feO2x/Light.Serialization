using System;
using System.Linq.Expressions;
using System.Reflection;
using Light.GuardClauses;

namespace Light.Serialization.Json.FrameworkExtensions
{
    /// <summary>
    ///     Provides extension methods for <see cref="Expression" /> instances.
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        ///     Extracts the property name of the specified expression.
        /// </summary>
        /// <typeparam name="T">The type whose property name will be extracted.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="expression">The expression where the property name is extracted from. It must be in the form of o => o.Property.</param>
        /// <returns>The name of the property.</returns>
        /// <exception cref="ArgumentException">Thrown when an invalid expression is specified.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="expression" /> is null.</exception>
        public static string ExtractPropertyName<T, TProperty>(this Expression<Func<T, TProperty>> expression)
        {
            expression.MustNotBeNull(nameof(expression));

            var memberExpression = expression.Body as MemberExpression;
            memberExpression.MustNotBeNull(exception: () => new ArgumentException("The specified expression is not valid. Please use an expression like the following one: o => o.Property", nameof(expression)));

            // ReSharper disable once PossibleNullReferenceException
            var propertyInfo = memberExpression.Member as PropertyInfo;
            propertyInfo.MustNotBeNull(exception: () => new ArgumentException("The specified expression is not valid. Please use an expression like the following one: o => o.Property", nameof(expression)));

            // ReSharper disable once PossibleNullReferenceException
            Check.That(propertyInfo.CanRead,
                       () => new ArgumentException($"The specified property {propertyInfo.Name} has no get method and thus cannot be used for serialization.", nameof(expression)));

            return propertyInfo.Name;
        }

        /// <summary>
        ///     Extracts the field name of the specified expression.
        /// </summary>
        /// <typeparam name="T">The type whose field name will be extracted.</typeparam>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="expression">The expression where the property name is extracted from. It must be in the form of o => o.Field.</param>
        /// <returns>The name of the field.</returns>
        /// <exception cref="ArgumentException">Thrown when an invalid expression is specified.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="expression" /> is null.</exception>
        public static string ExtractFieldName<T, TField>(this Expression<Func<T, TField>> expression)
        {
            expression.MustNotBeNull(nameof(expression));

            var memberExpression = expression.Body as MemberExpression;
            memberExpression.MustNotBeNull(exception: () => new ArgumentException("The specified fieldSelector is not valid. Please use an expression like the following one: o => o.Field", nameof(expression)));

            // ReSharper disable once PossibleNullReferenceException
            var fieldInfo = memberExpression.Member as FieldInfo;

            fieldInfo.MustNotBeNull(exception: () => new ArgumentException("The specified fieldSelector is not valid. Please use an expression like the following one: o => o.Field", nameof(expression)));

            // ReSharper disable once PossibleNullReferenceException
            return fieldInfo.Name;
        }
    }
}