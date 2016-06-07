using System;
using Light.Serialization.Json.Tests.SampleTypes;
using Xunit;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Light.Serialization.Json.Tests.SerializationTests
{
    public sealed class CustomRuleTests : BaseJsonSerializerTest
    {
        [Fact(DisplayName = "Specific properties can be added to a blacklist so that the serializer must ignore them.")]
        public void CustomObjectIngoreProperty()
        {
            DisableAllMetadata();

            var dummyObject = CreateDummyObject();
            AddRule<DummyClass>(r => r.IgnoreProperty(o => o.PublicProperty));

            CompareJsonToExpected(dummyObject, "{\"publicField\":42}");
        }

        [Fact(DisplayName = "Specific fields can be added to a blacklist so that the serializer must ignore them.")]
        public void CustomObjectIgnoreField()
        {
            DisableAllMetadata();

            var dummyObject = CreateDummyObject();
            AddRule<DummyClass>(r => r.IgnoreField(o => o.PublicField));

            CompareJsonToExpected(dummyObject, "{\"publicProperty\":\"2016-02-09\"}");
        }

        [Fact(DisplayName = "All public properties and fields can be ignored with an empty white list.")]
        public void CustomObjectIgnoreAll()
        {
            DisableAllMetadata();

            var dummyObject = CreateDummyObject();
            AddRule<DummyClass>(r => r.IgnoreAll());

            CompareJsonToExpected(dummyObject, "{}");
        }

        [Fact(DisplayName = "Specific properties can be added to a white list that gets serialized only.")]
        public void CustomObjectIgnoreAllButProperty()
        {
            DisableAllMetadata();

            var dummyObject = CreateDummyObject();
            AddRule<DummyClass>(r => r.IgnoreAll().ButProperty(o => o.PublicProperty));

            CompareJsonToExpected(dummyObject, "{\"publicProperty\":\"2016-02-09\"}");
        }

        [Fact(DisplayName = "Specific fields can be added to a white list that gets serialized only.")]
        public void CustomObjectIgnoreAllButField()
        {
            DisableAllMetadata();

            var dummyObject = CreateDummyObject();
            AddRule<DummyClass>(r => r.IgnoreAll().ButField(o => o.PublicField));

            CompareJsonToExpected(dummyObject, "{\"publicField\":42}");
        }

        [Fact(DisplayName = "Specific properties can be added to a blacklist so that the serializer must ignore them.")]
        public void CustomObjectIngoreProperties()
        {
            DisableAllMetadata();

            var moreComplexDummyObject = MoreComplexDummyClass.CreateDefault();
            AddRule<MoreComplexDummyClass>(r => r.IgnoreProperty(o => o.PublicProperty)
                                                 .IgnoreProperty(o => o.PublicDoubleProperty));

            CompareJsonToExpected(moreComplexDummyObject, "{\"publicStringProperty\":\"works\",\"publicField\":42}");
        }

        [Fact(DisplayName = "Specific properties can be added to a white so that the serializer must serialize them.")]
        public void CustomObjectIngoreAllButProperties()
        {
            DisableAllMetadata();

            var moreComplexDummyObject = MoreComplexDummyClass.CreateDefault();
            AddRule<MoreComplexDummyClass>(r => r.IgnoreAll()
                                                 .ButProperty(o => o.PublicStringProperty)
                                                 .AndField(o => o.PublicField));

            CompareJsonToExpected(moreComplexDummyObject, "{\"publicStringProperty\":\"works\",\"publicField\":42}");
        }

        private static DummyClass CreateDummyObject()
        {
            return new DummyClass("foo", 42, new DateTime(2016, 2, 9));
        }

        public class DummyClass
        {
            // ReSharper disable once NotAccessedField.Local
            private string _privateField;

            public int PublicField;

            public DummyClass(string privateFieldValue, int publicFieldValue, DateTime publicPropertyValue)
            {
                _privateField = privateFieldValue;
                PublicField = publicFieldValue;
                PublicProperty = publicPropertyValue;
            }

            public DateTime PublicProperty { get; }
        }
    }
}