using System;
using System.Reflection;

namespace Light.Serialization.Json.ComplexTypeDecomposition
{
    /// <summary>
    ///     Represents a factory that creates <see cref="IValueProvider" /> instances out of property infos or field infos.
    /// </summary>
    public interface IValueProviderFactory
    {
        /// <summary>
        ///     Creates a <see cref="IValueProvider" /> instance for the specified property info.
        /// </summary>
        /// <param name="targetType">The type that the property belongs to.</param>
        /// <param name="propertyInfo">The info describing the property.</param>
        /// <returns>A value provider that is able to read the value from the specified property.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        IValueProvider Create(Type targetType, PropertyInfo propertyInfo);

        /// <summary>
        ///     Creates a <see cref="IValueProvider" /> instance for the specified field info.
        /// </summary>
        /// <param name="targetType">The type that the field belongs to.</param>
        /// <param name="fieldInfo">The info describing the field.</param>
        /// <returns>A value provider that is able to read the value from the specified field.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        IValueProvider Create(Type targetType, FieldInfo fieldInfo);
    }
}