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
        ///     Gets the "genericMap" name for the Dictionary of TKey, TValue type.
        /// </summary>
        public const string GenericDictionary = "genericMap";

        /// <summary>
        ///     Gets the "sortedGenericMap" name for the SortedDictionary of TKey, TValue type.
        /// </summary>
        public const string SortedGenericDictionary = "sortedGenericMap";

        /// <summary>
        ///     Gets the "abstractGenericMap" name for the IDictionary of TKey, TValue type.
        /// </summary>
        public const string AbstractGenericDictionary = "abstractGenericMap";

        /// <summary>
        ///     Gets the "abstractMap" name for the non-generic IDictionary type.
        /// </summary>
        public const string AbstractDictionary = "abstractMap";

        /// <summary>
        ///     Gets the "abstractReadOnlyGenericMap" name for the IReadOnlyDictionary of TKey, TValue type.
        /// </summary>
        public const string AbstractReadOnlyGenericDictionary = "abstractReadOnlyGenericMap";

        /// <summary>
        ///     Gets the "genericCollection" name for the List of T type.
        /// </summary>
        public const string GenericList = "genericList";

        /// <summary>
        ///     Gets the "observableGenericList" name for the ObservableCollection of T type.
        /// </summary>
        public const string ObservableGenericCollection = "observableGenericList";

        /// <summary>
        ///     Gets the "genericCollection" name for the Collection of T type.
        /// </summary>
        public const string GenericCollection = "genericCollection";

        /// <summary>
        ///     Gets the "abstractGenericList" name for the IList of T type.
        /// </summary>
        public const string AbstractGenericList = "abstractGenericList";

        /// <summary>
        ///     Gets the "abstractGenericCollection" name for the ICollection of T type.
        /// </summary>
        public const string AbstractGenericCollection = "abstractGenericCollection";

        /// <summary>
        ///     Gets the "abstractGenericEnumerable" name for the IEnumerable of T type.
        /// </summary>
        public const string AbstractGenericEnumerable = "abstractGenericEnumerable";

        /// <summary>
        ///     Gets the "abstractReadOnlyGenericList" name for the IReadOnlyList of T type.
        /// </summary>
        public const string AbstractReadOnlyGenericList = "abstractReadOnlyGenericList";

        /// <summary>
        ///     Gets the "abstractReadOnlyGenericCollection" name for the IReadOnlyCollection of T type.
        /// </summary>
        public const string AbstractReadOnlyGenericCollection = "abstractReadOnlyGenericCollection";

        /// <summary>
        ///     Gets the "abstractList" name for the non-generic IList type.
        /// </summary>
        public const string AbstractList = "abstractList";

        /// <summary>
        ///     Gets the "abstractCollection" name for the non-generic ICollection type.
        /// </summary>
        public const string AbstractCollection = "abstractCollection";

        /// <summary>
        ///     Gets the "abstractEnumerable" name for the non-generic IEnumerable type.
        /// </summary>
        public const string AbstractEnumerable = "abstractEnumerable";

        /// <summary>
        ///     Gets the "genericSet" name for the HashSet of T type.
        /// </summary>
        public const string GenericHashSet = "genericHashSet";

        /// <summary>
        ///     Gets the "sortedGenericSet" name for the SortedSet of T type.
        /// </summary>
        public const string SortedGenericSet = "sortedGenericSet";

        /// <summary>
        ///     Gets the "abstractGenericSet" name for the ISet of T type.
        /// </summary>
        public const string AbstractGenericSet = "abstractGenericSet";

        /// <summary>
        ///     Gets the "array" name for the Array type.
        /// </summary>
        public const string Array = "array";

        /// <summary>
        ///     Adds all mappings for basic .NET types to the specified DomainFriendlyNameMapping.
        /// </summary>
        /// <param name="mapping">The mapping to be populated.</param>
        /// <returns>The mapping for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="mapping" /> is null.</exception>
        public static IAddMapping AddDefaultMappingsForBasicTypes(this IAddMapping mapping)
        {
            mapping.MustNotBeNull(nameof(mapping));

            return mapping
                // Primitive types
                .AddMapping(Int32, typeof(int))
                .AddMapping(Int64, typeof(long))
                .AddMapping(Int16, typeof(short))
                .AddMapping(Sbyte, typeof(sbyte))
                .AddMapping(UInt32, typeof(uint))
                .AddMapping(UInt64, typeof(ulong))
                .AddMapping(UInt16, typeof(ushort))
                .AddMapping(Byte, typeof(byte))
                .AddMapping(Single, typeof(float))
                .AddMapping(Double, typeof(double))
                .AddMapping(Decimal, typeof(decimal))
                .AddMapping(Bool, typeof(bool))
                .AddMapping(Character, typeof(char))
                .AddMapping(String, typeof(string))
                .AddMapping(Object, typeof(object))
                .AddMapping(DateTime, typeof(DateTime))
                .AddMapping(TimeSpan, typeof(TimeSpan))
                .AddMapping(DateTimeOffset, typeof(DateTimeOffset))

                // Collections, dictionaries, and sets
                .AddMapping(Array, typeof(Array))
                .AddMapping(GenericList, typeof(List<>))
                .AddMapping(ObservableGenericCollection, typeof(ObservableCollection<>))
                .AddMapping(GenericCollection, typeof(Collection<>))
                .AddMapping(AbstractGenericList, typeof(IList<>))
                .AddMapping(AbstractGenericCollection, typeof(ICollection<>))
                .AddMapping(AbstractGenericEnumerable, typeof(IEnumerable<>))
                .AddMapping(AbstractList, typeof(IList))
                .AddMapping(AbstractCollection, typeof(ICollection))
                .AddMapping(AbstractEnumerable, typeof(IEnumerable))
                .AddMapping(AbstractReadOnlyGenericList, typeof(IReadOnlyList<>))
                .AddMapping(AbstractReadOnlyGenericCollection, typeof(IReadOnlyCollection<>))
                .AddMapping(GenericDictionary, typeof(Dictionary<,>))
                .AddMapping(AbstractGenericDictionary, typeof(IDictionary<,>))
                .AddMapping(AbstractDictionary, typeof(IDictionary))
                .AddMapping(AbstractReadOnlyGenericDictionary, typeof(IReadOnlyDictionary<,>))
                .AddMapping(SortedGenericDictionary, typeof(SortedDictionary<,>))
                .AddMapping(GenericHashSet, typeof(HashSet<>))
                .AddMapping(AbstractGenericSet, typeof(ISet<>));
        }
    }
}