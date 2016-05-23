using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.Serialization.Json.ComplexTypeConstruction;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Represents the object that is used for object reference preservation and setting deferred references.
    /// </summary>
    public sealed class ObjectReferencePreserver
    {
        private readonly Dictionary<int, List<DeferredReferenceInfo>> _deferredReferences = new Dictionary<int, List<DeferredReferenceInfo>>();
        private readonly Dictionary<int, object> _deserializedObjects = new Dictionary<int, object>();

        /// <summary>
        ///     Adds a deserialized object to the Object Reference Preserver.
        /// </summary>
        /// <param name="id">The JSON document ID of the deserialized object.</param>
        /// <param name="object">The deserialized object</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="id" /> is less than zero.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="object" /> is null.</exception>
        public void AddDeserializedObject(int id, object @object)
        {
            id.MustNotBeLessThan(0, nameof(id));
            @object.MustNotBeNull(nameof(@object));

            _deserializedObjects.Add(id, @object);
        }

        /// <summary>
        ///     Tries to get the associated object with the specified id.
        /// </summary>
        /// <param name="id">The id of the deserialized object.</param>
        /// <param name="object">When this method returns, the associated object is assigned to this parameter, or this parameter contains the default value when the object could not be found.</param>
        /// <returns>True if the associated object was found, else false.</returns>
        public bool TryGetDeserializedObject(int id, out object @object)
        {
            return _deserializedObjects.TryGetValue(id, out @object);
        }

        /// <summary>
        ///     Adds a deferred reference that will be resolved as soon as the corresponding object is added to the ObjectReferencePreserver.
        /// </summary>
        /// <param name="referenceId">The id of the deferred reference.</param>
        /// <param name="injectableValueDescription">The injectable value description that allows Field and / or Property Injection on the target object, when the deferred reference is available.</param>
        /// <param name="targetObject">The object the deferred reference will be set on.</param>
        public void AddDeferredReference(int referenceId, InjectableValueDescription injectableValueDescription, object targetObject)
        {
            List<DeferredReferenceInfo> targetList;
            var deferredReferenceInfo = new DeferredReferenceInfo(referenceId, injectableValueDescription, targetObject);
            if (_deferredReferences.TryGetValue(referenceId, out targetList) == false)
            {
                targetList = new List<DeferredReferenceInfo>();
                _deferredReferences.Add(referenceId, targetList);
            }

            targetList.Add(deferredReferenceInfo);
        }
    }
}