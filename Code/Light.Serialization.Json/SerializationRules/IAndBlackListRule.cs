using System;
using System.Linq.Expressions;

namespace Light.Serialization.Json.SerializationRules
{
    /// <summary>
    ///     Represents the part of a serialization rule that can be called after the first member was blacklisted.
    /// </summary>
    /// <typeparam name="T">The type that is configured for serialization.</typeparam>
    public interface IAndBlackListRule<T>
    {
        /// <summary>
        ///     Configures the rule so that the specified property is not serialized.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property that should be ignored.</typeparam>
        /// <param name="propertyExpression">The lambda expression specifying the property. This should be of form o => o.PropertyName;</param>
        /// <returns>The rule for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyExpression" /> is null.</exception>
        IAndBlackListRule<T> IgnoreProperty<TProperty>(Expression<Func<T, TProperty>> propertyExpression);

        /// <summary>
        ///     Configures the rule so that the specified field is not serialized.
        /// </summary>
        /// <typeparam name="TField">The type of the field that should be ignored.</typeparam>
        /// <param name="fieldExpression">The lambda expression specifying the field. This should be of form o => o.FieldName;</param>
        /// <returns>The rule for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fieldExpression" /> is null.</exception>
        IAndBlackListRule<T> IgnoreField<TField>(Expression<Func<T, TField>> fieldExpression);
    }
}