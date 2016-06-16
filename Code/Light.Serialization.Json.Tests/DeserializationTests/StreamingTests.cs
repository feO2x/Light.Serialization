using System;
using System.IO;
using FluentAssertions;
using Light.Serialization.Abstractions;
using Xunit;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Light.Serialization.Json.Tests.DeserializationTests
{
    public sealed class StreamingTests : BaseJsonDeserializerTest
    {
        [Fact(DisplayName = "The deserializer must throw a DeserializationException when a token in the JSON document exceeeds the buffer size of the TextReaderAdapter.")]
        public void TokenLengthExceedsBufferLength()
        {
            const string json = "{ \"text\": \"This JSON string is extremely long and exceeds the smallest buffer size of 32 characters easily.\"}";
            var memoryStream = ToMemoryStream(json);

            ConfigureReaderFactory(factory => factory.BufferSizeForStreaming = 32);
            var deserializer = GetDeserializer();


            Action act = () => deserializer.Deserialize<TextDto>(new StreamReader(memoryStream));

            act.ShouldThrow<DeserializationException>()
               .And.Message.Should().Contain("The buffer size for deseserializing the JSON document is too small (the pinned index was reached).");
        }

        [Fact(DisplayName = "The deserializer must be able to parse JSON documents that are smaller than the internal buffer size.")]
        public void JsonDocumentSmallerThanBuffer()
        {
            const string json = "{ \"text\": \"This JSON string is smaller than the default buffer size of 2K characters.\"}";
            var memoryStream = ToMemoryStream(json);
            var deserializer = new JsonDeserializerBuilder().Build();

            var deserializedObject = deserializer.Deserialize<TextDto>(new StreamReader(memoryStream));

            var expected = new TextDto { Text = "This JSON string is smaller than the default buffer size of 2K characters." };
            deserializedObject.ShouldBeEquivalentTo(expected);
        }

        private static MemoryStream ToMemoryStream(string json)
        {
            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream);
            streamWriter.Write(json);
            streamWriter.Flush();
            memoryStream.Position = 0;
            return memoryStream;
        }

        public class TextDto
        {
            public string Text { get; set; }
        }
    }
}