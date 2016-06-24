using System;
using System.Collections.Generic;
using System.Reflection;
using Light.GuardClauses;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents the base class of an <see cref="IJsonTokenParser" /> that can deserialize
    ///     JSON strings to .NET primitve types other than <see cref="string" />.
    /// </summary>
    /// <typeparam name="T">The primitive type that the parser handles.</typeparam>
    public abstract class BaseJsonStringToPrimitiveParser<T>
    {
        private readonly List<Type> _associatedInterfacesAndBaseClasses = new List<Type>();

        /// <summary>
        ///     Initializes the <see cref="AssociatedInterfacesAndBaseClasses" /> collection
        ///     with all base classes and interfaces of <see cref="T" />.
        /// </summary>
        protected BaseJsonStringToPrimitiveParser()
        {
            var typeInfo = typeof(T).GetTypeInfo();
            Check.Against(typeInfo.IsInterface, () => new InvalidOperationException($"The specified type {typeInfo.FullName} is an interface and cannot be used with this base class."));
            Check.Against(typeInfo.BaseType == typeof(Delegate), () => new InvalidOperationException($"The specified type {typeInfo.FullName} is a delegate and cannot be used with this base class."));

            _associatedInterfacesAndBaseClasses.Add(typeof(object));

            // If it is a value type, it can only be a struct or an enum
            if (typeInfo.IsValueType)
            {
                _associatedInterfacesAndBaseClasses.Add(typeof(ValueType));
                if (typeInfo.IsEnum)
                    _associatedInterfacesAndBaseClasses.Add(typeof(Enum));
                else
                {
                    foreach (var @interface in typeInfo.ImplementedInterfaces)
                    {
                        _associatedInterfacesAndBaseClasses.Add(@interface);
                    }
                }
                return;
            }

            // Else it is a class - get all base classes and interfaces along the inheritance hierarchy
            var currentType = typeInfo;
            while (currentType.AsType() != typeof(object))
            {
                foreach (var @interface in currentType.ImplementedInterfaces)
                {
                    if (_associatedInterfacesAndBaseClasses.Contains(@interface) == false)
                        _associatedInterfacesAndBaseClasses.Add(@interface);
                }

                if (_associatedInterfacesAndBaseClasses.Contains(currentType.BaseType) == false)
                    _associatedInterfacesAndBaseClasses.Add(currentType.BaseType);

                currentType = currentType.BaseType.GetTypeInfo();
            }
        }

        /// <summary>
        ///     Gets the base types that <see cref="T" /> derives from or implements.
        /// </summary>
        public IReadOnlyList<Type> AssociatedInterfacesAndBaseClasses => _associatedInterfacesAndBaseClasses;
    }
}