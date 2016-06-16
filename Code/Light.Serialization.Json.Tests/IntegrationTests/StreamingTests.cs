using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Light.Serialization.Json.ObjectMetadata;
using Light.Serialization.Json.Tests.SampleTypes;
using Xunit;

namespace Light.Serialization.Json.Tests.IntegrationTests
{
    public sealed class StreamingTests
    {
        [Fact(DisplayName = "The deserializer must be able to write and parse large documents using streaming.")]
        public void LargeDocument()
        {
            var persons = new List<DummyPerson>();
            for (var i = 0; i < 10000; i++)
            {
                persons.Add(new DummyPerson { Name = $"Name {i}", Age = i });
            }

            var domainFriendlyNameMapping = DomainFriendlyNameMapping.CreateWithDefaultTypeMappings()
                                                                     .ScanTypes(o => o.UseTypes(typeof(DummyPerson)));

            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream);

            new JsonSerializerBuilder().WithTypeToNameMapping(domainFriendlyNameMapping)
                                       .Build()
                                       .Serialize(persons, streamWriter);

            streamWriter.Flush();
            memoryStream.Position = 0;

            var textReader = new StreamReader(memoryStream);

            var deserializedPersons = new JsonDeserializerBuilder().WithNameToTypeMapping(domainFriendlyNameMapping)
                                                                   .Build()
                                                                   .Deserialize<List<DummyPerson>>(textReader);

            deserializedPersons.ShouldAllBeEquivalentTo(persons);
        }
    }
}