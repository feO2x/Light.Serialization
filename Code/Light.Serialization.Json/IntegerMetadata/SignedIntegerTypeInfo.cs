using System;
using System.Text.RegularExpressions;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;

namespace Light.Serialization.Json.IntegerMetadata
{
    /// <summary>
    ///     Represents an info object that describes a .NET signed integer type for the SignedIntegerParser.
    /// </summary>
    public sealed class SignedIntegerTypeInfo
    {
        /// <summary>
        ///     Gets the delagate that is used to downcast a long value to the .NET numeric type that this info object describes.
        /// </summary>
        public readonly Func<long, object> DowncastValue;

        /// <summary>
        ///     Gets the maximum value of the .NET numeric type as a string.
        /// </summary>
        public readonly string MaximumAsString;

        /// <summary>
        ///     Gets the minimum value of the .NET numeric type as a string.
        /// </summary>
        public readonly string MinimumAsString;

        /// <summary>
        ///     Gets the .NET numeric type.
        /// </summary>
        public readonly Type Type;

        /// <summary>
        ///     Creates a new instance of <see cref="SignedIntegerTypeInfo" />.
        /// </summary>
        /// <param name="type">The .NET numeric type that this info describes.</param>
        /// <param name="minimumAsString">The minimum of the .NET numeric type as a string.</param>
        /// <param name="maximumAsString">The maximum of the .NET numeric type as a string.</param>
        /// <param name="downcastValue">The delegate that is able to downcast the long value to the .NET numeric type.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        /// <exception cref="StringDoesNotMatchException">
        ///     Thrown when <paramref name="minimumAsString" /> or <paramref name="maximumAsString" /> do
        ///     not describe a negative or positive integer number, respectively.
        /// </exception>
        public SignedIntegerTypeInfo(Type type, string minimumAsString, string maximumAsString, Func<long, object> downcastValue)
        {
            type.MustNotBeNull(nameof(type));
            minimumAsString.MustMatch(new Regex("-[1-9][0-9]*"), nameof(minimumAsString));
            maximumAsString.MustMatch(new Regex("[1-9][0-9]*"), nameof(maximumAsString));
            downcastValue.MustNotBeNull(nameof(downcastValue));

            Type = type;
            MinimumAsString = minimumAsString;
            MaximumAsString = maximumAsString;
            DowncastValue = downcastValue;
        }
    }
}