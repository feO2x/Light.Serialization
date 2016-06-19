using System;
using System.Collections.Generic;
using System.Linq;
using Light.Serialization.Json.ComplexTypeConstruction;
using Light.Serialization.Json.ObjectMetadata;
using Light.Serialization.Json.TokenParsers;

namespace Light.Serialization.Json.BuilderHelpers
{
    /// <summary>
    ///     Provides extension methods for the <see cref="JsonDeserializerBuilder" />.
    /// </summary>
    public static class JsonDeserializerBuilderExtensions
    {
        /// <summary>
        ///     Adds factories to the specified collection that are able to create all default token parsers for a <see cref="JsonDeserializer" /> instance.
        /// </summary>
        /// <typeparam name="TCollection">The type of the collection.</typeparam>
        /// <param name="collection">The collection to be populated.</param>
        /// <param name="metaFactory">The factory that can create collections, dictionaries, and complex objects using type information.</param>
        /// <param name="complexObjectMetadataParser">The metadata parser for the complex JSON objects.</param>
        /// <param name="arrayMetadataParser">The metadata parser for JSON arrays.</param>
        /// <param name="typeDescriptionService">The type description service that creates type creation descriptions for complex types.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        /// <returns>The collection for method chaining.</returns>
        public static TCollection AddDefaultTokenParserFactories<TCollection>(this TCollection collection,
                                                                              IMetaFactory metaFactory,
                                                                              ITypeParser complexObjectMetadataParser,
                                                                              IArrayMetadataParser arrayMetadataParser,
                                                                              ITypeDescriptionService typeDescriptionService)
            where TCollection : IList<IJsonTokenParserFactory>
        {
            collection.Add(new SingletonFactory(new SignedIntegerParser()));
            collection.Add(new SingletonFactory(new DoubleParser()));
            collection.Add(new SingletonFactory(new BooleanParser()));
            collection.Add(new SingletonFactory(new CharacterParser()));
            collection.Add(new SingletonFactory(new DateTimeParser()));
            collection.Add(new SingletonFactory(new DateTimeOffsetParser()));
            collection.Add(new SingletonFactory(new TimeSpanParser()));
            collection.Add(new SingletonFactory(new UnsignedIntegerParser()));
            collection.Add(new SingletonFactory(new FloatParser()));
            collection.Add(new SingletonFactory(new DecimalParser()));
            collection.Add(new SingletonFactory(new NullParser()));
            collection.Add(new SingletonFactory(new EnumParser()));
            collection.Add(new SingletonFactory(new StringParser()));
            collection.Add(new SingletonFactory(new GuidParser()));
            collection.Add(new JsonStringInheritanceParserFactory(collection.Select(f => f.Create()).OfType<IJsonStringToPrimitiveParser>().ToList()));
            collection.Add(new SingletonFactory(new TypeAndTypeInfoParser(complexObjectMetadataParser)));
            collection.Add(new SingletonFactory(new CollectionParser(metaFactory, arrayMetadataParser)));
            collection.Add(new SingletonFactory(new DictionaryParser(metaFactory, complexObjectMetadataParser)));
            collection.Add(new SingletonFactory(new ComplexObjectParser(metaFactory, typeDescriptionService, complexObjectMetadataParser)));

            return collection;
        }
    }
}