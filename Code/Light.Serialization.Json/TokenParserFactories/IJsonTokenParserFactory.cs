using System;
using Light.Serialization.Json.TokenParsers;

namespace Light.Serialization.Json.TokenParserFactories
{
    public interface IJsonTokenParserFactory
    {
        Type ParserType { get; }

        IJsonTokenParser Create();
    }
}