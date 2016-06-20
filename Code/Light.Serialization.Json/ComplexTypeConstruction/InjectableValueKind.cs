using System;

namespace Light.Serialization.Json.ComplexTypeConstruction
{
    /// <summary>
    ///     Represents the different kinds of injection an <see cref="InjectableValueDescription" /> instance can have.
    /// </summary>
    [Flags]
    public enum InjectableValueKind
    {
        ConstructorParameter = 1,
        PropertySetter = 2,
        SettableField = 4,
        UnknownOnTargetObject = 0
    }
}