namespace Light.Serialization.Json.ComplexTypeConstruction
{
    /// <summary>
    ///     Represents a wrapper for a value that will be injected into
    ///     another object, additionaly providing a boolean indicating
    ///     whether the value has been injected.
    /// </summary>
    public struct InjectableValue
    {
        /// <summary>
        ///     Gets the value that should be injected.
        /// </summary>
        public readonly object Value;

        private bool _hasBeenInjectedBefore;

        /// <summary>
        ///     Initializes a new instance of <see cref="InjectableValue" />.
        /// </summary>
        /// <param name="value">The value to be injected.</param>
        public InjectableValue(object value)
        {
            Value = value;
            _hasBeenInjectedBefore = false;
        }

        /// <summary>
        ///     Gets the value indicating whether the value has been injected before, usually via constructor injection.
        /// </summary>
        public bool HasBeenInjectedBefore => _hasBeenInjectedBefore;

        /// <summary>
        ///     Returns the object and sets <see cref="HasBeenInjectedBefore" /> to true.
        /// </summary>
        public object Inject()
        {
            _hasBeenInjectedBefore = true;
            return Value;
        }
    }
}