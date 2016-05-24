using System;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents a JSON Token Parser that deserializes JSON strings in ISO 8601 format to .NET date time offsets.
    /// </summary>
    public sealed class DateTimeOffsetParser : BaseIso8601DateTimeParser<DateTimeOffset>, IJsonStringToPrimitiveParser
    {
        /// <summary>
        ///     Gets or sets the default offset that is used when a ISO 8601 date time has no time part.
        ///     This value defaults to TimeSpan.Zero.
        /// </summary>
        public TimeSpan DefaultOffset = TimeSpan.Zero;

        /// <summary>
        ///     Gets the value indicating that this parser can be cached.
        /// </summary>
        public bool CanBeCached => true;

        /// <summary>
        ///     Parses the given JSON string in ISO 8601 format to a .NET DateTimeOffset value.
        /// </summary>
        public ParseResult ParseValue(JsonDeserializationContext context)
        {
            return ParseResult.FromParsedValue(Parse(context.Token));
        }

        /// <summary>
        ///     Tries to parse the given JSON token as a .NET DateTimeOffset value.
        /// </summary>
        public JsonStringParseResult TryParse(JsonDeserializationContext context, string deserializedString)
        {
            DateTimeOffset dateTimeOffset;
            return DateTimeOffset.TryParse(deserializedString, out dateTimeOffset) ? new JsonStringParseResult(true, dateTimeOffset) : new JsonStringParseResult(false);
        }

        private DateTimeOffset Parse(JsonToken token)
        {
            int year,
                month,
                day = 1,
                hour = 0,
                minute = 0,
                second = 0,
                millisecond = 0;
            var offset = DefaultOffset;

            var currentIndex = 1;
            year = ReadNumber(4, token, ref currentIndex);
            ExpectCharacter('-', token, ref currentIndex);
            month = ReadNumber(2, token, ref currentIndex);
            if (IsEndOfToken(currentIndex, token.Length))
                goto CreateDateTimeOffset;

            ExpectCharacter('-', token, ref currentIndex);
            day = ReadNumber(2, token, ref currentIndex);
            if (IsEndOfToken(currentIndex, token.Length))
                goto CreateDateTimeOffset;

            ExpectCharacter('T', token, ref currentIndex);
            hour = ReadNumber(2, token, ref currentIndex);
            if (IsEndOfToken(currentIndex, token.Length))
                goto CreateDateTimeOffset;
            if (IsTimeZoneIndicator(token, ref currentIndex))
                goto CheckTimeZoneIndicator;

            ExpectCharacter(':', token, ref currentIndex);
            minute = ReadNumber(2, token, ref currentIndex);
            if (IsEndOfToken(currentIndex, token.Length))
                goto CreateDateTimeOffset;
            if (IsTimeZoneIndicator(token, ref currentIndex))
                goto CheckTimeZoneIndicator;

            ExpectCharacter(':', token, ref currentIndex);
            second = ReadNumber(2, token, ref currentIndex);
            if (IsEndOfToken(currentIndex, token.Length))
                goto CreateDateTimeOffset;
            if (IsTimeZoneIndicator(token, ref currentIndex))
                goto CheckTimeZoneIndicator;

            ExpectCharacter('.', token, ref currentIndex);
            millisecond = ReadNumber(3, token, ref currentIndex);
            if (IsEndOfToken(currentIndex, token.Length))
                goto CreateDateTimeOffset;

            CheckTimeZoneIndicator:
            var character = token[currentIndex++];
            if (character == 'Z')
                offset = TimeSpan.Zero;
            else if (character == '+' || character == '-')
            {
                var hourOffset = ReadNumber(2, token, ref currentIndex);
                if (IsEndOfToken(currentIndex, token.Length))
                {
                    offset = TimeSpan.FromHours(character == '+' ? hourOffset : -hourOffset);
                    goto CreateDateTimeOffset;
                }

                ExpectCharacter(':', token, ref currentIndex);
                var minuteOffset = ReadNumber(2, token, ref currentIndex);
                if (character == '-')
                {
                    hourOffset = -hourOffset;
                    minuteOffset = -minuteOffset;
                }
                offset = new TimeSpan(hourOffset, minuteOffset, 0);
            }
            else
                throw CreateException(token);

            if (IsEndOfToken(currentIndex, token.Length) == false)
                throw CreateException(token);

            CreateDateTimeOffset:
            try
            {
                return new DateTimeOffset(year, month, day, hour, minute, second, millisecond, offset);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw CreateException(token, ex);
            }
        }
    }
}