namespace Light.Serialization.Json.LowLevelReading
{
    /// <summary>
    ///     Represents the abstraction of exchanging the message of an existing exception object.
    ///     This is done in the <see cref="JsonDeserializer" /> to add line number and position information
    ///     of the erroneous token.
    /// </summary>
    public interface IExchangeExceptionMessage
    {
        void ExchangeMessage(string message);
    }
}