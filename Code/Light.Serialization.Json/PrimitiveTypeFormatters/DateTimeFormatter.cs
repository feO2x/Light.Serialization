using System;
using System.Text;
using Light.Serialization.Json.FrameworkExtensions;

namespace Light.Serialization.Json.PrimitiveTypeFormatters
{
    /// <summary>
    ///     Represents a Primitive Type Formatter that serializes .NET DateTime instances to JSON string.
    ///     The format of the date time corresponds to ISO 8601 (https://en.wikipedia.org/wiki/ISO_8601).
    /// </summary>
    public sealed class DateTimeFormatter : BasePrimitiveTypeFormatter<DateTime>, IPrimitiveTypeFormatter
    {
        private TimeZoneInfo _timeZoneInfo = TimeZoneInfo.Local;

        /// <summary>
        ///     Creates a new instance of <see cref="DateTimeFormatter" />.
        /// </summary>
        public DateTimeFormatter() : base(false) { }

        /// <summary>
        ///     Gets or sets the time zone info that is used to calculate offset values for local date times.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        public TimeZoneInfo TimeZoneInfo
        {
            get { return _timeZoneInfo; }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                _timeZoneInfo = value;
            }
        }

        /// <summary>
        ///     Serializes the specified date time to a JSON string that conforms to ISO 8601.
        /// </summary>
        /// <param name="primitiveValue">The date time to be serialized.</param>
        /// <returns>The JSON string conforming to ISO 8601.</returns>
        /// <exception cref="InvalidCastException">Thrown when the specified <paramref name="primitiveValue" /> is not of type DateTime.</exception>
        public string FormatPrimitiveType(object primitiveValue)
        {
            var dateTime = (DateTime) primitiveValue;

            // Append the date in any case
            var stringBuilder = new StringBuilder(23);
            stringBuilder.AppendFormat("\"{0:D4}-{1:D2}-{2:D2}", dateTime.Year, dateTime.Month, dateTime.Day);

            bool isHourZero = dateTime.Hour == 0,
                 isMinuteZero = dateTime.Minute == 0,
                 isSecondZero = dateTime.Second == 0,
                 isMillisecondZero = dateTime.Millisecond == 0;

            // Omit the time part if possible
            if (isHourZero && isMinuteZero && isSecondZero && isMillisecondZero)
                return stringBuilder.CompleteJsonStringWithQuotationMark();

            // Otherwise append hour and minute at least
            stringBuilder.Append('T');
            stringBuilder.AppendFormat("{0:D2}:{1:D2}", dateTime.Hour, dateTime.Minute);

            // Append second and milliseconds if necessary
            if (isSecondZero == false || isMillisecondZero == false)
            {
                stringBuilder.Append(':');
                stringBuilder.Append(dateTime.Second.ToString("D2"));
                if (isMillisecondZero == false)
                {
                    stringBuilder.Append('.');
                    stringBuilder.Append(dateTime.Millisecond.ToString("D3"));
                }
            }

            // Append the offset
            switch (dateTime.Kind)
            {
                // UTC just has a Z appended
                case DateTimeKind.Utc:
                    stringBuilder.Append('Z');
                    break;
                // For local DateTimes, the offset is appended
                case DateTimeKind.Local:
                    var offset = _timeZoneInfo.GetUtcOffset(dateTime);
                    if (offset.Hours >= 0)
                        stringBuilder.Append('+');
                    stringBuilder.Append(offset.Hours.ToString("D2"));
                    if (offset.Minutes != 0)
                    {
                        stringBuilder.Append(':');
                        stringBuilder.Append(offset.Minutes.ToString("D2"));
                    }
                    break;
                // DateTimes with kind "Unspecified" have no extension
                case DateTimeKind.Unspecified:
                    break;
            }

            return stringBuilder.CompleteJsonStringWithQuotationMark();
        }
    }
}