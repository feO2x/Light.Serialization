using System;
using System.IO.Compression;
using Xunit;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Light.Serialization.Json.Tests.DeserializationTests
{
    public sealed class EnumTests : BaseJsonDeserializerTest
    {
        [Theory(DisplayName = "The deserializer must be able to deserialize JSON strings to .NET enum values.")]
        [InlineData("\"black\"", ConsoleColor.Black)]
        [InlineData("\"insertLineBreaks\"", Base64FormattingOptions.InsertLineBreaks)]
        [InlineData("\"noCompression\"", CompressionLevel.NoCompression)]
        public void EnumValues<T>(string json, T expected)
        {
            CompareDeserializedJsonToExpected(json, expected);
        }
    }
}