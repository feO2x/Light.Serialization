using System;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents the base class holding all data fields for metadata sections in complex JSON objects or JSON arrays.
    /// </summary>
    public abstract class BaseMetadata
    {
        /// <summary>
        ///     Gets or sets the symbol that is used to mark the JSON document ID for a complex object.
        ///     This value defaults to to "$id".
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="value" /> is an empty string.</exception>
        /// <exception cref="StringIsOnlyWhiteSpaceException">Thrown when <paramref name="value" /> contains only whitespace.</exception>
        public string IdSymbol
        {
            get { return _idSymbol; }
            set
            {
                value.MustNotBeNullOrWhiteSpace(nameof(value));
                _idSymbol = value;
            }
        }

        /// <summary>
        ///     Gets or sets the symbol that is used to mark a reference to another complex JSON object within the document.
        ///     This value defaults to "$ref".
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="value" /> is an empty string.</exception>
        /// <exception cref="StringIsOnlyWhiteSpaceException">Thrown when <paramref name="value" /> contains only whitespace.</exception>
        public string ReferenceSymbol
        {
            get { return _referenceSymbol; }
            set
            {
                value.MustNotBeNullOrWhiteSpace(nameof(value));
                _referenceSymbol = value;
            }
        }

        /// <summary>
        ///     Gets or sets the symbol that is used to mark the type of a complex JSON object.
        ///     This value defaults to "$type".
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="value" /> is an empty string.</exception>
        /// <exception cref="StringIsOnlyWhiteSpaceException">Thrown when <paramref name="value" /> contains only whitespace.</exception>
        public string ConcreteTypeSymbol
        {
            get { return _concreteTypeSymbol; }
            set
            {
                value.MustNotBeNullOrWhiteSpace(nameof(value));
                _concreteTypeSymbol = value;
            }
        }

        /// <summary>
        ///     Gets or sets the symbol that is used to mark the name of a generic type.
        ///     This value defaults to "name".
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="value" /> is an empty string.</exception>
        /// <exception cref="StringIsOnlyWhiteSpaceException">Thrown when <paramref name="value" /> contains only whitespace.</exception>
        public string GenericTypeNameSymbol
        {
            get { return _genericTypeNameSymbol; }
            set
            {
                value.MustNotBeNullOrWhiteSpace(nameof(value));
                _genericTypeNameSymbol = value;
            }
        }

        /// <summary>
        ///     Gets or sets the symbol that is used to mark the collection of type arguments for a generic type.
        ///     This value defaults to "typeArguments".
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="value" /> is an empty string.</exception>
        /// <exception cref="StringIsOnlyWhiteSpaceException">Thrown when <paramref name="value" /> contains only whitespace.</exception>
        public string GenericTypeArgumentsSymbol
        {
            get { return _genericTypeArgumentsSymbol; }
            set
            {
                value.MustNotBeNullOrWhiteSpace(nameof(value));
                _genericTypeArgumentsSymbol = value;
            }
        }

        /// <summary>
        ///     Gets or sets the symbol that is used to mark the JSON string containing the actual type of a .NET array.
        ///     This value defaults to "arrayType".
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="value" /> is an empty string.</exception>
        /// <exception cref="StringIsOnlyWhiteSpaceException">Thrown when <paramref name="value" /> contains only whitespace.</exception>
        public string ArrayTypeSymbol
        {
            get { return _arrayTypeSymbol; }
            set
            {
                value.MustNotBeNullOrWhiteSpace(nameof(value));
                _arrayTypeSymbol = value;
            }
        }

        /// <summary>
        ///     Gets or sets the value that is used to mark the JSON string containing the rank of a .NET array.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="value" /> is an empty string.</exception>
        /// <exception cref="StringIsOnlyWhiteSpaceException">Thrown when <paramref name="value" /> contains only whitespace.</exception>
        public string ArrayRankSymbol
        {
            get { return _arrayRankSymbol; }
            set
            {
                value.MustNotBeNullOrWhiteSpace(nameof(value));
                _arrayRankSymbol = value;
            }
        }

        /// <summary>
        ///     Gets or sets the symbol that is used to mark the JSON number describing the length of a .NET array.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="value" /> is an empty string.</exception>
        /// <exception cref="StringIsOnlyWhiteSpaceException">Thrown when <paramref name="value" /> contains only whitespace.</exception>
        public string ArrayLengthSymbol
        {
            get { return _arrayLengthSymbol; }
            set
            {
                value.MustNotBeNullOrWhiteSpace(nameof(value));
                _arrayLengthSymbol = value;
            }
        }

        // ReSharper disable InconsistentNaming
        protected string _concreteTypeSymbol = JsonSymbols.DefaultConcreteTypeSymbol;
        protected string _genericTypeArgumentsSymbol = JsonSymbols.DefaultGenericTypeArgumentsSymbol;
        protected string _genericTypeNameSymbol = JsonSymbols.DefaultGenericTypeNameSymbol;
        protected string _idSymbol = JsonSymbols.DefaultIdSymbol;
        protected string _referenceSymbol = JsonSymbols.DefaultReferenceSymbol;
        protected string _arrayTypeSymbol = JsonSymbols.DefaultArrayTypeSymbol;
        protected string _arrayRankSymbol = JsonSymbols.DefaultArrayRankSymbol;
        protected string _arrayLengthSymbol = JsonSymbols.DefaultArrayLengthSymbol;
        // ReSharper restore InconsistentNaming
    }
}