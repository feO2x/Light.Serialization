using System;
using System.Text;
using Light.GuardClauses;

namespace Light.Serialization.Json.FrameworkExtensions
{
    /// <summary>
    ///     Provides extension methods for the <see cref="StringBuilder" /> class.
    /// </summary>
    public static class StringBuilderExtensions
    {
        /// <summary>
        ///     Appends a single quotation mark to the specified string builder and returns the resulting string.
        /// </summary>
        /// <param name="stringBuilder">The string builder that will modified.</param>
        /// <returns>The string created by the string builder.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="stringBuilder" /> is null.</exception>
        public static string CompleteJsonStringWithQuotationMark(this StringBuilder stringBuilder)
        {
            stringBuilder.MustNotBeNull(nameof(stringBuilder));

            stringBuilder.Append('"');
            return stringBuilder.ToString();
        }
    }
}