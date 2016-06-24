using System;
using System.Reflection;

namespace Light.Serialization.Json.ComplexTypeDecomposition
{
    /// <summary>
    ///     Represents a factory that creates <see cref="IValueReader" /> instances for <see cref="PropertyInfo" /> and <see cref="FieldInfo" /> objects.
    /// </summary>
    public interface IValueReaderFactory
    {
        /// <summary>
        ///     Creates an <see cref="IValueReader" /> instance for the specified <see cref="PropertyInfo" />.
        /// </summary>
        /// <param name="targetType">The type that the property belongs to.</param>
        /// <param name="propertyInfo">The info describing the property.</param>
        /// <returns>An <see cref="IValueReader" /> that is able to read the value from the specified property.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        IValueReader Create(Type targetType, PropertyInfo propertyInfo);

        /// <summary>
        ///     Creates a <see cref="IValueReader" /> instance for the specified <see cref="FieldInfo" />.
        /// </summary>
        /// <param name="targetType">The type that the field belongs to.</param>
        /// <param name="fieldInfo">The info describing the field.</param>
        /// <returns>An <see cref="IValueReader" /> that is able to read the value from the specified field.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        IValueReader Create(Type targetType, FieldInfo fieldInfo);
    }
}