namespace Light.Serialization.Json.LowLevelReading
{
    /// <summary>
    ///     Represents the different values that a JSON token can have.
    /// </summary>
    public enum JsonTokenType
    {
        String,
        IntegerNumber,
        FloatingPointNumber,
        BeginOfObject,
        BeginOfArray,
        EndOfObject,
        EndOfArray,
        ValueDelimiter,
        PairDelimiter,
        True,
        False,
        Null,
        EndOfDocument,
        Error
    }
}