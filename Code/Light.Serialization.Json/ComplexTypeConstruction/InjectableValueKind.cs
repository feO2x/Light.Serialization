using System;

namespace Light.Serialization.Json.ComplexTypeConstruction
{
    [Flags]
    public enum InjectableValueKind
    {
        ConstructorParameter = 1,
        PropertySetter = 2,
        SettableField = 4,
        UnknownOnTargetObject = 0
    }
}