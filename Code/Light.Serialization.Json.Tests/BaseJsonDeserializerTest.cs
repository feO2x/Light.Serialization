﻿using System;
using FluentAssertions;
using Light.Serialization.Abstractions;
using Light.Serialization.Json.ObjectMetadata;

namespace Light.Serialization.Json.Tests
{
    public abstract class BaseJsonDeserializerTest
    {
        private readonly JsonDeserializerBuilder _builder = new JsonDeserializerBuilder();

        public void CompareDeserializedJsonToExpected<T>(string json, T expected)
        {
            var actual = GetDeserializedJson<T>(json);

            actual.Should().Be(expected);
        }

        public T GetDeserializedJson<T>(string json)
        {
            var testTarget = _builder.Build();
            return testTarget.Deserialize<T>(json);
        }

        public object GetDeserializedJson(string json, Type requestedType)
        {
            var testTarget = _builder.Build();
            return testTarget.Deserialize(json, requestedType);
        }

        public void ConfigureDefaultDomainFriendlyNames(Action<TypeNameToJsonNameScanner.IScanningOptions> configureAdditionalTypes = null)
        {
            var domainFriendlyNameMapping = DomainFriendlyNameMapping.CreateWithDefaultTypeMappings();

            if (configureAdditionalTypes != null)
                domainFriendlyNameMapping.ScanTypes(configureAdditionalTypes);

            _builder.WithNameToTypeMapping(domainFriendlyNameMapping);
        }

        public void CheckDeserializerThrowsExceptionWithMessageContaining<T>(string json, string partOfExceptionMessage)
        {
            var testTarget = _builder.Build();

            Action act = () => testTarget.Deserialize<T>(json);

            act.ShouldThrow<DeserializationException>().And.Message.Should().Contain(partOfExceptionMessage);
        }

        public void CheckDeserializerThrowsExceptionWithMessage<T>(string json, string exceptionMessage)
        {
            var testTarget = _builder.Build();

            Action act = () => testTarget.Deserialize<T>(json);

            act.ShouldThrow<DeserializationException>().And.Message.Should().Be(exceptionMessage);
        }
    }
}