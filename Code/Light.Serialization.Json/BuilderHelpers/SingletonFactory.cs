using System;
using Light.GuardClauses;
using Light.Serialization.Json.TokenParsers;

namespace Light.Serialization.Json.BuilderHelpers
{
    /// <summary>
    ///     Represents an <see cref="IJsonTokenParserFactory" /> that returns a singleton instance of an <see cref="IJsonTokenParser" />.
    /// </summary>
    public sealed class SingletonFactory : IJsonTokenParserFactory, IGetSingletonInstance<IJsonTokenParser>
    {
        private readonly IJsonTokenParser _instance;

        /// <summary>
        ///     Creates a new instance of <see cref="SingletonFactory" />.
        /// </summary>
        /// <param name="instance">The singleton JSON token parser instance that will be returned by this factory.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance" /> is null.</exception>
        public SingletonFactory(IJsonTokenParser instance)
        {
            instance.MustNotBeNull(nameof(instance));

            _instance = instance;
            ParserType = Instance.GetType();
        }

        /// <summary>
        ///     Gets the singleton instance that this factory manages.
        /// </summary>
        public IJsonTokenParser Instance => _instance;

        /// <summary>
        ///     Gets the conrete parser type of the singleton instance.
        /// </summary>
        public Type ParserType { get; }

        /// <summary>
        ///     Returns the singleton instance that this factory manages.
        /// </summary>
        public IJsonTokenParser Create()
        {
            return _instance;
        }
    }
}