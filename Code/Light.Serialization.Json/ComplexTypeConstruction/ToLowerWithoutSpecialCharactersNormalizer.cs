using Light.Serialization.Json.FrameworkExtensions;

namespace Light.Serialization.Json.ComplexTypeConstruction
{
    public sealed class ToLowerWithoutSpecialCharactersNormalizer : IInjectableValueNameNormalizer
    {
        public string Normalize(string name)
        {
            return name.ToLowerAndRemoveAllSpecialCharacters();
        }
    }
}