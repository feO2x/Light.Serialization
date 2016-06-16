using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using Light.Serialization.Abstractions;
using Light.Serialization.Json.ObjectMetadata;
using Light.Serialization.Json.Tests.SampleTypes;
using Xunit;

namespace Light.Serialization.Json.Tests.DeserializationTests
{
    public sealed class StreamingTests
    {
        [Fact(DisplayName = "The deserializer must be able to parse large documents using streaming.")]
        public void LargeDocument()
        {
            var persons = new List<DummyPerson>();
            for (var i = 0; i < 10000; i++)
            {
                persons.Add(new DummyPerson { Name = $"Name {i}", Age = i });
            }

            var domainFriendlyNameMapping = DomainFriendlyNameMapping.CreateWithDefaultTypeMappings()
                                                                     .ScanTypes(o => o.UseTypes(typeof(DummyPerson)));

            var json = new JsonSerializerBuilder().WithTypeToNameMapping(domainFriendlyNameMapping)
                                                  .Build()
                                                  .Serialize(persons);
            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream, Encoding.Unicode);
            streamWriter.Write(json);
            streamWriter.Flush();

            memoryStream.Position = 0;

            var textReader = new StreamReader(memoryStream, Encoding.Unicode);

            var deserializedPersons = new JsonDeserializerBuilder().WithNameToTypeMapping(domainFriendlyNameMapping)
                                                                   .Build()
                                                                   .Deserialize<List<DummyPerson>>(textReader);

            deserializedPersons.ShouldAllBeEquivalentTo(persons);
        }

        [Fact(DisplayName = "The deserializer must throw a DeserializationException when a token in the JSON document exceeeds the buffer size of the TextReaderAdapter.")]
        public void TokenLengthExceedsBufferLength()
        {
            const string json = "{ \"text\": \"This JSON string is extremely long and exceeds the smallest buffer size of 32 characters easily.\"}";
            var memoryStream = ToMemoryStream(json);
            var deserializer = new JsonDeserializerBuilder().ConfigureDefaultReaderFactory(factory => factory.BufferSizeForStreaming = 32)
                                                            .Build();
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