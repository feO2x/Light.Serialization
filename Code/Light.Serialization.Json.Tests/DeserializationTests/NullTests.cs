using System;
using System.IO;
using FluentAssertions;
using Xunit;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Light.Serialization.Json.Tests.DeserializationTests
{
    public sealed class NullTests : BaseJsonDeserializerTest
    {
        [Theory(DisplayName = "The deserializer must be able to parse JSON null to .NET null and assign it to reference types.")]
        [InlineData(typeof(object))] // Normal class
        [InlineData(typeof(IComparable))] // Interface
        [InlineData(typeof(Stream))] // Abstract base class
        public void NullValuesAreParsedCorrectly(Type requestedType)
        {
            const string json = "null";
            var result = GetDeserializedJson(json, requestedType);
            result.Should().BeNull();
        }

        [Theory(DisplayName = "The deserializer must throw a JsonDocumentException when JSON null token is malformed.")]
        [InlineData("nll")]
        [InlineData("nlul")]
        [InlineData("nul")]
        public void ExceptionIsThrownWhenNullIsMisspelled(string json)
        {
            CheckDeserializerThrowsExceptionWithMessageContaining<object>(json, $"Cannot deserialize value {json} to {JsonSymbols.Null}.");
        }
    }
}