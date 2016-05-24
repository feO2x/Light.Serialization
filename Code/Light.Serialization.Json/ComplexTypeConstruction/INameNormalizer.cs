namespace Light.Serialization.Json.ComplexTypeConstruction
{
    /// <summary>
    ///     Represents the abstraction of an object that can be used to normalize
    ///     the name of a type member, constructor parameter, and keys in complex JSON objects
    ///     to check them for equality.
    /// </summary>
    public interface INameNormalizer
    {
        /// <summary>
        ///     Normalizes the specified string.
        /// </summary>
        string Normalize(string name);
    }
}