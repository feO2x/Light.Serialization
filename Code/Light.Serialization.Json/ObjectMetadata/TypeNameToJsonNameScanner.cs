using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents an object that scans the types of a specified assembly and creates mappings for JSON names using the type name.
    ///     You can specify which types are used by providing whitelist or blacklist filters.
    ///     This class should be used when you e.g. incorporate Domain-Driven Design in your development process and your domain
    ///     class names reflect the names that should occur in JSON documents.
    /// </summary>
    public sealed class TypeNameToJsonNameScanner : TypeNameToJsonNameScanner.IScanningOptions, TypeNameToJsonNameScanner.INamespaceOptions, TypeNameToJsonNameScanner.IExeptTypeOptions
    {
        private readonly List<Type> _usedTypes = new List<Type>();

        IScanningOptions IExeptTypeOptions.ExceptTypes(params Type[] types)
        {
            types.MustNotBeNullOrEmpty(nameof(types));

            var currentIndex = 0;
            while (currentIndex < _usedTypes.Count)
            {
                var type = _usedTypes[currentIndex];
                if (types.Contains(type))
                    _usedTypes.RemoveAt(currentIndex);
                else
                    currentIndex++;
            }
            return this;
        }

        IExeptTypeOptions INamespaceOptions.ExceptNamespaces(params string[] namespaces)
        {
            namespaces.MustNotBeNullOrEmpty(nameof(namespaces));

            var currentIndex = 0;
            while (currentIndex < _usedTypes.Count)
            {
                var type = _usedTypes[currentIndex];
                if (namespaces.Contains(type.Namespace))
                    _usedTypes.RemoveAt(currentIndex);
                else
                    currentIndex++;
            }

            return this;
        }

        IExeptTypeOptions INamespaceOptions.UseOnlyNamespaces(params string[] namespaces)
        {
            namespaces.MustNotBeNullOrEmpty(nameof(namespaces));

            var currentIndex = 0;
            while (currentIndex < _usedTypes.Count)
            {
                var type = _usedTypes[currentIndex];
                if (namespaces.Contains(type.Namespace) == false)
                    _usedTypes.RemoveAt(currentIndex);
                else
                    currentIndex++;
            }

            return this;
        }

        INamespaceOptions IScanningOptions.AllTypesFromAssemblies(params Type[] assemblyMarkers)
        {
            assemblyMarkers.MustNotBeNullOrEmpty(nameof(assemblyMarkers));

            var allTypes = assemblyMarkers.Select(m => m.GetTypeInfo().Assembly)
                                          .SelectMany(a => a.ExportedTypes)
                                          .Where(t => assemblyMarkers.Contains(t) == false);

            foreach (var type in allTypes)
            {
                _usedTypes.Add(type);
            }
            return this;
        }

        INamespaceOptions IScanningOptions.AllTypesFromAssemblies(params Assembly[] assemblies)
        {
            assemblies.MustNotBeNullOrEmpty(nameof(assemblies));

            foreach (var type in assemblies.SelectMany(a => a.ExportedTypes))
            {
                _usedTypes.Add(type);
            }

            return this;
        }

        IScanningOptions IScanningOptions.UseTypes(params Type[] types)
        {
            _usedTypes.AddRange(types);
            return this;
        }

        /// <summary>
        ///     Creates that JSON name to .NET type mappings and adds them to the specified object.
        /// </summary>
        /// <param name="mapping">The object containing all mappings from JSON names to .NET types.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="mapping" /> is null.</exception>
        public void CreateMappings(IAddOneToOneMapping mapping)
        {
            mapping.MustNotBeNull(nameof(mapping));

            foreach (var type in _usedTypes)
            {
                var jsonName = type.Name;
                if (type.GetTypeInfo().IsGenericType)
                    jsonName = type.Name.Split('`')[0];

                mapping.AddMapping(jsonName, type);
            }
        }

        /// <summary>
        ///     Represents the initial scanning options that a client should have when using a TypeNameToJsonNameScanner instance.
        /// </summary>
        public interface IScanningOptions
        {
            /// <summary>
            ///     Includes all types from the assemblies that contain the specified marker types. You can include or exclude specific namespaces or types later using a fluent API.
            ///     IMPORTANT: The marker types themselves are not included in the mappings.
            /// </summary>
            /// <param name="assemblyMarkers">The types marking the assemblies to be scanned.</param>
            /// <returns>The options to include or exclude specific namespaces in a fluent way.</returns>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="assemblyMarkers" /> is null.</exception>
            /// <exception cref="EmptyCollectionException">Thrown when <paramref name="assemblyMarkers" /> contains no items.</exception>
            INamespaceOptions AllTypesFromAssemblies(params Type[] assemblyMarkers);

            /// <summary>
            ///     Includes all types from the specified assemblies. You can include or exclude specific namespaces or types later using a fluent API.
            /// </summary>
            /// <param name="assemblies">The assemblies that should be scanned.</param>
            /// <returns>The options to include or exclude specific namespaces in a fluent way.</returns>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="assemblies" /> is null.</exception>
            /// <exception cref="EmptyCollectionException">Thrown when <paramref name="assemblies" /> contains no items.</exception>
            INamespaceOptions AllTypesFromAssemblies(params Assembly[] assemblies);

            IScanningOptions UseTypes(params Type[] types);
        }

        /// <summary>
        ///     Represents the blacklist or whitelist options for namespaces that are presented to the client after they choose the assemblies to be scanned.
        /// </summary>
        public interface INamespaceOptions
        {
            /// <summary>
            ///     Excludes all types from the specified namespaces when creating the JSON name to .NET type mapping.
            /// </summary>
            /// <param name="namespaces">The namespaces to be excluded.</param>
            /// <returns>The option to exclude specific types.</returns>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="namespaces" /> is null.</exception>
            /// <exception cref="EmptyCollectionException">Thrown when <paramref name="namespaces" /> contains no items.</exception>
            IExeptTypeOptions ExceptNamespaces(params string[] namespaces);

            /// <summary>
            ///     Includes only the types from the specified namespaces when creating the JSON name to .NET type mapping.
            /// </summary>
            /// <param name="namespaces">The namespaces to be included.</param>
            /// <returns>The option to exclude specific types.</returns>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="namespaces" /> is null.</exception>
            /// <exception cref="EmptyCollectionException">Thrown when <paramref name="namespaces" /> contains no items.</exception>
            IExeptTypeOptions UseOnlyNamespaces(params string[] namespaces);
        }

        /// <summary>
        ///     Represents the options to blacklist certain types for the JSON name to .NET type mapping.
        /// </summary>
        public interface IExeptTypeOptions
        {
            /// <summary>
            ///     Excludes the specified types from the JSON name to .NET type mapping.
            /// </summary>
            /// <param name="types">The types to be excluded.</param>
            /// <returns>The initial scanning options.</returns>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="types" /> is null.</exception>
            /// <exception cref="EmptyCollectionException">Thrown when <paramref name="types" /> contains no items.</exception>
            IScanningOptions ExceptTypes(params Type[] types);
        }
    }
}