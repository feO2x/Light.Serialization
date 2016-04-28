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
    public sealed class DomainFriendlyNameMapping : INameToTypeMapping, ITypeToNameMapping, IAddMapping
    {
        private readonly Dictionary<string, Type> _nameToTypeMappings;
        private readonly Dictionary<Type, string> _typeToNameMappings;

        /// <summary>
        ///     Creates a new instance of <see cref="DomainFriendlyNameMapping" />.
        /// </summary>
        public DomainFriendlyNameMapping()
        {
            _nameToTypeMappings = new Dictionary<string, Type>();
            _typeToNameMappings = new Dictionary<Type, string>();
        }

        /// <summary>
        ///     Creates a new instance of <see cref="DomainFriendlyNameMapping" />
        /// </summary>
        /// <param name="nameToTypeMappings">The dictionary containing all mappings from JSON name to .NET type.</param>
        public DomainFriendlyNameMapping(Dictionary<string, Type> nameToTypeMappings)
        {
            nameToTypeMappings.MustNotBeNull(nameof(nameToTypeMappings));
            nameToTypeMappings.Values.MustNotContainNull(message: "The mapping must not contain type values that are null.");

            _nameToTypeMappings = nameToTypeMappings;
            _typeToNameMappings = nameToTypeMappings.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
        }

        IAddMapping IAddMapping.AddMapping(string jsonName, Type correspondingType)
        {
            return AddMapping(jsonName, correspondingType);
        }

        /// <summary>
        ///     Maps the specified JSON name to the .NET type.
        /// </summary>
        /// <param name="typeName">The JSON name to be mapped.</param>
        /// <returns>The type that corresponds to the JSON name.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when <paramref name="typeName" /> is not a known JSON name.</exception>
        public Type Map(string typeName)
        {
            return _nameToTypeMappings[typeName];
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
        /// <param name="mappedType">The type that is returned when the JSON name should be mapped.</param>
        /// <returns>The mapping for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="jsonName" /> or <paramref name="mappedType" /> is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="jsonName" /> is an empty string.</exception>
        /// <exception cref="StringIsOnlyWhiteSpaceException">Thrown when <paramref name="jsonName" /> contains only whitespace.</exception>
        public DomainFriendlyNameMapping AddMapping(string jsonName, Type mappedType)
        {
            jsonName.MustNotBeNullOrWhiteSpace(nameof(jsonName));
            mappedType.MustNotBeNull(nameof(mappedType));

            _nameToTypeMappings.Add(jsonName, mappedType);
            _typeToNameMappings.Add(mappedType, jsonName);

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

            var type = _nameToTypeMappings[jsonName];
            _nameToTypeMappings.Remove(jsonName);
            _typeToNameMappings.Remove(type);

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
            _nameToTypeMappings.Remove(jsonName);

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

        /// <summary>
        ///     Creates a new instance of DomainFriendlyNameMapping with the default mappings for basic .NET types already registered.
        /// </summary>
        public static DomainFriendlyNameMapping CreateWithDefaultTypeMappings()
        {
            var mapping = new DomainFriendlyNameMapping();
            mapping.AddDefaultMappingsForBasicTypes();
            return mapping;
        }
    }
}