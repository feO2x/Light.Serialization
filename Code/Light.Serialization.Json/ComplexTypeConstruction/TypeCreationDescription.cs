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
        private readonly List<ConstructorDescription> _constructorDescriptions = new List<ConstructorDescription>();
        private readonly Dictionary<string, InjectableValueDescription> _injectableValueDescriptions = new Dictionary<string, InjectableValueDescription>();

        /// <summary>
        ///     Gets the type that is described by this instance.
        /// </summary>
        public readonly Type TargetType;

        /// <summary>
        ///     Creates a new instance of TypeCreationDescription.
        /// </summary>
        /// <param name="targetType">The type that will be described.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="targetType" /> is null.</exception>
        public TypeCreationDescription(Type targetType)
        {
            targetType.MustNotBeNull(nameof(targetType));

            TargetType = targetType;
        }

        /// <summary>
        ///     Gets the descriptions of all constructors of the target type that are registered with this instance.
        /// </summary>
        public IReadOnlyList<ConstructorDescription> ConstructorDescriptions => _constructorDescriptions;

        /// <summary>
        ///     Gets all injectable value descriptions that allow property injection.
        /// </summary>
        public IEnumerable<InjectableValueDescription> PropertyDescriptions => _injectableValueDescriptions.Values.Where(d => (d.Kind & InjectableValueKind.PropertySetter) != 0);

        /// <summary>
        ///     Gets all injetable value descriptions that allow field injection.
        /// </summary>
        public IEnumerable<InjectableValueDescription> FieldDescriptions => _injectableValueDescriptions.Values.Where(d => (d.Kind & InjectableValueKind.SettableField) != 0);

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

        /// <summary>
        ///     Adds the specified injectable value description to this type creation description instance.
        /// </summary>
        /// <param name="description">The description to be added.</param>
        public void AddInjectableValueDescription(InjectableValueDescription description)
        {
            description.MustNotBeNull(nameof(description));

            _injectableValueDescriptions.Add(description.NormalizedName, description);
        }

        /// <summary>
        ///     Adds the specified constructor description to this type creation description instance.
        /// </summary>
        /// <param name="description">The description to be added.</param>
        public void AddConstructorDescription(ConstructorDescription description)
        {
            description.MustNotBeNull(nameof(description));
            description.MustNotBeOneOf(_constructorDescriptions, nameof(description));

            _constructorDescriptions.Add(description);
        }
    }
}