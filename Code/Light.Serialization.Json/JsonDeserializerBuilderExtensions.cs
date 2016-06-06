using System.Collections.Generic;
using System.Linq;
using Light.Serialization.Json.ComplexTypeConstruction;
using Light.Serialization.Json.ObjectMetadata;
using Light.Serialization.Json.TokenParserFactories;
using Light.Serialization.Json.TokenParsers;

namespace Light.Serialization.Json
{
    public static class JsonDeserializerBuilderExtensions
    {
        public static TCollection AddDefaultTokenParserFactories<TCollection>(this TCollection collection,
                                                                              IMetaFactory metaFactory,
                                                                              IMetadataParser complexObjectMetadataParser,
                                                                              IMetadataParser arrayMetadataParser,
                                                                              ITypeDescriptionService typeDescriptionService)
            where TCollection : IList<IJsonTokenParserFactory>
        {
            collection.Add(new SingletonFactory(new UnsignedIntegerParser()));
            collection.Add(new SingletonFactory(new DoubleParser()));
            collection.Add(new SingletonFactory(new BooleanParser()));
            collection.Add(new SingletonFactory(new NullParser()));
            collection.Add(new SingletonFactory(new DateTimeParser()));
            collection.Add(new SingletonFactory(new DateTimeOffsetParser()));
            collection.Add(new SingletonFactory(new TimeSpanParser()));
            collection.Add(new SingletonFactory(new SignedIntegerParser()));
            collection.Add(new SingletonFactory(new FloatParser()));
            collection.Add(new SingletonFactory(new DecimalParser()));
            collection.Add(new SingletonFactory(new CharacterParser()));
            collection.Add(new SingletonFactory(new EnumParser()));
            collection.Add(new SingletonFactory(new StringParser()));
            collection.Add(new SingletonFactory(new GuidParser()));
            collection.Add(new JsonStringInheritanceParserFactory(collection.Select(f => f.Create()).OfType<IJsonStringToPrimitiveParser>().ToList()));
            collection.Add(new SingletonFactory(new CollectionParser(metaFactory, arrayMetadataParser)));
            collection.Add(new SingletonFactory(new DictionaryParser(metaFactory, complexObjectMetadataParser)));
            collection.Add(new SingletonFactory(new ComplexObjectParser(metaFactory, typeDescriptionService, complexObjectMetadataParser)));

            return collection;
        }
    }
}