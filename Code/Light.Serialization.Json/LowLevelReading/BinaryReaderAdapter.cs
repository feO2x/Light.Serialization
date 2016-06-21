using System;
using System.IO;
using Light.GuardClauses;

namespace Light.Serialization.Json.LowLevelReading
{
    /// <summary>
    ///     Represents an <see cref="ICharacterStream" /> that reads a complete JSON document as a string
    ///     from a <see cref="BinaryReader" /> into memory and forwards all calls to an internally created <see cref="StringStream" />.
    /// </summary>
    public sealed class BinaryReaderAdapter : ICharacterStream
    {
        private readonly StringStream _stringStream;

        /// <summary>
        ///     Creates a new instance of <see cref="BinaryReaderAdapter" />.
        /// </summary>
        /// <param name="reader">
        ///     The reader that is used to immediately read all
        ///     contents of the binary stream into memory as a string. It is disposed immediately after.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="reader" /> is null.</exception>
        public BinaryReaderAdapter(BinaryReader reader)
        {
            reader.MustNotBeNull(nameof(reader));

            var json = reader.ReadString();
            _stringStream = new StringStream(json);
            reader.Dispose();
        }

        /// <summary>
        ///     Does nothing.
        /// </summary>
        public void Dispose() { }

        /// <summary>
        ///     Forwards the calls to the internal <see cref="StringStream" />.
        /// </summary>
        public char[] Buffer => _stringStream.Buffer;

        /// <summary>
        ///     Forwards the calls to the internal <see cref="StringStream" />.
        /// </summary>
        public int Position => _stringStream.Position;

        /// <summary>
        ///     Forwards the calls to the internal <see cref="StringStream" />.
        /// </summary>
        public bool IsAtEndOfStream => _stringStream.IsAtEndOfStream;

        /// <summary>
        ///     Forwards the calls to the internal <see cref="StringStream" />.
        /// </summary>
        public char CurrentCharacter => _stringStream.CurrentCharacter;

        /// <summary>
        ///     Forwards the calls to the internal <see cref="StringStream" />.
        /// </summary>
        public int PinPosition()
        {
            return _stringStream.PinPosition();
        }

        /// <summary>
        ///     Forwards the calls to the internal <see cref="StringStream" />.
        /// </summary>
        public bool Advance()
        {
            return _stringStream.Advance();
        }
    }
}