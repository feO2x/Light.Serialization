using System;
using Light.Serialization.Json.TokenParsers;

namespace Light.Serialization.Json.BuilderHelpers
{
    /// <summary>
    ///     Represents the abstraction of a factory that creates <see cref="IJsonTokenParser" /> instances.
    /// </summary>
    public interface IJsonTokenParserFactory
    {
        /// <summary>
        ///     Gets the concrete type of the parser that this factory creates.
        /// </summary>
        Type ParserType { get; }

        /// <summary>
        ///     Creates a new instance of the target parser.
        /// </summary>
        IJsonTokenParser Create();
    }
}