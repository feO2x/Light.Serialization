using System;
using Light.GuardClauses;
using Light.Serialization.Json.TokenParsers;

namespace Light.Serialization.Json.Builders
{
    public sealed class SingletonFactory : IJsonTokenParserFactory
    {
        public readonly IJsonTokenParser Instance;

        public SingletonFactory(IJsonTokenParser instance)
        {
            instance.MustNotBeNull(nameof(instance));

            Instance = instance;
            ParserType = Instance.GetType();
        }

        public Type ParserType { get; }

        public IJsonTokenParser Create()
        {
            return Instance;
        }
    }
}