using System.IO;
using Light.GuardClauses;
using Light.Serialization.Json.FrameworkExtensions;

namespace Light.Serialization.Json.LowLevelWriting
{
    public sealed class JsonWriter : IJsonWriter
    {
        private readonly IJsonWhitespaceFormatter _whitespaceFormatter;
        private readonly IJsonKeyNormalizer _jsonKeyNormalizer;
        private readonly TextWriter _textWriter;

        public JsonWriter(TextWriter textWriter, IJsonWhitespaceFormatter whitespaceFormatter, IJsonKeyNormalizer jsonKeyNormalizer)
        {
            textWriter.MustNotBeNull(nameof(textWriter));
            whitespaceFormatter.MustNotBeNull(nameof(whitespaceFormatter));
            jsonKeyNormalizer.MustNotBeNull(nameof(jsonKeyNormalizer));

            _textWriter = textWriter;
            _whitespaceFormatter = whitespaceFormatter;
            _jsonKeyNormalizer = jsonKeyNormalizer;
        }

        public void BeginArray()
        {
            _textWriter.Write(JsonSymbols.BeginOfArray);
            _whitespaceFormatter.NewlineAndIncreaseIndent(this);
        }

        public void EndArray()
        {
            _whitespaceFormatter.NewlineAndDecreaseIndent(this);
            _textWriter.Write(JsonSymbols.EndOfArray);
        }

        public void BeginObject()
        {
            _textWriter.Write(JsonSymbols.BeginOfObject);
            _whitespaceFormatter.NewlineAndIncreaseIndent(this);
        }

        public void EndObject()
        {
            _whitespaceFormatter.NewlineAndDecreaseIndent(this);
            _textWriter.Write(JsonSymbols.EndOfObject);
        }

        public void WriteKey(string key, bool shouldNormalizeKey)
        {
            if (shouldNormalizeKey)
                key = _jsonKeyNormalizer.Normalize(key);

            if (key.IsSurroundedByQuotationMarks() == false)
                key = key.SurroundWithQuotationMarks();

            _textWriter.Write(key);
            _textWriter.Write(JsonSymbols.PairDelimiter);
            _whitespaceFormatter.InsertWhitespaceBetweenKeyAndValue(this);
        }

        public void WriteDelimiter()
        {
            _textWriter.Write(JsonSymbols.ValueDelimiter);
            _whitespaceFormatter.Newline(this);
        }

        public void WritePrimitiveValue(string @string)
        {
            _textWriter.Write(@string);
        }

        public void WriteNull()
        {
            _textWriter.Write(JsonSymbols.Null);
        }
    }
}