using System;
using System.Globalization;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents an <see cref="IJsonTokenParser" /> that can deserialize JSON strings in the ISO 8601 format to .NET <see cref="DateTime" /> instances.
    /// </summary>
    public sealed class DateTimeParser : BaseIso8601DateTimeParser<DateTime>, IJsonStringToPrimitiveParser
    {
        /// <summary>
        ///     Gets or sets the <see cref="DateTimeKind" /> that is used for short date values (without the time component).
        ///     This value defaults to <see cref="DateTimeKind.Utc" />.
        /// </summary>
        public DateTimeKind DefaultDateTimeKind = DateTimeKind.Utc;

        /// <summary>
        ///     Gets the value indicating that this parser can be cached.
        /// </summary>
        public bool CanBeCached => true;

        /// <summary>
        ///     Parses the given JSON string in ISO 8601 format to a .NET <see cref="DateTime" /> value.
        /// </summary>
        public ParseResult ParseValue(JsonDeserializationContext context)
        {
            return ParseResult.FromParsedValue(Parse(context.Token));
        }

        /// <summary>
        ///     Tries to parse the given JSON string to a .NET <see cref="DateTime" /> value.
        /// </summary>
        public JsonStringParseResult TryParse(JsonDeserializationContext context, string deserializedString)
        {
            DateTime dateTime;
            return DateTime.TryParse(deserializedString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dateTime) ? new JsonStringParseResult(true, dateTime) : new JsonStringParseResult(false);
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