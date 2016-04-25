using System;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents a JSON token parser that can deserialize JSON strings in the ISO 8601 format to .NET date times.
    /// </summary>
    public sealed class DateTimeParser : BaseIso8601DateTimeParser<DateTime>, IJsonStringToPrimitiveParser
    {
        /// <summary>
        ///     Gets or sets the date time kind that is used for short date values (without time component).
        /// </summary>
        public DateTimeKind DefaultDateTimeKind = DateTimeKind.Utc;

        /// <summary>
        ///     Gets the value indicating whether this parser can be cached.
        /// </summary>
        public bool CanBeCached => true;

        /// <summary>
        ///     Parses the given JSON string in ISO 8601 format to a .NET DateTime value.
        /// </summary>
        public object ParseValue(JsonDeserializationContext context)
        {
            return Parse(context.Token);
        }

        /// <summary>
        ///     Tries to parse the given JSON string to a .NET DateTime.
        /// </summary>
        public PrimitiveParseResult TryParse(JsonToken token)
        {
            try
            {
                return new PrimitiveParseResult(true, Parse(token));
            }
            catch (JsonDocumentException)
            {
                return new PrimitiveParseResult(false);
            }
        }

        private DateTime Parse(JsonToken token)
        {
            int year,
                month,
                day = 1,
                hour = 0,
                minute = 0,
                second = 0,
                millisecond = 0;

            var currentIndex = 1;
            var kind = DefaultDateTimeKind;

            year = ReadNumber(4, token, ref currentIndex);
            ExpectCharacter('-', token, ref currentIndex);
            month = ReadNumber(2, token, ref currentIndex);
            if (IsEndOfToken(currentIndex, token.Length))
                goto CreateDateTime;

            ExpectCharacter('-', token, ref currentIndex);
            day = ReadNumber(2, token, ref currentIndex);
            if (IsEndOfToken(currentIndex, token.Length))
                goto CreateDateTime;

            ExpectCharacter('T', token, ref currentIndex);
            kind = DateTimeKind.Unspecified;
            hour = ReadNumber(2, token, ref currentIndex);
            if (IsEndOfToken(currentIndex, token.Length))
                goto CreateDateTime;
            if (IsTimeZoneIndicator(token, ref currentIndex))
                goto CheckTimeZoneIndicator;

            ExpectCharacter(':', token, ref currentIndex);
            minute = ReadNumber(2, token, ref currentIndex);
            if (IsEndOfToken(currentIndex, token.Length))
                goto CreateDateTime;
            if (IsTimeZoneIndicator(token, ref currentIndex))
                goto CheckTimeZoneIndicator;

            ExpectCharacter(':', token, ref currentIndex);
            second = ReadNumber(2, token, ref currentIndex);
            if (IsEndOfToken(currentIndex, token.Length))
                goto CreateDateTime;
            if (IsTimeZoneIndicator(token, ref currentIndex))
                goto CheckTimeZoneIndicator;

            ExpectCharacter('.', token, ref currentIndex);
            millisecond = ReadNumber(3, token, ref currentIndex);
            if (IsEndOfToken(currentIndex, token.Length))
                goto CreateDateTime;

            CheckTimeZoneIndicator:
            var character = token[currentIndex++];
            if (character == 'Z')
                kind = DateTimeKind.Utc;
            else if (character == '+' || character == '-')
            {
                kind = DateTimeKind.Local;
                ReadNumber(2, token, ref currentIndex);
                if (IsEndOfToken(currentIndex, token.Length))
                    goto CreateDateTime;

                ExpectCharacter(':', token, ref currentIndex);
                ReadNumber(2, token, ref currentIndex);
            }

            if (IsEndOfToken(currentIndex, token.Length) == false)
                throw CreateException(token);

            CreateDateTime:
            try
            {
                return new DateTime(year, month, day, hour, minute, second, millisecond, kind);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw CreateException(token, ex);
            }
        }
    }
}