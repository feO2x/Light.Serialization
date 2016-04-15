using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Light.GuardClauses;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Provides domain friendly names for essential .NET types like e.g. int, double, bool, string, object, DateTime, collections and dictionaries.
    /// </summary>
    public static class BasicNetTypeNames
    {
        /// <summary>
        ///     Gets the "int32" name for type int.
        /// </summary>
        public const string Int32 = "int32";

        /// <summary>
        ///     Gets the "int64" name for type long.
        /// </summary>
        public const string Int64 = "int64";

        /// <summary>
        ///     Gets the "int16" name for type short.
        /// </summary>
        public const string Int16 = "int16";

        /// <summary>
        ///     Gets the "int8" name for type sbyte.
        /// </summary>
        public const string Sbyte = "int8";

        /// <summary>
        ///     Gets the "uint32" name for type uint.
        /// </summary>
        public const string UInt32 = "uint32";

        /// <summary>
        ///     Gets the "uint64" name for type ulong.
        /// </summary>
        public const string UInt64 = "uint64";

        /// <summary>
        ///     Gets the "uint16" name for type ushort.
        /// </summary>
        public const string UInt16 = "uint16";

        /// <summary>
        ///     Gets the "byte" name for type byte.
        /// </summary>
        public const string Byte = "byte";

        /// <summary>
        ///     Gets the "float32" name for type float.
        /// </summary>
        public const string Single = "float32";

        /// <summary>
        ///     Gets the "float64" name for type double.
        /// </summary>
        public const string Double = "float64";

        /// <summary>
        ///     Gets the "decimal" name for type decimal.
        /// </summary>
        public const string Decimal = "decimal";

        /// <summary>
        ///     Gets the "bool" name for type bool.
        /// </summary>
        public const string Bool = "bool";

        /// <summary>
        ///     Gets the "character" name for type char.
        /// </summary>
        public const string Character = "character";

        /// <summary>
        ///     Gets the "string" name for type string.
        /// </summary>
        public const string String = "string";

        /// <summary>
        ///     Gets the "object" name for type object.
        /// </summary>
        public const string Object = "object";

        /// <summary>
        ///     Gets the "dateTime" name for type DateTime.
        /// </summary>
        public const string DateTime = "dateTime";

        /// <summary>
        ///     Gets the "duration" name for type TimeSpan.
        /// </summary>
        public const string TimeSpan = "duration";

        /// <summary>
        ///     Gets the "localDateTime" name for type DateTimeOffset.
        /// </summary>
        public const string DateTimeOffset = "localDateTime";

        /// <summary>
        ///     Gets the "genericMap" name for all types that implement IDictionary of TKey, TValue.
        /// </summary>
        public const string GenericDictionary = "genericMap";

        /// <summary>
        ///     Gets the "genericCollection" name for all types that implement IEnumerable of T.
        /// </summary>
        public const string GenericCollection = "genericCollection";

        /// <summary>
        /// Gets the "genericSet" name for all types that implement ISet of T.
        /// </summary>
        public const string GenericSet = "genericSet";

        /// <summary>
        ///     Adds all mappings for basic .NET types to the specified DomainFriendlyNameMapping.
        /// </summary>
        /// <param name="mapping">The mapping to be populated.</param>
        /// <returns>The mapping for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="mapping" /> is null.</exception>
        public static DomainFriendlyNameMapping AddDefaultMappingsForBasicTypes(this DomainFriendlyNameMapping mapping)
        {
            mapping.MustNotBeNull(nameof(mapping));

            return mapping.AddMapping(Int32, typeof (int))
                          .AddMapping(Int64, typeof (long))
                          .AddMapping(Int16, typeof (short))
                          .AddMapping(Sbyte, typeof (sbyte))
                          .AddMapping(UInt32, typeof (uint))
                          .AddMapping(UInt64, typeof (ulong))
                          .AddMapping(UInt16, typeof (ushort))
                          .AddMapping(Byte, typeof (byte))
                          .AddMapping(Single, typeof (float))
                          .AddMapping(Double, typeof (double))
                          .AddMapping(Decimal, typeof (decimal))
                          .AddMapping(Bool, typeof (bool))
                          .AddMapping(Character, typeof (char))
                          .AddMapping(String, typeof (string))
                          .AddMapping(Object, typeof (object))
                          .AddMapping(DateTime, typeof (DateTime))
                          .AddMapping(TimeSpan, typeof (TimeSpan))
                          .AddMapping(DateTimeOffset, typeof (DateTimeOffset))
                          .AddMapping(GenericCollection, typeof (List<>), typeof (IList<>), typeof (ICollection<>), typeof (IEnumerable<>), typeof (Collection<>), typeof (ObservableCollection<>))
                          .AddMapping(GenericDictionary, typeof (Dictionary<,>), typeof (IDictionary<,>), typeof (IDictionary), typeof (IReadOnlyDictionary<,>), typeof (SortedDictionary<,>))
                          .AddMapping(GenericSet, typeof (HashSet<>), typeof (ISet<>), typeof (SortedSet<>));

        }
    }
}