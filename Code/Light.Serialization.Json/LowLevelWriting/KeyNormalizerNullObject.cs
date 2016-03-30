namespace Light.Serialization.Json.LowLevelWriting
{
    /// <summary>
    ///     Reprensents a JSON key normalizer that does nothing.
    /// </summary>
    public sealed class KeyNormalizerNullObject : IJsonKeyNormalizer
    {
        /// <summary>
        /// Returns the specified key.
        /// </summary>
        public string Normalize(string key)
        {
            return key;
        }
    }
}