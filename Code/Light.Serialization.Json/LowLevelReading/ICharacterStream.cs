using System;

namespace Light.Serialization.Json.LowLevelReading
{
    /// <summary>
    ///     Represents the abstraction of a character stream that the <see cref="JsonReader" />
    ///     can use to access the JSON document.
    /// </summary>
    public interface ICharacterStream
    {
        /// <summary>
        ///     Gets the internal buffer that is used by the character stream.
        /// </summary>
        char[] Buffer { get; }

        /// <summary>
        ///     Gets the current position that the stream is pointing to.
        /// </summary>
        int Position { get; }

        /// <summary>
        ///     Gets the value indicating whether the end of the stream is reached.
        /// </summary>
        bool IsAtEndOfStream { get; }

        /// <summary>
        ///     Gets the current character that the stream points to.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="IsAtEndOfStream" /> is true - you cannot get the current character when the end of the stream is reached.</exception>
        char CurrentCharacter { get; }

        /// <summary>
        ///     Pins the current position so that all characters from this index onwards are not overwritten when the next content is loaded into the underlying buffer.
        /// </summary>
        /// <returns>The current position of the stream.</returns>
        int PinPosition();

        /// <summary>
        ///     Advances the current position to the next character in the underlying buffer. The next content is loaded into the buffer if necessary.
        /// </summary>
        /// <returns>True if the position could be advanced, else false, if the end of the stream was reached.</returns>
        bool Advance();
    }
}