using System;

namespace Light.Serialization.Json.Tests.SampleTypes
{
    public class MoreComplexDummyClass
    {
        public static int PublicStaticField = 0;
        // ReSharper disable once NotAccessedField.Local
        private string _privateField;
        public int PublicField;

        public MoreComplexDummyClass(string privateFieldValue, int publicFieldValue, DateTime publicPropertyValue, double publicDoubleProperty, string publicStringProperty)
        {
            _privateField = privateFieldValue;
            PublicField = publicFieldValue;
            PublicProperty = publicPropertyValue;
            PublicDoubleProperty = publicDoubleProperty;
            PublicStringProperty = publicStringProperty;
        }

        public DateTime PublicProperty { get; }
        public double PublicDoubleProperty { get; set; }
        public string PublicStringProperty { get; set; }

        public static MoreComplexDummyClass CreateDefault()
        {
            return new MoreComplexDummyClass("foo", 42, new DateTime(2016, 2, 9), 42.75, "works");
        }
    }
}