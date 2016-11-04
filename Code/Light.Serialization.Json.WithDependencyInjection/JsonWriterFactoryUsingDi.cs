using System;
using System.IO;
using System.Text;
using Light.DependencyInjection;
using Light.GuardClauses;
using Light.Serialization.Json.LowLevelWriting;

namespace Light.Serialization.Json.WithDependencyInjection
{
    /// <summary>
    ///     Represents an <see cref="IJsonWriterFactory" /> that utilizes a <see cref="DependencyInjectionContainer" /> to create <see cref="JsonWriter" /> instances.
    /// </summary>
    public sealed class JsonWriterFactoryUsingDi : IJsonWriterFactory
    {
        private readonly DependencyInjectionContainer _container;

        /// <summary>
        ///     Initializes a new instance of <see cref="JsonWriterFactoryUsingDi" />.
        /// </summary>
        /// <param name="container">The container used to resolve dependencies for <see cref="JsonWriter" /> instances.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        public JsonWriterFactoryUsingDi(DependencyInjectionContainer container)
        {
            container.MustNotBeNull(nameof(container));

            _container = container;
        }

        /// <inheritdoc />
        public IJsonWriter CreateFromStringBuilder(StringBuilder stringBuilder)
        {
            return new JsonWriter(new StringBuilderAdapter(stringBuilder),
                                  _container.Resolve<IJsonWhitespaceFormatter>(),
                                  _container.Resolve<IJsonKeyNormalizer>());
        }

        /// <inheritdoc />
        public IJsonWriter CreateFromTextWriter(TextWriter writer)
        {
            return new JsonWriter(new TextWriterAdapter(writer),
                                  _container.Resolve<IJsonWhitespaceFormatter>(),
                                  _container.Resolve<IJsonKeyNormalizer>());
        }

        /// <inheritdoc />
        public IJsonWriter CreateFromBinaryWriter(BinaryWriter writer)
        {
            return new JsonWriter(new BinaryWriterAdapter(writer),
                                  _container.Resolve<IJsonWhitespaceFormatter>(),
                                  _container.Resolve<IJsonKeyNormalizer>());
        }
    }
}