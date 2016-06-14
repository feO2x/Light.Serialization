using System;
using Light.Serialization.Json.LowLevelWriting;

namespace Light.Serialization.Json.BuilderHelpers
{
    /// <summary>
    ///     Represents the abstraction to inject a key normalizer via a property set method.
    /// </summary>
    public interface ISetKeyNormalizer
    {
        /// <summary>
        ///     Sets the specified key normalizer.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        IJsonKeyNormalizer KeyNormalizer { set; }
    }
}