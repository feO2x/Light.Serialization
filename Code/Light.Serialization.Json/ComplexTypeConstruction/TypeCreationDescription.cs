using System;
using System.Collections.Generic;
using System.Linq;
using Light.GuardClauses;

namespace Light.Serialization.Json.ComplexTypeConstruction
{
    public sealed class TypeCreationDescription
    {
        private readonly List<ConstructorDescription> _constructorDescriptions = new List<ConstructorDescription>();
        private readonly Dictionary<string, InjectableValueDescription> _injectableValueDescriptions = new Dictionary<string, InjectableValueDescription>();
        public readonly Type TargetType;

        public TypeCreationDescription(Type targetType)
        {
            targetType.MustNotBeNull(nameof(targetType));

            TargetType = targetType;
        }

        public IReadOnlyList<ConstructorDescription> ConstructorDescriptions => _constructorDescriptions;
        public IEnumerable<InjectableValueDescription> PropertyDescriptions => _injectableValueDescriptions.Values.Where(d => (d.Kind & InjectableValueKind.PropertySetter) != 0);
        public IEnumerable<InjectableValueDescription> FieldDescriptions => _injectableValueDescriptions.Values.Where(d => (d.Kind & InjectableValueKind.SettableField) != 0);
        public IEnumerable<InjectableValueDescription> UnknownOnTargetTypeDescriptions => _injectableValueDescriptions.Values.Where(d => (d.Kind & InjectableValueKind.UnknownOnTargetObject) != 0);

        public InjectableValueDescription GetInjectableValueDescriptionFromNormalizedName(string normalizedName)
        {
            InjectableValueDescription description;
            _injectableValueDescriptions.TryGetValue(normalizedName, out description);
            return description;
        }

        public void AddInjectableValueDescription(InjectableValueDescription description)
        {
            _injectableValueDescriptions.Add(description.NormalizedName, description);
        }

        public void AddConstructorDescription(ConstructorDescription description)
        {
            description.MustNotBeOneOf(_constructorDescriptions, nameof(description));

            _constructorDescriptions.Add(description);
        }
    }
}