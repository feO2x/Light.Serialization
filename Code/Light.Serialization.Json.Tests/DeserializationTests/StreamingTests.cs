using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using Light.Serialization.Json.Tests.SampleTypes;
using Xunit;

namespace Light.Serialization.Json.Tests.DeserializationTests
{
    public sealed class StreamingTests : BaseJsonDeserializerTest
    {
        [Fact]
        public void LargeDocument()
        {
            var persons = new List<DummyPerson>();
            for (var i = 0; i < 10000; i++)
            {
                persons.Add(new DummyPerson { Name = Guid.NewGuid().ToString(), Age = i });
            }

            var json = new JsonSerializerBuilder().Build().Serialize(persons);
            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream, Encoding.Unicode);
            streamWriter.Write(json);
            streamWriter.Flush();

            memoryStream.Position = 0;

            var textReader = new StreamReader(memoryStream, Encoding.Unicode);

            var deserializedPersons = GetDeserializedJsonFromStream<List<DummyPerson>>(textReader);

            deserializedPersons.Should().BeEquivalentTo(persons);
        }
    }
}