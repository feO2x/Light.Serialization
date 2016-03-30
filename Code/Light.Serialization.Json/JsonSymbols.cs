﻿using System.Collections.Generic;

namespace Light.Serialization.Json
{
    /// <summary>
    /// Defines the default JSON symbols according to http://json.org/.
    /// </summary>
    public static class JsonSymbols
    {
        /// <summary>
        /// Gets the number symbol.
        /// </summary>
        public const string Number = "number";

        /// <summary>
        /// Gets the array symbol.
        /// </summary>
        public const string Array = "array";

        /// <summary>
        /// Gets the object symbol.
        /// </summary>
        public const string Object = "object";

        /// <summary>
        /// Gets the string symbol.
        /// </summary>
        public const string String = "string";

        /// <summary>
        /// Gets the "true" symbol.
        /// </summary>
        public const string True = "true";

        /// <summary>
        /// Gets the "false" symbol.
        /// </summary>
        public const string False = "false";

        /// <summary>
        /// Gets the "null" symbol.
        /// </summary>
        public const string Null = "null";

        /// <summary>
        /// Gets the "e" symbol used for exponential JSON numbers
        /// </summary>
        public const char LowercaseExponential = 'e';

        /// <summary>
        /// Gets the "E" symbol used for exponential JSON numbers.
        /// </summary>
        public const char UppercaseExponential = 'E';

        /// <summary>
        /// Gets the "{" symbol used to mark the beginning of a complex JSON object.
        /// </summary>
        public const char BeginOfObject = '{';

        /// <summary>
        /// Gets the "}" symbol used to mark the end of a complex JSON object.
        /// </summary>
        public const char EndOfObject = '}';

        /// <summary>
        /// Get the "[" symbol used to mark the beginning of a JSON array.
        /// </summary>
        public const char BeginOfArray = '[';

        /// <summary>
        /// Gets the "]" symbol used to mark the end of a JSON array.
        /// </summary>
        public const char EndOfArray = ']';

        /// <summary>
        /// Gets the '"' (quotation marks) symbol that marks the beginning and end of a JSON string.
        /// </summary>
        public const char StringDelimiter = '"';

        /// <summary>
        /// Gets the ":" symbol that is placed between key-value pairs in complex JSON objects.
        /// </summary>
        public const char PairDelimiter = ':';

        /// <summary>
        /// Gets the "," symbol that is placed after values in complex JSON objects or JSON arrays.
        /// </summary>
        public const char ValueDelimiter = ',';

        /// <summary>
        /// Gets the "." symbol that marks the decimal point in JSON numbers.
        /// </summary>
        public const char DecimalPoint = '.';

        /// <summary>
        /// Gets the "+" symbol that marks a positive exponential part of a JSON number.
        /// </summary>
        public const char Plus = '+';

        /// <summary>
        /// Gets the "-" symbol that marks either a negative JSON number or a negative exponential part of a JSON number.
        /// </summary>
        public const char Minus = '-';

        /// <summary>
        /// Gets the "\" symbol that marks an escape sequence in a JSON string.
        /// </summary>
        public const char StringEscapeCharacter = '\\';

        /// <summary>
        /// Gets the "u" symbol that marks a four digit hexadecimal escape sequence in a JSON string.
        /// </summary>
        public const char HexadecimalEscapeIndicator = 'u';

        /// <summary>
        /// Gets the "$type" symbol that marks the type section of a complex JSON object (specific to Light.Serialization.Json).
        /// </summary>
        public const string DefaultConcreteTypeSymbol = "$type";

        /// <summary>
        /// Gets the "typeArguments" symbol that is used to mark the type arguments section of a generic type (specific to Light.Serialization.Json).
        /// </summary>
        public const string DefaultTypeArgumentSymbol = "typeArguments";

        /// <summary>
        /// Gets the "name" symbol that is used to mark the type name of a generic type (specific to Light.Serialization.Json).
        /// </summary>
        public const string DefaultTypeNameSymbol = "name";

        /// <summary>
        /// Gets the collection containing all single-escaped characters that are used in JSON strings.
        /// These are: \", \\, \/, \b, \f, \n, \r, and \t.
        /// </summary>
        public static readonly IReadOnlyList<SingleEscapedCharacter> SingleEscapedCharacters =
            new[]
            {
                new SingleEscapedCharacter('"', '"'),
                new SingleEscapedCharacter('\\', '\\'),
                new SingleEscapedCharacter('/', '/'),
                new SingleEscapedCharacter('\b', 'b'),
                new SingleEscapedCharacter('\f', 'f'),
                new SingleEscapedCharacter('\n', 'n'),
                new SingleEscapedCharacter('\r', 'r'),
                new SingleEscapedCharacter('\t', 't')
            };

        /// <summary>
        /// Checks if the specified character is an exponential symbol (i.e. 'e' or 'E').
        /// </summary>
        /// <param name="character">The character to be checked.</param>
        /// <returns>True if the specified character is an 'e' or an 'E', else false.</returns>
        public static bool IsExponentialSymbol(this char character)
        {
            return character == LowercaseExponential || character == UppercaseExponential;
        }
    }
}