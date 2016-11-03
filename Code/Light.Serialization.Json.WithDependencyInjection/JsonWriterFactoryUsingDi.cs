using System.IO;
using System.Text;
using Light.DependencyInjection;
using Light.GuardClauses;
using Light.Serialization.Json.LowLevelWriting;

namespace Light.Serialization.Json.WithDependencyInjection
{
    public sealed class JsonWriterFactoryUsingDi : IJsonWriterFactory
    {
        private readonly DependencyInjectionContainer _container;

        public JsonWriterFactoryUsingDi(DependencyInjectionContainer container)
        {
            container.MustNotBeNull(nameof(container));

            _container = container;
        }

        public IJsonWriter CreateFromStringBuilder(StringBuilder stringBuilder)
        {
            return new JsonWriter(new StringBuilderAdapter(stringBuilder),
                                  _container.Resolve<IJsonWhitespaceFormatter>(),
                                  _container.Resolve<IJsonKeyNormalizer>());
        }

        public IJsonWriter CreateFromTextWriter(TextWriter writer)
        {
            return new JsonWriter(new TextWriterAdapter(writer),
                                  _container.Resolve<IJsonWhitespaceFormatter>(),
                                  _container.Resolve<IJsonKeyNormalizer>());
        }

        public IJsonWriter CreateFromBinaryWriter(BinaryWriter writer)
        {
            return new JsonWriter(new BinaryWriterAdapter(writer),
                                  _container.Resolve<IJsonWhitespaceFormatter>(),
                                  _container.Resolve<IJsonKeyNormalizer>());
        }
    }
}