using System;
using FluentAssertions;
using Light.Serialization.Json.ObjectMetadata;
using Light.Serialization.Json.PrimitiveTypeFormatters;
using Light.Serialization.Json.SerializationRules;

namespace Light.Serialization.Json.Tests
{
    public abstract class BaseJsonSerializerTest
    {
        private readonly JsonSerializerBuilder _jsonSerializerBuilder;

        protected BaseJsonSerializerTest()
        {
            _jsonSerializerBuilder = new JsonSerializerBuilder();
        }

        protected void CompareJsonToExpected<T>(T value, string expected)
        {
            var json = GetSerializedJson(value);

            json.Should().Be(expected);
        }

        protected void CompareHumanReadableJsonToExpected<T>(T value, string expected)
        {
            var json = GetSerializedHumanReadableJson(value);

            json.Should().Be(expected);
        }

        protected string GetSerializedJson<T>(T value)
        {
            var jsonSerializer = _jsonSerializerBuilder.Build();

            return jsonSerializer.Serialize(value);
        }

        protected string GetSerializedHumanReadableJson<T>(T value)
        {
            var jsonSerializer = _jsonSerializerBuilder.EnableHumanReadableJsonDocuments()
                                                       .Build();

            return jsonSerializer.Serialize(value);
        }

        protected void AddRule<T>(Action<Rule<T>> rule)
        {
            _jsonSerializerBuilder.WithRuleFor(rule);
        }

        protected void ReplaceTimeZoneInfoInDateTimeFormatter(TimeZoneInfo timeZoneInfo)
        {
            _jsonSerializerBuilder.ConfigurePrimitiveTypeFormatter<DateTimeFormatter>(f => f.TimeZoneInfo = timeZoneInfo);
        }

        protected void DisableObjectReferencePreservation()
        {
            _jsonSerializerBuilder.DisableObjectReferencePreservation();
        }

        protected void DisableTypeMetadata()
        {
            _jsonSerializerBuilder.DisableTypeMetadata();
        }

        protected void DisableAllMetadata()
        {
            DisableObjectReferencePreservation();
            DisableTypeMetadata();
        }

        protected void UseDomainFriendlyNames(Action<TypeNameToJsonNameScanner.IScanningOptions> options = null)
        {
            var domainFriendlyNameMapping = DomainFriendlyNameMapping.CreateWithDefaultTypeMappings();

            if (options != null)
                domainFriendlyNameMapping.ScanTypes(options);

            _jsonSerializerBuilder.WithTypeToNameMapping(domainFriendlyNameMapping);
        }
    }
}