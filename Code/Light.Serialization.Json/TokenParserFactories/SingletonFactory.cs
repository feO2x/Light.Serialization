using System;
using Light.GuardClauses;
using Light.Serialization.Json.BuilderInterfaces;
using Light.Serialization.Json.TokenParsers;

namespace Light.Serialization.Json.TokenParserFactories
{
    public sealed class SingletonFactory : IJsonTokenParserFactory, IGetSingletonInstance<IJsonTokenParser>
    {
        public SingletonFactory(IJsonTokenParser instance)
        {
            instance.MustNotBeNull(nameof(instance));

            Instance = instance;
            ParserType = Instance.GetType();
        }

        public Type ParserType { get; }

        public IJsonTokenParser Instance { get; }

        public IJsonTokenParser Create()
        {
            return Instance;
        }
    }
}