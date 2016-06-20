using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Light.GuardClauses;
using Light.Serialization.Json.ComplexTypeDecomposition;
using Light.Serialization.Json.FrameworkExtensions;

namespace Light.Serialization.Json.SerializationRules
{
    /// <summary>
    ///     Represents a non-generic rule. This base class exists so that rules can be stored in a context where no generic is needed.
    /// </summary>
    public abstract class Rule : IEquatable<Rule>
    {
        /// <summary>
        ///     Gets the type this rule is created for.
        /// </summary>
        public readonly Type TargetType;


        /// <summary>
        ///     Creates a new instance of Rule.
        /// </summary>
        /// <param name="targetType">The type this rule is created for.</param>
        protected Rule(Type targetType)
        {
            targetType.MustNotBeNull(nameof(targetType));

            TargetType = targetType;
        }

        /// <summary>
        ///     Returns true if the other rule targets the same type as this instance.
        /// </summary>
        /// <param name="other">The other rule to be compared.</param>
        /// <returns>True if the other rule has the same target type, else false.</returns>
        public bool Equals(Rule other)
        {
            if (other == null)
                return false;

            return TargetType == other.TargetType;
        }

        /// <summary>
        ///     Returns true if the other object is a rule that targets the same type as this instance.
        /// </summary>
        /// <param name="obj">The object to be compared.</param>
        /// <returns>True if the other object is a rule that has the same target type, else false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Rule);
        }

        /// <summary>
        ///     Returns the hash code of the target type.
        /// </summary>
        public override int GetHashCode()
        {
            return TargetType.GetHashCode();
        }

        /// <summary>
        ///     Creates the value readers that are able to read all values specified in this rule.
        /// </summary>
        public abstract List<IValueReader> CreateValueReaders();
    }

    /// <summary>
    ///     Represents a serialization rule for the specified type.
    /// </summary>
    /// <typeparam name="T">The type that should be customized for serialization.</typeparam>
    public sealed class Rule<T> : Rule, IButWhiteListRule<T>, IAndWhiteListRule<T>, IAndBlackListRule<T>
    {
        private readonly List<string> _targetMembersToSerialize = new List<string>();
        private readonly IReadableValuesTypeAnalyzer _typeAnalyzer;

        /// <summary>
        ///     Creates a new instance of Rule of T.
        /// </summary>
        /// <param name="typeAnalyzer">The type analyzer used to create the value readers when calling <see cref="CreateValueReaders" />.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeAnalyzer" /> is null.</exception>
        public Rule(IReadableValuesTypeAnalyzer typeAnalyzer) : base(typeof(T))
        {
            typeAnalyzer.MustNotBeNull(nameof(typeAnalyzer));

            _typeAnalyzer = typeAnalyzer;
            DeterminePublicPropertiesAndFields();
        }

        /// <summary>
        ///     Configures the rule so that the specified property is not serialized.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property that should be ignored.</typeparam>
        /// <param name="propertyExpression">The lambda expression specifying the property. This should be of form o => o.PropertyName;</param>
        /// <returns>The rule for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyExpression" /> is null.</exception>
        public IAndBlackListRule<T> IgnoreProperty<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            propertyExpression.MustNotBeNull(nameof(propertyExpression));

            var propertyName = propertyExpression.ExtractPropertyName();
            _targetMembersToSerialize.Remove(propertyName);

            return this;
        }

        /// <summary>
        ///     Configures the rule so that the specified field is not serialized.
        /// </summary>
        /// <typeparam name="TField">The type of the field that should be ignored.</typeparam>
        /// <param name="fieldExpression">The lambda expression specifying the field. This should be of form o => o.FieldName;</param>
        /// <returns>The rule for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fieldExpression" /> is null.</exception>
        public IAndBlackListRule<T> IgnoreField<TField>(Expression<Func<T, TField>> fieldExpression)
        {
            fieldExpression.MustNotBeNull(nameof(fieldExpression));

            var fieldName = fieldExpression.ExtractFieldName();
            _targetMembersToSerialize.Remove(fieldName);

            return this;
        }

        /// <summary>
        ///     Configures the rule so that the specified property is included in the serialization process.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property that should be included.</typeparam>
        /// <param name="propertyExpression">The lambda expression specifying the property. This should be of form o => o.PropertyName;</param>
        /// <returns>The rule for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyExpression" /> is null.</exception>
        IAndWhiteListRule<T> IAndWhiteListRule<T>.AndProperty<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            AddProperty(propertyExpression);
            return this;
        }

        /// <summary>
        ///     Configures the rule so that the specified field is included in the serialization process.
        /// </summary>
        /// <typeparam name="TField">The type of the field that should be included.</typeparam>
        /// <param name="fieldExpression">The lambda expression specifying the field. This should be of form o => o.FieldName;</param>
        /// <returns>The rule for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fieldExpression" /> is null.</exception>
        IAndWhiteListRule<T> IAndWhiteListRule<T>.AndField<TField>(Expression<Func<T, TField>> fieldExpression)
        {
            AddField(fieldExpression);
            return this;
        }

        /// <summary>
        ///     Configures the rule so that the specified property is included in the serialization process.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property that should be included.</typeparam>
        /// <param name="propertyExpression">The lambda expression specifying the property. This should be of form o => o.PropertyName;</param>
        /// <returns>The rule for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyExpression" /> is null.</exception>
        IAndWhiteListRule<T> IButWhiteListRule<T>.ButProperty<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            AddProperty(propertyExpression);
            return this;
        }

        /// <summary>
        ///     Configures the rule so that the specified field is included in the serialization process.
        /// </summary>
        /// <typeparam name="TField">The type of the field that should be included.</typeparam>
        /// <param name="fieldExpression">The lambda expression specifying the field. This should be of form o => o.FieldName;</param>
        /// <returns>The rule for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fieldExpression" /> is null.</exception>
        IAndWhiteListRule<T> IButWhiteListRule<T>.ButField<TField>(Expression<Func<T, TField>> fieldExpression)
        {
            AddField(fieldExpression);
            return this;
        }

        /// <summary>
        ///     Configures the rule so that all members of the target type will be ignored on serialization. You have to explicitly configure the rule which members should be included after you called this method.
        /// </summary>
        /// <returns>The rule for method chaining.</returns>
        public IButWhiteListRule<T> IgnoreAll()
        {
            _targetMembersToSerialize.Clear();
            return this;
        }

        private void DeterminePublicPropertiesAndFields()
        {
            foreach (var runtimeProperty in TargetType.GetRuntimeProperties())
            {
                var getMethod = runtimeProperty.GetMethod;
                if (getMethod == null || getMethod.IsPublic == false || getMethod.IsStatic)
                    continue;

                _targetMembersToSerialize.Add(runtimeProperty.Name);
            }

            foreach (var runtimeField in TargetType.GetRuntimeFields())
            {
                if (runtimeField.IsStatic || runtimeField.IsPublic == false)
                    continue;

                _targetMembersToSerialize.Add(runtimeField.Name);
            }
        }

        private void AddProperty<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            propertyExpression.MustNotBeNull(nameof(propertyExpression));

            var propertyName = propertyExpression.ExtractPropertyName();
            _targetMembersToSerialize.Add(propertyName);
        }

        private void AddField<TField>(Expression<Func<T, TField>> fieldExpression)
        {
            fieldExpression.MustNotBeNull(nameof(fieldExpression));

            var fieldName = fieldExpression.ExtractFieldName();
            _targetMembersToSerialize.Add(fieldName);
        }

        /// <summary>
        ///     Creates and filters the value readers according to this serialization rule.
        /// </summary>
        public override List<IValueReader> CreateValueReaders()
        {
            var valueReaders = _typeAnalyzer.AnalyzeType(TargetType);

            var i = 0;
            while (i < valueReaders.Count)
            {
                if (_targetMembersToSerialize.Contains(valueReaders[i].Name) == false)
                {
                    valueReaders.RemoveAt(i);
                    continue;
                }
                i++;
            }

            return valueReaders;
        }
    }
}