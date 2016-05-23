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
    public struct DeferredReferenceInfo
    {
        /// <summary>
        ///     Gets the ID of the deferred reference.
        /// </summary>
        public readonly int DeferredReferenceId;

        /// <summary>
        ///     Gets the injectable value description for the deferred object to be set.
        /// </summary>
        public readonly InjectableValueDescription InjectableValueDescription;

        /// <summary>
        ///     Gets the object the reference will be set on.
        /// </summary>
        public readonly object TargetObject;

        /// <summary>
        ///     Creates a new instance of DeferredReferenceInfo.
        /// </summary>
        /// <param name="deferredReferenceId">The id of the deferred reference.</param>
        /// <param name="injectableValueDescription">The injectable value description used to set the value later.</param>
        /// <param name="targetObject">The object the reference will be set on.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="deferredReferenceId" /> is less than zero.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="injectableValueDescription" /> or <paramref name="targetObject" /> is null.</exception>
        /// <exception cref="DeserializationException">Thrown when the <paramref name="injectableValueDescription" /> does not allow Property or Field injection.</exception>
        public DeferredReferenceInfo(int deferredReferenceId, InjectableValueDescription injectableValueDescription, object targetObject)
        {
            deferredReferenceId.MustNotBeLessThan(0, nameof(deferredReferenceId));
            injectableValueDescription.MustNotBeNull(nameof(injectableValueDescription));
            targetObject.MustNotBeNull(nameof(targetObject));
            CheckInjectableValueDescriptionKind(injectableValueDescription, targetObject);

            DeferredReferenceId = deferredReferenceId;
            InjectableValueDescription = injectableValueDescription;
            TargetObject = targetObject;
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckInjectableValueDescriptionKind(InjectableValueDescription injectableValueDescription, object targetObject)
        {
            if (injectableValueDescription.FieldInfo == null && injectableValueDescription.PropertyInfo == null)
                throw new DeserializationException($"The injectable value description {injectableValueDescription.NormalizedName} for type {targetObject.GetType()} does not support Property Injection or Field Injection and thus cannot be used for setting deferred references.");
        }

        /// <summary>
        ///     Sets the deferred reference on the target object.
        /// </summary>
        /// <param name="reference">The object that should be set on the target object.</param>
        public void SetDeferredReference(object reference)
        {
            if (InjectableValueDescription.PropertyInfo != null)
                InjectableValueDescription.SetPropertyValue(TargetObject, reference);
            else
                InjectableValueDescription.SetFieldValue(TargetObject, reference);
        }
    }
}