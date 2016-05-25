using System;
using Light.Serialization.Json.TokenParsers;

namespace Light.Serialization.Json.Builders
{
    public interface IJsonTokenParserFactory
    {
        Type ParserType { get; }

        IJsonTokenParser Create();
    }
}