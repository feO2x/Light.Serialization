﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;
using Light.Serialization.Json.LowLevelReading;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents an <see cref="IJsonTokenParser" /> that is activated when a JSON string representing a primitive value
    ///     was not parsed by another token parser because the value is referenced via a base class (e.g. <see cref="object" /> or <see cref="ValueType" />).
    ///     This parser should be placed after all other parsers that interpret JSON strings.
    /// </summary>
    public sealed class JsonStringInheritanceParser : IJsonTokenParser
    {
        private readonly List<Type> _stringInterfaces = typeof(string).GetTypeInfo().ImplementedInterfaces.ToList();
        private readonly IReadOnlyList<IJsonStringToPrimitiveParser> _stringToPrimitiveParsers;
        private object _lastParsedValue;

        /// <summary>
        ///     Creates a new instance of <see cref="JsonStringInheritanceParser" />.
        /// </summary>
        /// <param name="stringToPrimitiveParsers">All JSON token parsers that are able to "try to parse" a JSON string to a primitive type.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="stringToPrimitiveParsers" /> is null.</exception>
        /// <exception cref="EmptyCollectionException">Thrown when <paramref name="stringToPrimitiveParsers" /> is an empty collection.</exception>
        public JsonStringInheritanceParser(IReadOnlyList<IJsonStringToPrimitiveParser> stringToPrimitiveParsers)
        {
            stringToPrimitiveParsers.MustNotBeNullOrEmpty(nameof(stringToPrimitiveParsers));

            _stringToPrimitiveParsers = stringToPrimitiveParsers;
        }

        /// <summary>
        ///     Gets the value indicating that this parser must not be cached.
        /// </summary>
        public bool CanBeCached => false;

        /// <summary>
        ///     Checks if there is any <see cref="IJsonStringToPrimitiveParser" /> that can interpret the given JSON string.
        /// </summary>
        public bool IsSuitableFor(JsonDeserializationContext context)
        {
            if (context.Token.JsonType != JsonTokenType.String)
                return false;

            var tokenAsString = context.DeserializeToken<string>(context.Token);

            // Check if there is any primitive parser that can make sense of the JSON string
            foreach (var parser in _stringToPrimitiveParsers)
            {
                if (parser.AssociatedInterfacesAndBaseClasses.Contains(context.RequestedType) == false)
                    continue;

                var parseResult = parser.TryParse(context, tokenAsString);
                if (parseResult.WasTokenParsedSuccessfully == false)
                    continue;

                _lastParsedValue = parseResult.ParsedObject;
                return true;
            }

            // If the string could not be interpreted, then use the parsed string if possible
            if (context.RequestedType != typeof(object) && _stringInterfaces.Contains(context.RequestedType) == false)
                return false;

            _lastParsedValue = tokenAsString;
            return true;
        }

        /// <summary>
        ///     Returns the parsed value that was obtained in <see cref="IsSuitableFor" />.
        ///     This method must be called before this one.
        /// </summary>
        public ParseResult ParseValue(JsonDeserializationContext context)
        {
            var returnValue = _lastParsedValue;
            _lastParsedValue = null;
            return ParseResult.FromParsedValue(returnValue);
        }
    }
}