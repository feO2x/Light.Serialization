using System;
using System.Collections.Generic;
using System.Reflection;
using Light.GuardClauses;

namespace Light.Serialization.Json.ComplexTypeDecomposition
{
    /// <summary>
    ///     Represents a type analyzer that creates <see cref="IValueReader" /> instances for all public properties and public fields of a certain type.
    /// </summary>
    public sealed class PublicPropertiesAndFieldsAnalyzer : IReadableValuesTypeAnalyzer
    {
        private IValueReaderFactory _valueReaderFactory = new ValueReaderFactoryUsingLambdas();

        /// <summary>
        ///     Gets or sets the factory that is used to create <see cref="IValueReader" /> instances.
        ///     This value defaults to an instance of <see cref="ValueReaderFactoryUsingLambdas" />.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        public IValueReaderFactory ValueReaderFactory
        {
            get { return _valueReaderFactory; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _valueReaderFactory = value;
            }
        }

        /// <summary>
        ///     Analyzes the specified type and returns <see cref="IValueReader" /> instances for each property and field that is public and non-static.
        /// </summary>
        /// <param name="type">The type to be analyzed.</param>
        /// <returns>The objects that can be used to read values from all public instance properties and fields.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
        public List<IValueReader> AnalyzeType(Type type)
        {
            type.MustNotBeNull(nameof(type));

            var valueReaders = new List<IValueReader>();

            // ReSharper disable LoopCanBeConvertedToQuery
            foreach (var propertyInfo in type.GetRuntimeProperties())
            {
                if (propertyInfo.CanRead == false)
                    continue;

                var getMethod = propertyInfo.GetMethod;
                if (getMethod.IsPublic && getMethod.IsStatic == false)
                    valueReaders.Add(_valueReaderFactory.Create(type, propertyInfo));
            }

            foreach (var fieldInfo in type.GetRuntimeFields())
            {
                if (fieldInfo.IsPublic && fieldInfo.IsStatic == false)
                    valueReaders.Add(_valueReaderFactory.Create(type, fieldInfo));
            }
            // ReSharper restore LoopCanBeConvertedToQuery

            return valueReaders;
        }
    }
}