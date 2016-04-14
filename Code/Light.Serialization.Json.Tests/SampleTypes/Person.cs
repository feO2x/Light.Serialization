using Light.GuardClauses;

namespace Light.Serialization.Json.Tests.SampleTypes
{
    public sealed class Person
    {
        private string _firstName;
        private string _lastName;
        public int Age;


        public Person(string firstName, string lastName, int age)
        {
            _firstName = firstName;
            _lastName = lastName;
            Age = age;
        }

        public string FirstName
        {
            get { return _firstName; }
            set
            {
                value.MustNotBeNullOrWhiteSpace(nameof(value));
                _firstName = value;
            }
        }

        public string LastName
        {
            get { return _lastName; }
            set
            {
                value.MustNotBeNullOrWhiteSpace(nameof(value));
                _lastName = value;
            }
        }
    }
}