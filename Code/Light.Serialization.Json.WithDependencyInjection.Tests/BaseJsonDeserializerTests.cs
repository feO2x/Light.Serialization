using System;
using FluentAssertions;
using Light.DependencyInjection;
using Light.Serialization.Abstractions;
using Light.Serialization.Json.LowLevelReading;
using Light.Serialization.Json.ObjectMetadata;
using Light.Serialization.Json.WithDependencyInjection;

// ReSharper disable once CheckNamespace
namespace Light.Serialization.Json.Tests
{
    public abstract class BaseJsonDeserializerTest
    {
        protected DependencyInjectionContainer Container;

        protected BaseJsonDeserializerTest()
        {
            Container = new DependencyInjectionContainer().RegisterDefaultDeserializationTypes()
                                                          .RegisterDefaultCollectionTypes()
                                                          .RegisterDefaultDeserializationTypes()
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

        public void UseDomainFriendlyNames(Action<TypeNameToJsonNameScanner.IScanningOptions> configureAdditionalTypes = null)
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

        public void ConfigureReaderFactory(Action<JsonReaderFactory> configureFactory)
        {
            configureFactory(Container.Resolve<JsonReaderFactory>());
        }

        public JsonDeserializer GetDeserializer()
        {
            return Container.Resolve<JsonDeserializer>();
        }

        public void IgnoreTypesInMetadataSection()
        {
            Container.Resolve<ArrayMetadataParser>().IsIgnoringMetadataTypeInformation = true;
            Container.Resolve<ObjectMetadataParser>().IsIgnoringMetadataTypeInformation = true;
        }
    }
}