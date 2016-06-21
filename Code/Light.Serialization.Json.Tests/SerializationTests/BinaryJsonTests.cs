using System.IO;
using System.Text;
using FluentAssertions;
using Xunit;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Light.Serialization.Json.Tests.SerializationTests
{
    public sealed class BinaryJsonTests : BaseJsonSerializerTest
    {
        [Theory(DisplayName = "The serializer must be able to write JSON documents in binary format.")]
        [InlineData(1, "1")]
        [InlineData(null, "null")]
        [InlineData(42.25, "42.25")]
        [InlineData(true, "true")]
        [InlineData(new[] { 1, 2, 3 }, "[1,2,3]")]
        public void BinaryJson(object objectGraphRoot, string expectedJson)
        {
            DisableAllMetadata();

            var expectedBinary = GetBinaryFormat(expectedJson);
            var memoryStream = new MemoryStream();
            var binaryWriter = new BinaryWriter(memoryStream, Encoding.Default, true);
            var serializer = GetSerializer();

            serializer.Serialize(objectGraphRoot, binaryWriter);

            var actualBinary = GetBinaryFromMemoryStream(memoryStream);
            actualBinary.Should().BeEquivalentTo(expectedBinary);
        }

        private static byte[] GetBinaryFormat(string expectedJson)
        {
            var memoryStream = new MemoryStream();
            var binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write(expectedJson);
            binaryWriter.Flush();

            return GetBinaryFromMemoryStream(memoryStream);
        }

        private static byte[] GetBinaryFromMemoryStream(MemoryStream stream)
        {
            stream.Position = 0;
            return new BinaryReader(stream).ReadBytes((int) stream.Length);
        }
    }
}