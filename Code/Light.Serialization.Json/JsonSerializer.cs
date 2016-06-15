using System;
using System.Collections.Generic;
using System.IO;
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
        private List<object> _serializedObjects;

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
        /// <param name="objectGraphRoot">The object that is the starting point of the object graph.</param>
        /// <returns>The serialized JSON string.</returns>
        /// <exception cref="SerializationException">Thrown when any part of the object graph could not be serialized.</exception>
        public string Serialize(object objectGraphRoot)
        {
           _jsonWriter = _writerFactory.Create();
            _serializedObjects = new List<object>();
            SerializeObject(objectGraphRoot);

            var json = _writerFactory.FinishWriteProcessAndReleaseResources();
            _jsonWriter = null;
            _serializedObjects = null;
            return json;
        }

        public void Serialize(object objectGraphRoot, Stream stream)
        {
            throw new NotImplementedException();
        }

        private void SerializeObject(object @object)
        {
            if (@object == null)
            {
                _jsonWriter.WriteNull();
                return;
            }

            var actualType = @object.GetType();
            IJsonWriterInstructor targetWriterInstructor;
            if (_instructorCache.TryGetValue(actualType, out targetWriterInstructor) == false)
            {
                targetWriterInstructor = FindTargetInstructor(@object, actualType);
                if (targetWriterInstructor == null)
                    throw new SerializationException($"Type {actualType.FullName} cannot be serialized because there is no IJsonWriterInstructor registered that can cover this type.");

                _instructorCache.Add(actualType, targetWriterInstructor);
            }

            targetWriterInstructor.Serialize(new JsonSerializationContext(@object, actualType, SerializeObject, _jsonWriter, _serializedObjects));
        }

        private IJsonWriterInstructor FindTargetInstructor(object @object, Type objectType)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var writerInstructor in _writerInstructors)
            {
                if (writerInstructor.IsSuitableFor(@object, objectType))
                    return writerInstructor;
            }
            return null;
        }
    }
}