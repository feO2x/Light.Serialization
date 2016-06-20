using System;
using System.Collections.Generic;
using System.Linq;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;
using Light.Serialization.Json.TokenParsers;

namespace Light.Serialization.Json.IntegerMetadata
{
    /// <summary>
    ///     Represents an object describing all different .NET signed integer types for the <see cref="SignedIntegerParser" />.
    /// </summary>
    public sealed class SignedIntegerTypes
    {
        /// <summary>
        ///     Gets the default type that is used when parsing a JSON number and the requested type is not
        ///     a concrete type.
        /// </summary>
        public readonly SignedIntegerTypeInfo DefaultType;

        /// <summary>
        ///     Gets the mapping from .NET numeric type to signed integer type info.
        /// </summary>
        public readonly Dictionary<Type, SignedIntegerTypeInfo> IntegerTypeInfos;

        /// <summary>
        ///     Creates a new instance of <see cref="SignedIntegerTypes" />.
        /// </summary>
        /// <param name="integerTypeInfos">The mapping from .NET numeric type to signed integer type info.</param>
        /// <param name="defaultType">The default type that should be used when a JSON number should be serialized to object or ValueType.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        /// <exception cref="EmptyCollectionException">Thrown when <paramref name="integerTypeInfos" /> contains no entries.</exception>
        public SignedIntegerTypes(Dictionary<Type, SignedIntegerTypeInfo> integerTypeInfos, SignedIntegerTypeInfo defaultType)
        {
            integerTypeInfos.MustNotBeNullOrEmpty(nameof(integerTypeInfos));
            defaultType.MustNotBeNull(nameof(defaultType));

            IntegerTypeInfos = integerTypeInfos;
            DefaultType = defaultType;
        }

        /// <summary>
        ///     Creates a new SignedIntegerTypes instance for the default .NET signed integer types int, long, short, and sbyte.
        /// </summary>
        public static SignedIntegerTypes CreateDefaultSignedIntegerTypes()
        {
            var signedIntegerTypes = new[]
                                     {
                                         new SignedIntegerTypeInfo(typeof(int), int.MinValue.ToString(), int.MaxValue.ToString(), l => (int) l),
                                         new SignedIntegerTypeInfo(typeof(long), long.MinValue.ToString(), long.MaxValue.ToString(), l => l),
                                         new SignedIntegerTypeInfo(typeof(short), short.MinValue.ToString(), short.MaxValue.ToString(), l => (short) l),
                                         new SignedIntegerTypeInfo(typeof(sbyte), sbyte.MinValue.ToString(), sbyte.MaxValue.ToString(), l => (sbyte) l)
                                     };

            return new SignedIntegerTypes(signedIntegerTypes.ToDictionary(i => i.Type), signedIntegerTypes[0]);
        }
    }
}