using System;
using FluentAssertions;
using Light.Serialization.Abstractions;
using Light.Serialization.Json.PrimitiveTypeFormatters;
using Light.Serialization.Json.SerializationRules;
using Light.Serialization.Json.Unity;
using Microsoft.Practices.Unity;

// ReSharper disable once CheckNamespace

namespace Light.Serialization.Tests
{
    public abstract class BaseJsonSerializerTest
    {
        protected IUnityContainer Container;

        protected BaseJsonSerializerTest()
        {
            Container = new UnityContainer().RegisterDefaultSerializationTypes();
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
            var jsonSerializer = Container.Resolve<ISerializer>();

            return jsonSerializer.Serialize(value);
        }

        protected string GetSerializedHumanReadableJson<T>(T value)
        {
            Container.RegisterIndentingWhitespaceformatter();
            var jsonSerializer = Container.Resolve<ISerializer>();

            return jsonSerializer.Serialize(value);
        }

        protected void AddRule<T>(Action<Rule<T>> rule)
        {
            Container.WithSerializationRuleFor(rule);
        }

        protected void ReplaceTimeZoneInfoInDateTimeFormatter(TimeZoneInfo timeZoneInfo)
        {
            Container.RegisterType<IPrimitiveTypeFormatter, DateTimeFormatter>(typeof (DateTimeFormatter).Name,
                                                                               new ContainerControlledLifetimeManager(), new InjectionFactory(c => new DateTimeFormatter
                                                                                                                                                   {
                                                                                                                                                       TimeZoneInfo = timeZoneInfo
                                                                                                                                                   }));
        }
    }
}