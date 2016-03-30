using System;

namespace Light.Serialization.Json.LowLevelWriting
{
    /// <summary>
    ///     Represents the abstraction of an object that formats the whitespace in a JSON document.
    /// </summary>
    public interface IJsonWhitespaceFormatter
    {
        /// <summary>
        ///     Gets the current indentation level of the JSON document.
        /// </summary>
        int CurrentIndentationLevel { get; }

        /// <summary>
        ///     Creates a newline and increases the indentation level using the specified writer.
        /// </summary>
        /// <param name="writer">The object that writes the JSON document.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer" /> is null.</exception>
        void NewlineAndIncreaseIndent(IJsonWriter writer);

        /// <summary>
        ///     Creates a newline using the specified writer.
        /// </summary>
        /// <param name="writer">The object that writes the JSON document.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer" /> is null.</exception>
        void Newline(IJsonWriter writer);

        /// <summary>
        ///     Creates a newline and decreases the indentation level using the specified writer.
        /// </summary>
        /// <param name="writer">The object that writes the JSON document.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer" /> is null.</exception>
        void NewlineAndDecreaseIndent(IJsonWriter writer);

        /// <summary>
        ///     Inserts whitespace between a key and a value in a complex JSON object, using the specified writer.
        /// </summary>
        /// <param name="writer">The object that writes the JSON document.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer" /> is null.</exception>
        void InsertWhitespaceBetweenKeyAndValue(IJsonWriter writer);

        /// <summary>
        ///     Resets the indentation level to zero.
        /// </summary>
        void ResetIndentationLevel();
    }
}