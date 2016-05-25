using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.Serialization.Json.TokenParsers;

namespace Light.Serialization.Json.TokenParserFactories
{
    public sealed class JsonStringInheritanceParserFactory : IJsonTokenParserFactory
    {
        private readonly List<IJsonStringToPrimitiveParser> _jsonStringToPrimitiveParsers;

        public JsonStringInheritanceParserFactory(List<IJsonStringToPrimitiveParser> jsonStringToPrimitiveParsers)
        {
            jsonStringToPrimitiveParsers.MustNotBeNull(nameof(jsonStringToPrimitiveParsers));

            _jsonStringToPrimitiveParsers = jsonStringToPrimitiveParsers;
        }

        public Type ParserType => typeof(JsonStringInheritanceParser);

        public IJsonTokenParser Create()
        {
            return new JsonStringInheritanceParser(_jsonStringToPrimitiveParsers);
        }
    }
}