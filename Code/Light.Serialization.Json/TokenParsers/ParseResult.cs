using System;
using Light.GuardClauses;

namespace Light.Serialization.Json.TokenParsers
{
    /// <summary>
    ///     Represents the result of a parse operation by a JSON token parser.
    /// </summary>
    public struct ParseResult
    {
        /// <summary>
        ///     Gets the value if parsing could be fully completed.
        /// </summary>
        public readonly object ParsedValue;

        /// <summary>
        ///     Gets the value indicating whether the object could not be parsed because it is a deferred reference.
        /// </summary>
        public readonly bool IsDeferredReference;

        private readonly int? _refId;

        private ParseResult(object parsedValue, int? refId)
        {
            ParsedValue = parsedValue;
            _refId = refId;
            IsDeferredReference = _refId.HasValue;
        }

        /// <summary>
        ///     Gets the id of the deferred reference.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when this parse result does not represent a deferred reference.</exception>
        public int ReferenceId
        {
            get
            {
                _refId.HasValue.MustNotBe(false, exception: () => new InvalidOperationException("Cannot get the ID when the reference is not deferred."));

                // ReSharper disable once PossibleInvalidOperationException
                return _refId.Value;
            }
        }

        /// <summary>
        ///     Creates a new instance of ParseResult from completely parsed value.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="parsedValue" /> is null.</exception>
        public static ParseResult FromParsedValue(object parsedValue)
        {
            parsedValue.MustNotBeNull(nameof(parsedValue));

            return new ParseResult(parsedValue, null);
        }

        /// <summary>
        ///     Creates a new instance of ParseResult from the id of a deferred reference.
        /// </summary>
        public static ParseResult FromDeferredReference(int refId)
        {
            refId.MustNotBeLessThan(0, nameof(refId));

            return new ParseResult(null, refId);
        }

        /// <summary>
        ///     Creates a new instance of ParseResult that represents null.
        /// </summary>
        public static ParseResult FromNull()
        {
            return new ParseResult(null, null);
        }
    }
}