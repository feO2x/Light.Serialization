using System;
using Light.Serialization.Json.TokenParsers;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents an <see cref="IObjectMetadataParser" /> that can also parse values of complex JSON objects as <see cref="Type" /> instances.
    /// </summary>
    public interface ITypeParser : IObjectMetadataParser
    {
        /// <summary>
        ///     Parses the specified value in a complex JSON object as a <see cref="Type" /> instance.
        ///     The next token read from the context's JSON reader must be the beginnning of this value.
        /// </summary>
        Type ParseType(JsonDeserializationContext context);
    }
}