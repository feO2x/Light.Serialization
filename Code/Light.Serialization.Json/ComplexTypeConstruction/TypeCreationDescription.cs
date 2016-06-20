using System;
using System.Collections.Generic;
using System.Linq;
using Light.GuardClauses;

namespace Light.Serialization.Json.ComplexTypeConstruction
{
    /// <summary>
    ///     Represents an object that describes how a type can be instantiated with constructor injection and further populated through
    ///     property and field injection.
    /// </summary>
    public sealed class TypeCreationDescription
    {
        private readonly Dictionary<string, InjectableValueDescription> _injectableValueDescriptions;

        /// <summary>
        ///     Gets the descriptions of all the constructors of the specified type.
        /// </summary>
        public readonly List<ConstructorDescription> ConstructorDescriptions;

        /// <summary>
        ///     Gets all injetable value descriptions that allow field injection.
        /// </summary>
        public readonly List<InjectableValueDescription> FieldDescriptions;

        /// <summary>
        ///     Gets all injectable value descriptions that allow property injection.
        /// </summary>
        public readonly List<InjectableValueDescription> PropertyDescriptions;

        /// <summary>
        ///     Gets the type that is described by this instance.
        /// </summary>
        public readonly Type TargetType;

        /// <summary>
        ///     Creates a new instance of TypeCreationDescription.
        /// </summary>
        /// <param name="targetType">The type that will be described.</param>
        /// <param name="constructorDescriptions">The list containing all descriptions for the constructors of the target type.</param>
        /// <param name="injectableValueDescriptions">The list containing all <see cref="InjectableValueDescription" /> instances for this type.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public TypeCreationDescription(Type targetType, List<ConstructorDescription> constructorDescriptions, List<InjectableValueDescription> injectableValueDescriptions)
        {
            targetType.MustNotBeNull(nameof(targetType));
            constructorDescriptions.MustNotBeNull(nameof(constructorDescriptions));
            injectableValueDescriptions.MustNotBeNull(nameof(injectableValueDescriptions));

            TargetType = targetType;
            ConstructorDescriptions = constructorDescriptions;
            _injectableValueDescriptions = injectableValueDescriptions.ToDictionary(injectableValueDescription => injectableValueDescription.NormalizedName);
            PropertyDescriptions = injectableValueDescriptions.Where(injectableValueDescription => injectableValueDescription.PropertyInfo != null).ToList();
            FieldDescriptions = injectableValueDescriptions.Where(injectableValueDescription => injectableValueDescription.FieldInfo != null).ToList();
        }

        /// <summary>
        ///     Gets the injectable value description that corresponds to the given normalized name.
        /// </summary>
        /// <param name="normalizedName">The normalized name of the injectable value description.</param>
        /// <returns>The injectable value description if it could be found, else null.</returns>
        public InjectableValueDescription GetInjectableValueDescriptionFromNormalizedName(string normalizedName)
        {
            InjectableValueDescription description;
            _injectableValueDescriptions.TryGetValue(normalizedName, out description);
            return description;
        }
    }
}