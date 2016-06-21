using System.IO;
using System.Text;
using FluentAssertions;
using Xunit;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Light.Serialization.Json.Tests.DeserializationTests
{
    public sealed class BinaryJsonTests : BaseJsonDeserializerTest
    {
        [Theory(DisplayName = "The deserialized must be able to read JSON from binary streams.")]
        [InlineData("32", 32)]
        [InlineData("55.5944", 55.5944)]
        [InlineData("false", false)]
        [InlineData("[1, 87, 555, -12]", new[] { 1, 87, 555, -12 })]
        public void BinaryJsonStreams<T>(string json, T expected)
        {
            var memoryStream = new MemoryStream();
            var binaryWriter = new BinaryWriter(memoryStream, Encoding.Default, true);
            binaryWriter.Write(json);
            binaryWriter.Dispose();
            memoryStream.Position = 0;
            var binaryReader = new BinaryReader(memoryStream);
            var deserializer = GetDeserializer();

            var actual = deserializer.Deserialize<T>(binaryReader);

            actual.ShouldBeEquivalentTo(expected);
        }
    }
}