using System;
using System.Reflection;
using Light.Serialization.Json.FrameworkExtensions;

namespace Light.Serialization.Json.WriterInstructors
{
    /// <summary>
    ///     Represents an <see cref="IJsonWriterInstructor" /> that serializes .NET enum values to JSON strings.
    /// </summary>
    public sealed class EnumInstructor : IJsonWriterInstructor
    {
        /// <summary>
        ///     Checks if the actual type is an enum.
        /// </summary>
        public bool AppliesToObject(object @object, Type actualType, Type referencedType)
        {
            return actualType.GetTypeInfo().IsEnum;
        }

        /// <summary>
        ///     Serializes the specified enum value using the context.
        /// </summary>
        /// <param name="serializationContext">The serialization context of the enum value.</param>
        public void Serialize(JsonSerializationContext serializationContext)
        {
            serializationContext.Writer.WritePrimitiveValue(serializationContext.ObjectToBeSerialized.ToString().SurroundWithQuotationMarks());
        }
    }
}