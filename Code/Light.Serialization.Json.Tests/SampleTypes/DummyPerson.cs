namespace Light.Serialization.Json.Tests.SampleTypes
{
    public interface IDummyPerson
    {
        string Name { get; set; }
        int Age { get; set; }
    }

    public class DummyPerson : IDummyPerson 
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}