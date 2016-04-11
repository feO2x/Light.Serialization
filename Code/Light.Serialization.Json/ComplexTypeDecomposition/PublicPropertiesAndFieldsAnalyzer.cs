using System;
using System.Collections.Generic;
using System.Reflection;
using Light.GuardClauses;

namespace Light.Serialization.Json.ComplexTypeDecomposition
{
    /// <summary>
    ///     Represents a type analyzer that creates value providers for all public properties and public fields of a certain type.
    /// </summary>
    public sealed class PublicPropertiesAndFieldsAnalyzer : IReadableValuesTypeAnalyzer
    {
        private IValueProviderFactory _valueProviderFactory = new ValueProviderFactoryUsingLambdas();

        /// <summary>
        ///     Gets or sets the factory that is used to create IValueProvider instances. This value defaults to ValueProviderFactoryUsingLambdas.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        public IValueProviderFactory ValueProviderFactory
        {
            get { return _valueProviderFactory; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _valueProviderFactory = value;
            }
        }

        /// <summary>
        ///     Analyzes the specified type and returns value providers describing all public properties and fields.
        /// </summary>
        /// <param name="type">The type to be analyzed.</param>
        /// <returns>The value providers that can be used to read values from all public properties and fields.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
        public IList<IValueProvider> AnalyzeType(Type type)
        {
            type.MustNotBeNull(nameof(type));

            var valueProviders = new List<IValueProvider>();

            // ReSharper disable LoopCanBeConvertedToQuery
            foreach (var propertyInfo in type.GetRuntimeProperties())
            {
                if (propertyInfo.CanRead == false)
                    continue;

                var getMethod = propertyInfo.GetMethod;
                if (getMethod.IsPublic && getMethod.IsStatic == false)
                    valueProviders.Add(_valueProviderFactory.Create(type, propertyInfo));
            }

            foreach (var fieldInfo in type.GetRuntimeFields())
            {
                if (fieldInfo.IsPublic && fieldInfo.IsStatic == false)
                    valueProviders.Add(_valueProviderFactory.Create(type, fieldInfo));
            }
            // ReSharper restore LoopCanBeConvertedToQuery

            return valueProviders;
        }
    }
}