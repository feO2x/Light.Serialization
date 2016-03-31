using System;
using System.Collections;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.Serialization.Json.PrimitiveTypeFormatters;

namespace Light.Serialization.Json.WriterInstructors
{
    /// <summary>
    ///     Represents an <see cref="IJsonWriterInstructor" /> that serializes .NET dictionaries to complex JSON objects.
    /// </summary>
    public sealed class DictionaryInstructor : IJsonWriterInstructor
    {
        private readonly IDictionary<Type, IPrimitiveTypeFormatter> _primitiveTypeToFormattersMapping;

        /// <summary>
        ///     Creates a new instance of <see cref="DictionaryInstructor" />.
        /// </summary>
        /// <param name="primitiveTypeToFormattersMapping">The dictionary containing mappings from type to primitive type formatters which are used to serialize keys.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="primitiveTypeToFormattersMapping" /> is null.</exception>
        public DictionaryInstructor(IDictionary<Type, IPrimitiveTypeFormatter> primitiveTypeToFormattersMapping)
        {
            primitiveTypeToFormattersMapping.MustNotBeNull(nameof(primitiveTypeToFormattersMapping));

            _primitiveTypeToFormattersMapping = primitiveTypeToFormattersMapping;
        }

        /// <summary>
        ///     Checks if the specified object is an <see cref="IDictionary" /> instance.
        /// </summary>
        public bool AppliesToObject(object @object, Type actualType, Type referencedType)
        {
            return @object is IDictionary;
        }

        /// <summary>
        ///     Serializes the specified dictionary using the serialization context.
        /// </summary>
        /// <param name="serializationContext">The serialization context for the dictionary.</param>
        /// <exception cref="InvalidCastException">Thrown when the object to be serialized is not of type <see cref="IDictionary" />.</exception>
        public void Serialize(JsonSerializationContext serializationContext)
        {
            var dictionary = (IDictionary) serializationContext.ObjectToBeSerialized;

            var writer = serializationContext.Writer;
            writer.BeginObject();

            if (dictionary.Count == 0)
            {
                writer.EndObject();
                return;
            }

            var dicitionaryEnumerator = dictionary.GetEnumerator();
            dicitionaryEnumerator.MoveNext();
            while (true)
            {
                var key = dicitionaryEnumerator.Key;
                if (key == null)
                    writer.WriteKey(JsonSymbols.Null);
                else
                {
                    var keyType = key.GetType();
                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if (_primitiveTypeToFormattersMapping.ContainsKey(keyType))
                    {
                        var typeFormatter = _primitiveTypeToFormattersMapping[keyType];
                        writer.WriteKey(typeFormatter.FormatPrimitiveType(key), typeFormatter.ShouldBeNormalizedKey);
                    }
                    else
                        writer.WriteKey(key.ToString(), false);
                }

                var value = dicitionaryEnumerator.Value;
                if (value == null)
                    writer.WriteNull();
                else
                {
                    var valueType = value.GetType();
                    // TODO: here an endless loop can be created too, if the value of a dictionary is the dictionary itself
                    serializationContext.SerializeChild(value, valueType, valueType);
                }

                if (dicitionaryEnumerator.MoveNext())
                    writer.WriteDelimiter();
                else
                    break;
            }
            writer.EndObject();
        }
    }
}