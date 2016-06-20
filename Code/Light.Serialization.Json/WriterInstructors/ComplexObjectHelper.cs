using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.Serialization.Json.ComplexTypeDecomposition;
using Light.Serialization.Json.ObjectMetadata;

namespace Light.Serialization.Json.WriterInstructors
{
    /// <summary>
    ///     Provides a static helper method for serializing complex .NET objects to complex JSON objects.
    /// </summary>
    public static class ComplexObjectHelper
    {
        /// <summary>
        ///     Serializes the specified complex .NET object using the context and the list of value readers.
        /// </summary>
        /// <param name="context">The serialization context for the current value to be serialized.</param>
        /// <param name="valueReaders">The value readers that can be used to read all values from the object to be serialized.</param>
        /// <param name="metadataInstructor">The object that serializes the metadata section of the complex object.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="valueReaders" /> or <paramref name="metadataInstructor" /> is null.</exception>
        public static void SerializeComplexObject(JsonSerializationContext context, List<IValueReader> valueReaders, IMetadataInstructor metadataInstructor)
        {
            valueReaders.MustNotBeNull(nameof(valueReaders));
            metadataInstructor.MustNotBeNull(nameof(metadataInstructor));

            var writer = context.Writer;
            writer.BeginObject();

            var shouldSerializeData = metadataInstructor.SerializeMetadata(context);
            if (shouldSerializeData == false)
            {
                writer.EndObject();
                return;
            }

            for (var i = 0; i < valueReaders.Count; i++)
            {
                var valueReader = valueReaders[i];
                var childValue = valueReader.ReadValue(context.ObjectToBeSerialized);

                writer.WriteKey(valueReader.Name);
                if (childValue == null)
                    writer.WriteNull();
                else
                    context.SerializeChild(childValue);

                if (i < valueReaders.Count - 1)
                    writer.WriteDelimiter();
            }
            writer.EndObject();
        }
    }
}