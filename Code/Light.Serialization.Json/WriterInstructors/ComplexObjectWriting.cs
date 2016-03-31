using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.Serialization.Json.ComplexTypeDecomposition;

namespace Light.Serialization.Json.WriterInstructors
{
    /// <summary>
    ///     Provides a static helper method for serializing complex .NET object to complex JSON objects.
    /// </summary>
    public class ComplexObjectWriting
    {
        /// <summary>
        ///     Serializes the specified complex .NET object using the context and the list of value providers.
        /// </summary>
        /// <param name="context">The serialization context for the current value to be serialized.</param>
        /// <param name="valueProvidersForComplexObject">The value providers that can be used to read all values from the object to be serialized.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="valueProvidersForComplexObject" /> is null.</exception>
        public static void WriteValues(JsonSerializationContext context, IList<IValueProvider> valueProvidersForComplexObject)
        {
            valueProvidersForComplexObject.MustNotBeNull(nameof(valueProvidersForComplexObject));

            var writer = context.Writer;
            writer.BeginObject();
            for (var i = 0; i < valueProvidersForComplexObject.Count; i++)
            {
                var valueProvider = valueProvidersForComplexObject[i];
                var childValue = valueProvider.GetValue(context.ObjectToBeSerialized);

                writer.WriteKey(valueProvider.Name);
                if (childValue == null)
                    writer.WriteNull();
                else
                {
                    // TODO: This might end up in an endless loop if e.g. a property returns a reference to the object itself. Can be solved with a dictionary that contains all objects being serialized (what I wanted to integrate in the first place).
                    var childValueType = childValue.GetType();
                    context.SerializeChild(childValue, childValueType, valueProvider.ReferenceType);
                }

                if (i < valueProvidersForComplexObject.Count - 1)
                    writer.WriteDelimiter();
            }
            writer.EndObject();
        }
    }
}