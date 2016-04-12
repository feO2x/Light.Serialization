using System;
using Light.Serialization.Json.PrimitiveTypeFormatters;

namespace Light.Serialization.Json.BuilderInterfaces
{
    /// <summary>
    ///     Represents the abstraction to inject a metadata instructor via a property set method.
    /// </summary>
    public interface ISetCharacterEscaper
    {
        /// <summary>
        ///     Sets the specified character escaper.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        ICharacterEscaper CharacterEscaper { set; }
    }
}