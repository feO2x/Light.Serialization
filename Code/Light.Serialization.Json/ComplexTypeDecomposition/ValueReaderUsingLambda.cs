using System;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;

namespace Light.Serialization.Json.ComplexTypeDecomposition
{
    /// <summary>
    ///     Represents an <see cref="IValueReader" /> that uses a delegate internally to read a single value from an object.
    /// </summary>
    public sealed class ValueReaderUsingLambda : IValueReader
    {
        private readonly Func<object, object> _lambda;

        /// <summary>
        ///     Creates a new instance of <see cref="ValueReaderUsingLambda" />.
        /// </summary>
        /// <param name="name">The name of the member that is read from.</param>
        /// <param name="lambda">The delegate that reads the actual value from the target object.</param>
        /// <param name="referenceType">The type that is used to reference the value. This is usually the type of the field or property used within the target class.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="name" /> is empty.</exception>
        public ValueReaderUsingLambda(string name, Func<object, object> lambda, Type referenceType)
        {
            name.MustNotBeNullOrEmpty(nameof(name));
            lambda.MustNotBeNull(nameof(lambda));
            referenceType.MustNotBeNull(nameof(referenceType));

            Name = name;
            _lambda = lambda;
            ReferenceType = referenceType;
        }

        /// <summary>
        ///     Gets the name of the member whose value is read.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the type of the member that is used to reference the actual value.
        /// </summary>
        public Type ReferenceType { get; }

        /// <summary>
        ///     Executes the delegate to read the value from the specified object.
        /// </summary>
        /// <param name="object">The object whose value is read.</param>
        /// <returns>The value that was read from the specified object.</returns>
        public object ReadValue(object @object)
        {
            return _lambda(@object);
        }

        /// <summary>
        ///     Returns the string representation of this <see cref="ValueReaderUsingLambda" /> instance.
        /// </summary>
        public override string ToString()
        {
            return $"ValueProviderUsingLambda for {ReferenceType.FullName}.{Name}";
        }
    }
}