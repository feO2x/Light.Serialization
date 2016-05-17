using System;
using System.Diagnostics;
using System.Linq;
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
        ///     Checks if the given type implements the specified generic non-resolved interface.
        /// </summary>
        /// <param name="type">The type to be analyzed.</param>
        /// <param name="genericInterface">The type of the interface that <paramref name="type" /> should implement. The generic parameters of this type must not be resolved.</param>
        /// <returns>True if <paramref name="type" /> implements the specified generic interface, else false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="genericInterface" /> does not describe an interface type whose generic parameters are not resolved.</exception>
        public static bool ImplementsGenericInterface(this TypeInfo type, Type genericInterface)
        {
            type.MustNotBeNull(nameof(type));
            CheckGenericInterfaceType(genericInterface);

            foreach (var interfaceTypeInfo in type.ImplementedInterfaces.Select(t => t.GetTypeInfo()))
            {
                if (interfaceTypeInfo.IsGenericType == false)
                    continue;

                var usedInterfaceType = interfaceTypeInfo;
                if (interfaceTypeInfo.IsGenericTypeDefinition == false)
                    usedInterfaceType = interfaceTypeInfo.GetGenericTypeDefinition().GetTypeInfo();
                if (usedInterfaceType.GetGenericTypeDefinition().EqualsWithHashCode(genericInterface))
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
        public static TypeInfo GetResolvedTypeInfoForGenericInterface(this TypeInfo sourceType, Type genericInterface)
        {
            sourceType.MustNotBeNull();
            CheckGenericInterfaceType(genericInterface);

            foreach (var interfaceTypeInfo in sourceType.ImplementedInterfaces.Select(t => t.GetTypeInfo()))
            {
                if (interfaceTypeInfo.IsGenericType == false)
                    continue;
                if (interfaceTypeInfo.GetGenericTypeDefinition().EqualsWithHashCode(genericInterface))
                    return interfaceTypeInfo;
            }

            return null;
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckGenericInterfaceType(Type genericInterface)
        {
            genericInterface.MustNotBeNull(nameof(genericInterface));

            var interfaceTypeInfo = genericInterface.GetTypeInfo();
            if (interfaceTypeInfo.IsInterface == false)
                throw new ArgumentException($"The specified type \"{genericInterface.FullName}\" is no interface type.");

            if (interfaceTypeInfo.IsGenericTypeDefinition == false)
                throw new ArgumentException($"The specified type \"{genericInterface.FullName}\" is not an unresolved generic type.");
        }
    }
}