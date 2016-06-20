using System;
using Light.Serialization.Json.LowLevelWriting;

namespace Light.Serialization.Json.BuilderHelpers
{
    /// <summary>
    ///     Represents an abstraction of setting a delegate that creates an instance of <see cref="IJsonWhitespaceFormatter" />.
    /// </summary>
    public interface ISetWhitespaceFormatterCreationDelegate
    {
        /// <summary>
        ///     Sets the delegate that creates an <see cref="IJsonWhitespaceFormatter" /> instance.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        Func<IJsonWhitespaceFormatter> CreateWhitespaceFormatter { set; }
    }
}