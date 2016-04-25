using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;

namespace Light.Serialization.Json.IntegerMetadata
{
    /// <summary>
    ///     Represents an info object that describes a .NET unsigned integer type for the UnsignedIntegerParser.
    /// </summary>
    public sealed class UnsignedIntegerTypeInfo
    {
        /// <summary>
        ///     Gets the delegate that can be used to downcast the value from ulong to the target unsigned integer type.
        /// </summary>
        public readonly Func<ulong, object> DowncastValue;

        /// <summary>
        ///     Gets the maximum of the unsigned integer type as a string.
        /// </summary>
        public readonly string MaximumAsString;

        /// <summary>
        ///     Gets the type of the .NET unsigned integer type.
        /// </summary>
        public readonly Type Type;

        /// <summary>
        ///     Creates a new intance of UnsignedIntegerTypeInfo.
        /// </summary>
        /// <param name="type">The .NET unsigned integer type.</param>
        /// <param name="maximumAsString">The maximum of the unsigned integer type as a string.</param>
        /// <param name="downcastValue">The delegate to downcast from ulong to the target type.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        /// <exception cref="StringDoesNotMatchException">Thrown when <paramref name="maximumAsString" /> does not describe a positive integer number.</exception>
        public UnsignedIntegerTypeInfo(Type type, string maximumAsString, Func<ulong, object> downcastValue)
        {
            type.MustNotBeNull(nameof(type));
            maximumAsString.MustMatch(new Regex("[1-9][0-9]*"), nameof(maximumAsString));
            downcastValue.MustNotBeNull(nameof(downcastValue));

            Type = type;
            MaximumAsString = maximumAsString;
            DowncastValue = downcastValue;
        }

        /// <summary>
        ///     Creates the default mapping of .NET unsigned integer type to UnsignedIntegerTypeInfo for the following types: uint, ulong, ushort, and byte.
        /// </summary>
        public static Dictionary<Type, UnsignedIntegerTypeInfo> CreateDefaultUnsignedIntegerTypes()
        {
            return new[]
                   {
                       new UnsignedIntegerTypeInfo(typeof(uint), uint.MaxValue.ToString(), ul => (uint) ul),
                       new UnsignedIntegerTypeInfo(typeof(ulong), ulong.MaxValue.ToString(), ul => ul),
                       new UnsignedIntegerTypeInfo(typeof(ushort), ushort.MaxValue.ToString(), ul => (ushort) ul),
                       new UnsignedIntegerTypeInfo(typeof(byte), byte.MaxValue.ToString(), ul => (byte) ul)
                   }.ToDictionary(t => t.Type);
        }
    }
}