using System;
using Light.DependencyInjection;
using Light.Serialization.Abstractions;
using Light.Serialization.Json.ObjectMetadata;
using Light.Serialization.Json.PrimitiveTypeFormatters;
using Light.Serialization.Json.SerializationRules;
using Light.Serialization.Json.WithDependencyInjection;
using FluentAssertions;

// ReSharper disable once CheckNamespace

namespace Light.Serialization.Json.Tests
{
    public abstract class BaseJsonSerializerTest
    {
        private readonly DependencyInjectionContainer _container;

        protected BaseJsonSerializerTest()
        {
            _container = new DependencyInjectionContainer().RegisterDefaultSerializationTypes();
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
            var jsonSerializer = _container.Resolve<ISerializer>();

            return jsonSerializer.Serialize(value);
        }

        protected string GetSerializedHumanReadableJson<T>(T value)
        {
            return _container.UseIndentingWhitespaceFormatterForSerialization()
                             .Resolve<ISerializer>()
                             .Serialize(value);
        }

        protected void AddRule<T>(Action<Rule<T>> rule)
        {
            _container.WithSerializationRuleFor(rule);
        }

        protected void ReplaceTimeZoneInfoInDateTimeFormatter(TimeZoneInfo timeZoneInfo)
        {
            _container.Resolve<DateTimeFormatter>().TimeZoneInfo = timeZoneInfo;
        }

        protected void DisableObjectReferencePreservation()
        {
            _container.DisableObjectReferencePreservation();
        }

        protected void DisableTypeMetadata()
        {
            _container.DisableTypeMetadata();
        }

        protected void DisableAllMetadata()
        {
            DisableObjectReferencePreservation();
            DisableTypeMetadata();
        }

        protected void UseDomainFriendlyNames(Action<TypeNameToJsonNameScanner.IScanningOptions> options = null)
        {
            if (options == null)
                _container.UseDomainFriendlyNames(DomainFriendlyNameMapping.CreateWithDefaultTypeMappings());
            else
                _container.UseDomainFriendlyNames(options);
        }

        protected JsonSerializer GetSerializer()
        {
            return _container.Resolve<JsonSerializer>();
        }
    }
}