﻿using System;
using Light.GuardClauses;

namespace Light.Serialization.Json.LowLevelWriting
{
    /// <summary>
    ///     Represents an <see cref="IJsonWhitespaceFormatter" /> that produces human-readable JSON documents with newlines and indentation.
    /// </summary>
    public sealed class IndentingWhitespaceFormatter : IJsonWhitespaceFormatter
    {
        private const string WhiteSpace = " ";
        private int _currentIndentationLevel;
        private string _indentCharacters = "    ";

        /// <summary>
        ///     Gets or sets the characters used for indentation. This value defaults to "    " (four spaces).
        /// </summary>
        public string IdentCharacters
        {
            get { return _indentCharacters; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _indentCharacters = value;
            }
        }

        /// <summary>
        ///     Gets the current indentation level of the JSON document.
        /// </summary>
        public int CurrentIndentationLevel => _currentIndentationLevel;

        /// <summary>
        ///     Creates a newline and increases the indentation level using the specified writer.
        /// </summary>
        /// <param name="writer">The object that writes the JSON document.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer" /> is null.</exception>
        public void NewlineAndIncreaseIndent(IJsonWriter writer)
        {
            writer.MustNotBeNull(nameof(writer));

            writer.WritePrimitive(Environment.NewLine);
            ++_currentIndentationLevel;
            WriteIndent(writer);
        }

        /// <summary>
        ///     Creates a newline using the specified writer.
        /// </summary>
        /// <param name="writer">The object that writes the JSON document.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer" /> is null.</exception>
        public void Newline(IJsonWriter writer)
        {
            writer.MustNotBeNull(nameof(writer));

            writer.WritePrimitive(Environment.NewLine);
            WriteIndent(writer);
        }

        /// <summary>
        ///     Creates a newline and decreases the indentation level using the specified writer.
        /// </summary>
        /// <param name="writer">The object that writes the JSON document.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer" /> is null.</exception>
        public void NewlineAndDecreaseIndent(IJsonWriter writer)
        {
            writer.MustNotBeNull(nameof(writer));

            writer.WritePrimitive(Environment.NewLine);
            --_currentIndentationLevel;
            WriteIndent(writer);
        }

        /// <summary>
        ///     Inserts whitespace between a key and a value in a complex JSON object, using the specified writer.
        /// </summary>
        /// <param name="writer">The object that writes the JSON document.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer" /> is null.</exception>
        public void InsertWhitespaceBetweenKeyAndValue(IJsonWriter writer)
        {
            writer.MustNotBeNull(nameof(writer));

            writer.WritePrimitive(WhiteSpace);
        }

        private void WriteIndent(IJsonWriter writer)
        {
            for (var i = 0; i < _currentIndentationLevel; i++)
            {
                writer.WritePrimitive(IdentCharacters);
            }
        }

        /// <summary>
        ///     Creates a new instance of <see cref="IndentingWhitespaceFormatter" /> using the default constructor.
        /// </summary>
        public static IJsonWhitespaceFormatter Create()
        {
            return new IndentingWhitespaceFormatter();
        }
    }
}