using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
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
    }
}