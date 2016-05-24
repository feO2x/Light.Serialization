using System;
using System.Diagnostics;
using Light.GuardClauses;
using Light.Serialization.Abstractions;
using Light.Serialization.Json.ComplexTypeConstruction;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents the structure containing all infos to set a deferred reference on a target object.
    /// </summary>
    public sealed class DeferredReferenceForComplexObject : IDeferredReference
    {
        private readonly InjectableValueDescription _injectableValueDescription;
        private readonly object _targetObject;

        /// <summary>
        ///     Creates a new instance of DeferredReferenceInfo.
        /// </summary>
        /// <param name="referenceId">The id of the deferred reference.</param>
        /// <param name="injectableValueDescription">The injectable value description used to set the value later.</param>
        /// <param name="targetObject">The object the reference will be set on.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="referenceId" /> is less than zero.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="injectableValueDescription" /> or <paramref name="targetObject" /> is null.</exception>
        /// <exception cref="DeserializationException">Thrown when the <paramref name="injectableValueDescription" /> does not allow Property or Field injection.</exception>
        public DeferredReferenceForComplexObject(int referenceId, InjectableValueDescription injectableValueDescription, object targetObject)
        {
            referenceId.MustNotBeLessThan(0, nameof(referenceId));
            injectableValueDescription.MustNotBeNull(nameof(injectableValueDescription));
            targetObject.MustNotBeNull(nameof(targetObject));
            CheckInjectableValueDescriptionKind(injectableValueDescription, targetObject);

            ReferenceId = referenceId;
            _injectableValueDescription = injectableValueDescription;
            _targetObject = targetObject;
        }

        /// <summary>
        ///     Gets the ID of the object that is not fully deserialized yet.
        /// </summary>
        public int ReferenceId { get; }

        /// <summary>
        ///     Sets the deferred reference on the target object.
        /// </summary>
        /// <param name="reference">The object that should be set on the target object.</param>
        public void SetDeferredReference(object reference)
        {
            if (_injectableValueDescription.PropertyInfo != null)
                _injectableValueDescription.SetPropertyValue(_targetObject, reference);
            else
                _injectableValueDescription.SetFieldValue(_targetObject, reference);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckInjectableValueDescriptionKind(InjectableValueDescription injectableValueDescription, object targetObject)
        {
            if (injectableValueDescription.FieldInfo == null && injectableValueDescription.PropertyInfo == null)
                throw new DeserializationException($"The injectable value description {injectableValueDescription.NormalizedName} for type {targetObject.GetType()} does not support Property Injection or Field Injection and thus cannot be used for setting deferred references.");
        }
    }
}