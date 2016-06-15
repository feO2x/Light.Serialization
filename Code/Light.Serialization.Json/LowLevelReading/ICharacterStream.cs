namespace Light.Serialization.Json.LowLevelReading
{
    public interface ICharacterStream
    {
        char[] Buffer { get; }

        int CurrentIndex { get; }

        bool IsEndOfStream { get; }

        char CurrentCharacter { get; }

        int PinIndex();
        bool Advance();
    }
}