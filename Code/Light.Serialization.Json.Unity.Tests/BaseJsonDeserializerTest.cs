using System;
using FluentAssertions;
using Light.Serialization.Abstractions;
using Light.Serialization.Json.ObjectMetadata;
using Light.Serialization.Json.Unity;
using Microsoft.Practices.Unity;

// ReSharper disable once CheckNamespace

namespace Light.Serialization.Json.Tests
{
    public abstract class BaseJsonDeserializerTest
    {
        protected IUnityContainer Container;

        protected BaseJsonDeserializerTest()
        {
            Container = new UnityContainer().RegisterDefaultDeserializationTypes()
                                            .RegisterDefaultCollectionTypes()
                                            .RegisterDefaultDictionaryTypes()
                                            .RegisterDefaultSetTypes();
        }

        public void CompareDeserializedJsonToExpected<T>(string json, T expected)
        {
            var actual = GetDeserializedJson<T>(json);

            actual.Should().Be(expected);
        }

        public T GetDeserializedJson<T>(string json)
        {
            var testTarget = Container.Resolve<IDeserializer>();

            return testTarget.Deserialize<T>(json);
        }

        public object GetDeserializedJson(string json, Type requestedType)
        {
            var testTarget = Container.Resolve<IDeserializer>();

            return testTarget.Deserialize(json, requestedType);
        }

        public void ConfigureDefaultDomainFriendlyNames(Action<TypeNameToJsonNameScanner.IScanningOptions> configureAdditionalTypes = null)
        {
            var domainFriendlyNameMapping = DomainFriendlyNameMapping.CreateWithDefaultTypeMappings();

            if (configureAdditionalTypes != null)
                domainFriendlyNameMapping.ScanTypes(configureAdditionalTypes);

            Container.UseDomainFriendlyNames(domainFriendlyNameMapping);
        }

        public void CheckDeserializerThrowsExceptionWithMessageContaining<T>(string json, string partOfExceptionMessage)
        {
            var testTarget = Container.Resolve<IDeserializer>();

            Action act = () => testTarget.Deserialize<T>(json);

            act.ShouldThrow<DeserializationException>().And.Message.Should().Contain(partOfExceptionMessage);
        }

        public void CheckDeserializerThrowsExceptionWithMessage<T>(string json, string exceptionMessage)
        {
            var testTarget = Container.Resolve<IDeserializer>();

            Action act = () => testTarget.Deserialize<T>(json);

            act.ShouldThrow<DeserializationException>().And.Message.Should().Be(exceptionMessage);
        }
    }
}