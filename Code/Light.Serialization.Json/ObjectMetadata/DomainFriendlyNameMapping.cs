using System;
using System.Collections.Generic;
using System.Linq;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents a JSON name to .NET type mapping (and vice versa).
    /// </summary>
    public sealed class DomainFriendlyNameMapping : INameToTypeMapping, ITypeToNameMapping, IAddOneToOneMapping
    {
        private readonly Dictionary<string, List<Type>> _nameToTypeMappings = new Dictionary<string, List<Type>>();
        private readonly Dictionary<Type, string> _typeToNameMappings = new Dictionary<Type, string>();

        void IAddOneToOneMapping.AddMapping(string jsonName, Type correspondingType)
        {
            AddMapping(jsonName, correspondingType);
        }

        /// <summary>
        ///     Maps the specified JSON name to the .NET type.
        /// </summary>
        /// <param name="typeName">The JSON name to be mapped.</param>
        /// <returns>The type that corresponds to the JSON name.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when <paramref name="typeName" /> is not a known JSON name.</exception>
        public Type Map(string typeName)
        {
            return _nameToTypeMappings[typeName][0];
        }

        /// <summary>
        ///     Maps the specified .NET type to a JSON name.
        /// </summary>
        /// <param name="type">The .NET type to be mapped.</param>
        /// <returns>The JSON name that corresponds to the specified type.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the <paramref name="type" /> is unknown to the mapping.</exception>
        public string Map(Type type)
        {
            return _typeToNameMappings[type];
        }

        /// <summary>
        ///     Adds a mapping for the JSON name to the specified type (or types). This mapping works in both directions.
        /// </summary>
        /// <param name="jsonName">The JSON name of the mapping.</param>
        /// <param name="defaultMappedType">The type that is returned when the JSON name should be mapped.</param>
        /// <param name="otherMappedTypes">The types that also map to the specified JSON name. Usually, you have to provide these only for special types like collection or dictionary types.</param>
        /// <returns>The mapping for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="jsonName" /> or <paramref name="defaultMappedType" /> is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="jsonName" /> is an empty string.</exception>
        /// <exception cref="StringIsOnlyWhiteSpaceException">Thrown when <paramref name="jsonName" /> contains only whitespace.</exception>
        public DomainFriendlyNameMapping AddMapping(string jsonName, Type defaultMappedType, params Type[] otherMappedTypes)
        {
            jsonName.MustNotBeNullOrWhiteSpace(nameof(jsonName));
            defaultMappedType.MustNotBeNull(nameof(defaultMappedType));

            var mappedTypes = new List<Type> { defaultMappedType };
            mappedTypes.AddRange(otherMappedTypes);
            return AddMapping(jsonName, mappedTypes);
        }

        /// <summary>
        ///     Adds a mapping for the JSON name to the specified types. This mapping works in both directions. The first item of the specified collection is
        ///     used as the default type when the JSON name should be mapped.
        /// </summary>
        /// <param name="jsonName">The JSON name of the mapping.</param>
        /// <param name="mappedTypes">The types mapped to the JSON name. This collection must have at least one item.</param>
        /// <returns>The mapping for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="jsonName" /> is an empty string.</exception>
        /// <exception cref="StringIsOnlyWhiteSpaceException">Thrown when <paramref name="jsonName" /> contains only whitespace.</exception>
        /// <exception cref="EmptyCollectionException">Thrown when <paramref name="mappedTypes" /> is an empty collection.</exception>
        public DomainFriendlyNameMapping AddMapping(string jsonName, List<Type> mappedTypes)
        {
            jsonName.MustNotBeNullOrWhiteSpace(nameof(jsonName));
            mappedTypes.MustNotBeNullOrEmpty(nameof(mappedTypes));

            _nameToTypeMappings.Add(jsonName, mappedTypes.ToList());
            foreach (var type in mappedTypes)
            {
                _typeToNameMappings.Add(type, jsonName);
            }
            return this;
        }

        /// <summary>
        ///     Removes the mapping with the specified JSON name.
        /// </summary>
        /// <param name="jsonName">The JSON name of the mapping to be removed.</param>
        /// <returns>The mapping for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="jsonName" /> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when there is no mapping containing <paramref name="jsonName" />.</exception>
        public DomainFriendlyNameMapping RemoveMapping(string jsonName)
        {
            jsonName.MustBeKeyOf(_nameToTypeMappings, nameof(jsonName));

            var types = _nameToTypeMappings[jsonName];
            _nameToTypeMappings.Remove(jsonName);
            foreach (var type in types)
            {
                _typeToNameMappings.Remove(type);
            }
            return this;
        }

        /// <summary>
        ///     Removes the mapping with the specified .NET type. If there are multiple .NET types that map to the same JSON name, the other ones will be left intact.
        /// </summary>
        /// <param name="type">The type of the mapping to be removed.</param>
        /// <returns>The mapping for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when there is no mapping containing <paramref name="type" />.</exception>
        public DomainFriendlyNameMapping RemoveMapping(Type type)
        {
            type.MustBeKeyOf(_typeToNameMappings, nameof(type));

            var jsonName = _typeToNameMappings[type];
            _typeToNameMappings.Remove(type);

            var allMappedTypes = _nameToTypeMappings[jsonName];
            allMappedTypes.Remove(type);
            if (allMappedTypes.Count == 0)
                _nameToTypeMappings.Remove(jsonName);
            return this;
        }

        /// <summary>
        ///     Sets the default type for a JSON name to multiple .NET types mapping. If the specified type is not already part of the mapping, it will be added to it.
        /// </summary>
        /// <param name="jsonName">The JSON name of the mapping.</param>
        /// <param name="newDefaultType">The new default type that will be returned when the JSON name is mapped.</param>
        /// <returns>The mapping for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="jsonName" /> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when there is no mapping containing <paramref name="jsonName" />.</exception>
        public DomainFriendlyNameMapping SetDefaultType(string jsonName, Type newDefaultType)
        {
            jsonName.MustBeKeyOf(_nameToTypeMappings, nameof(jsonName));

            var types = _nameToTypeMappings[jsonName];
            var indexOfNewDefaultType = types.IndexOf(newDefaultType);
            if (indexOfNewDefaultType == -1)
                _typeToNameMappings.Add(newDefaultType, jsonName);
            else
                types.RemoveAt(indexOfNewDefaultType);

            types.Insert(0, newDefaultType);

            return this;
        }

        /// <summary>
        ///     Scans assemblies for types using a TypeNameToJsonNameScanner and adds all resulting mappings.
        /// </summary>
        /// <param name="configureOptions">The delegate that configures the assembly scanner.</param>
        /// <returns>The mapping for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configureOptions" /> is null.</exception>
        public DomainFriendlyNameMapping ScanTypes(Action<TypeNameToJsonNameScanner.IScanningOptions> configureOptions)
        {
            configureOptions.MustNotBeNull(nameof(configureOptions));

            var transformer = new TypeNameToJsonNameScanner();
            configureOptions(transformer);

            transformer.CreateMappings(this);
            return this;
        }

        /// <summary>
        ///     Removes all JSON name to .NET type(s) mappings from this instance.
        /// </summary>
        /// <returns>The mapping for method chaining.</returns>
        public DomainFriendlyNameMapping ClearAllMappings()
        {
            _nameToTypeMappings.Clear();
            _typeToNameMappings.Clear();
            return this;
        }
    }
}