using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.Serialization.Json.FrameworkExtensions
{
    /// <summary>
    ///     Provides extension methods for TypeInfo instances.
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        ///     Creates a list containing all interfaces that are implemented in the hierarchy of the specified type.
        /// </summary>
        /// <param name="type">The type info to be analyzed.</param>
        /// <returns>A new list containing all interfaces of the type hierarchy.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
        public static IList<Type> GetAllInterfacesOfInheritanceHierarchy(this TypeInfo type)
        {
            var interfaceTypes = new List<Type>();
            return GetAllInterfacesOfInheritanceHierarchy(type, interfaceTypes);
        }

        /// <summary>
        ///     Populates the given list with all interfaces that are implemented in the hierarchy of the specified type.
        /// </summary>
        /// <param name="type">The type info to be analyzed.</param>
        /// <param name="interfaceTypes">The list that will be populated.</param>
        /// <returns>The list to allow method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public static IList<Type> GetAllInterfacesOfInheritanceHierarchy(this TypeInfo type, IList<Type> interfaceTypes)
        {
            type.MustNotBeNull(nameof(type));
            interfaceTypes.MustNotBeNull(nameof(interfaceTypes));

            if (type.IsInterface)
                interfaceTypes.Add(type.AsType());

            PopulateInterfacesTypes(type, interfaceTypes);
            return interfaceTypes;
        }

        private static void PopulateInterfacesTypes(TypeInfo type, ICollection<Type> interfaceTypes)
        {
            while (true)
            {
                var interfaces = type.ImplementedInterfaces;
                foreach (var @interface in interfaces)
                {
                    if (interfaceTypes.Contains(@interface))
                        continue;
                    interfaceTypes.Add(@interface);
                    PopulateInterfacesTypes(@interface.GetTypeInfo(), interfaceTypes);
                }

                var baseClass = type.BaseType;
                if (baseClass != null)
                {
                    type = baseClass.GetTypeInfo();
                    continue;
                }
                break;
            }
        }

        /// <summary>
        ///     Checks if the given type implements the specified generic non-resolved interface.
        /// </summary>
        /// <param name="type">The type to be analyzed.</param>
        /// <param name="genericInterface">The type of the interface that <paramref name="type" /> should implement. The generic parameters of this type must not be resolved.</param>
        /// <returns>True if <paramref name="type" /> implements the specified generic interface, else false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="genericInterface" /> does not describe an interface type whose generic parameters are not resolved.</exception>
        public static bool ImplementsGenericInterface(this TypeInfo type, TypeInfo genericInterface)
        {
            type.MustNotBeNull(nameof(type));
            CheckGenericInterfaceType(genericInterface);

            var allInterfaces = type.GetAllInterfacesOfInheritanceHierarchy();
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < allInterfaces.Count; i++)
            {
                var @interface = allInterfaces[i].GetTypeInfo();
                if (@interface.IsGenericType == false)
                    continue;
                if (@interface.IsGenericTypeDefinition == false)
                    @interface = @interface.GetGenericTypeDefinition().GetTypeInfo();
                if (@interface.EqualsWithHashCode(genericInterface))
                    return true;
            }
            return false;
        }

        /// <summary>
        ///     Checks if the given type implements a resolved version of the specified generic interface type (without resolved generic parameters), and if yes, this resolved type will be returned.
        /// </summary>
        /// <param name="sourceType">The type to be analyzed.</param>
        /// <param name="genericInterface">The generic interface type that must have no resolved generic parameters.</param>
        /// <returns>The resolved interface type if it is part of the inheritance hierarchy of <see cref="sourceType" />.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="genericInterface" /> does not describe an interface type whose generic parameters are not resolved.</exception>
        public static TypeInfo GetSpecificTypeInfoThatCorrespondsToGenericInterface(this TypeInfo sourceType, TypeInfo genericInterface)
        {
            sourceType.MustNotBeNull();
            CheckGenericInterfaceType(genericInterface);

            var allInterfaces = sourceType.GetAllInterfacesOfInheritanceHierarchy();
            // ReSharper disable once ForCanBeConvertedToForeach
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < allInterfaces.Count; i++)
            {
                var @interface = allInterfaces[i].GetTypeInfo();
                if (@interface.IsGenericType == false)
                    continue;
                if (@interface.IsGenericTypeDefinition == false &&
                    @interface.GetGenericTypeDefinition().GetTypeInfo().EqualsWithHashCode(genericInterface))
                    return @interface;
            }
            return null;
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckGenericInterfaceType(TypeInfo genericInterface)
        {
            genericInterface.MustNotBeNull(nameof(genericInterface));
            if (genericInterface.IsInterface == false)
                throw new ArgumentException($"The specified type \"{genericInterface.FullName}\" is no interface type.");

            if (genericInterface.IsGenericTypeDefinition == false)
                throw new ArgumentException($"The specified type \"{genericInterface.FullName}\" is not an unresolved generic type.");
        }
    }
}