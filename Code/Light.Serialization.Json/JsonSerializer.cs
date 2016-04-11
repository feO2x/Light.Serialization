using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;
using Light.Serialization.Abstractions;
using Light.Serialization.Json.LowLevelWriting;
using Light.Serialization.Json.WriterInstructors;

namespace Light.Serialization.Json
{
    /// <summary>
    ///     Represents an object that can serialize object graphs to JSON strings.
    /// </summary>
    public sealed class JsonSerializer : ISerializer
    {
        private readonly IDictionary<Type, IJsonWriterInstructor> _instructorCache;
        private readonly IJsonWriterFactory _writerFactory;
        private readonly IReadOnlyList<IJsonWriterInstructor> _writerInstructors;
        private IJsonWriter _jsonWriter;

        /// <summary>
        ///     Creates a new instance of <see cref="JsonSerializer" />.
        /// </summary>
        /// <param name="writerInstructors">The writer instructors that serialize different types in different ways.</param>
        /// <param name="writerFactory">The factory that is able to create a JSON writer.</param>
        /// <param name="instructorCache">The cache holding writer instructors for faster access. This chache may be empty.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters is null.</exception>
        /// <exception cref="EmptyCollectionException">Thrown when <paramref name="writerInstructors" /> is empty.</exception>
        public JsonSerializer(IReadOnlyList<IJsonWriterInstructor> writerInstructors,
                              IJsonWriterFactory writerFactory,
                              IDictionary<Type, IJsonWriterInstructor> instructorCache)
        {
            writerInstructors.MustNotBeNullOrEmpty(nameof(writerInstructors));
            writerFactory.MustNotBeNull(nameof(writerFactory));
            instructorCache.MustNotBeNull(nameof(instructorCache));

            _writerInstructors = writerInstructors;
            _writerFactory = writerFactory;
            _instructorCache = instructorCache;
        }

        /// <summary>
        /// Serializes the specified object graph to a JSON string.
        /// </summary>
        /// <typeparam name="T">The type of the object graph root.</typeparam>
        /// <param name="objectGraphRoot">The object that is the starting point of the object graph.</param>
        /// <returns>The serialized JSON string.</returns>
        /// <exception cref="SerializationException">Thrown when any part of the object graph could not be serialized.</exception>
        public string Serialize<T>(T objectGraphRoot)
        {
            return Serialize(objectGraphRoot, typeof (T));
        }

        /// <summary>
        /// Serializes the specified object graph to a JSON string.
        /// </summary>
        /// <param name="objectGraphRoot">The object that is the starting point of the object graph.</param>
        /// <param name="referencedType">The type that is used to reference the object graph root.</param>
        /// <returns>The serialized JSON string.</returns>
        /// <exception cref="SerializationException">Thrown when any part of the object graph could not be serialized.</exception>
        public string Serialize(object objectGraphRoot, Type referencedType)
        {
            objectGraphRoot.MustNotBeNull(nameof(objectGraphRoot));
            referencedType.MustNotBeNull(nameof(referencedType));

            _jsonWriter = _writerFactory.Create();
            SerializeObject(objectGraphRoot, objectGraphRoot.GetType(), referencedType);

            var json = _writerFactory.FinishWriteProcessAndReleaseResources();
            _jsonWriter = null;
            return json;
        }

        private void SerializeObject(object @object, Type actualType, Type referencedType)
        {
            IJsonWriterInstructor targetWriterInstructor;
            if (_instructorCache.TryGetValue(actualType, out targetWriterInstructor) == false)
            {
                targetWriterInstructor = FindTargetInstructor(@object, actualType, referencedType);
                if (targetWriterInstructor == null)
                    throw new SerializationException($"Type {actualType.FullName} cannot be serialized because there is no IJsonWriterInstructor registered that can cover this type.");

                _instructorCache.Add(actualType, targetWriterInstructor);
            }

            targetWriterInstructor.Serialize(new JsonSerializationContext(@object, actualType, referencedType, SerializeObject, _jsonWriter));
        }

        private IJsonWriterInstructor FindTargetInstructor(object @object, Type objectType, Type referencedType)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var writerInstructor in _writerInstructors)
            {
                if (writerInstructor.IsSuitableFor(@object, objectType, referencedType))
                    return writerInstructor;
            }
            return null;
        }
    }
}