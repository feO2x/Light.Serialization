using System;
using Light.GuardClauses;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents the information that is necessary to create a dictionary or complex .NET object
    ///     from the metadata section of a complex JSON object.
    /// </summary>
    public struct MetadataParseResult
    {
        private object _objectFromCache;
        private Type _typeToConstruct;

        /// <summary>
        ///     Creates a new instance of <see cref="MetadataParseResult" />.
        /// </summary>
        /// <param name="typeToConstruct">The requested .NET type for the complex JSON object.</param>
        public MetadataParseResult(Type typeToConstruct)
        {
            typeToConstruct.MustNotBeNull(nameof(typeToConstruct));

            _typeToConstruct = typeToConstruct;
            _objectFromCache = null;
        }

        /// <summary>
        ///     Gets or sets the reference to the object that was deserialized before.
        /// </summary>
        public object ObjectFromCache
        {
            get { return _objectFromCache; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _objectFromCache = value;
            }
        }

        /// <summary>
        ///     Gets or sets the type to construct.
        /// </summary>
        public Type TypeToConstruct
        {
            get { return _typeToConstruct; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _typeToConstruct = value;
            }
        }
    }
}