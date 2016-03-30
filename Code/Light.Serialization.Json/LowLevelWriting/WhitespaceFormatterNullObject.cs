namespace Light.Serialization.Json.LowLevelWriting
{
    /// <summary>
    ///     Represents a JSON whitespace formatter that does nothing. Use an instance of this class if you want to keep the JSON document as small as possible.
    /// </summary>
    public sealed class WhitespaceFormatterNullObject : IJsonWhitespaceFormatter
    {
        private int _currentIndentationLevel;
        public string IdentCharacters { get; set; }

        public int CurrentIndentationLevel => _currentIndentationLevel;

        public void NewlineAndIncreaseIndent(IJsonWriter writer)
        {
            _currentIndentationLevel++;
        }

        public void Newline(IJsonWriter writer) { }

        public void NewlineAndDecreaseIndent(IJsonWriter writer)
        {
            _currentIndentationLevel--;
        }

        public void InsertWhitespaceBetweenKeyAndValue(IJsonWriter writer) { }

        public void ResetIndentationLevel()
        {
            _currentIndentationLevel = 0;
        }
    }
}