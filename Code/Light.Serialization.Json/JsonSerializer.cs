using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;
using Light.Serialization.Abstractions;
using Light.Serialization.Json.LowLevelWriting;
using Light.Serialization.Json.WriterInstructors;

namespace Light.Serialization.Json
{
    /// <summary>
    ///     Represents the default <see cref="ISerializer" /> implementation for JSON documents
    ///     that uses a recursive algorithm incorporating <see cref="IJsonWriterInstructor" /> instances.
    ///     Each <see cref="IJsonWriterInstructor" /> is designed for a specific .NET type and knows
    ///     how to instruct a <see cref="IJsonWriter" /> to write the type's serialized form to the
    ///     JSON document.
    /// </summary>
    public sealed class JsonSerializer : ISerializer
    {
        /// <summary>
        ///     Gets the default limit for recursion which is 300 levels.
        /// </summary>
        public const int DefaultRecursionLevelLimit = 300;

        /// <summary>
        ///     Gets the minimum level limit for recursion which is 3.
        /// </summary>
        public const int MinimumRecursionLevelLimit = 3;

        private readonly IDictionary<Type, IJsonWriterInstructor> _instructorCache;
        private readonly IJsonWriterFactory _writerFactory;
        private readonly IReadOnlyList<IJsonWriterInstructor> _writerInstructors;
        private IJsonWriter _jsonWriter;
        private int _recursionLevel;

        private int _recursionLevelLimit = DefaultRecursionLevelLimit;
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
        ///     Gets or sets the limit where the JSON serializer ends the recursive algorithm. This value defaults to 300.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value" /> is less than 3.</exception>
        public int RecursionLevelLimit
        {
            get { return _recursionLevelLimit; }
            set
            {
                value.MustNotBeLessThan(MinimumRecursionLevelLimit, nameof(value));
                _recursionLevelLimit = value;
            }
        }

        /// <summary>
        ///     Serializes the specified object graph to a string.
        /// </summary>
        /// <param name="objectGraphRoot">The object that is the starting point of the object graph.</param>
        /// <returns>The serialized JSON string.</returns>
        /// <exception cref="SerializationException">Thrown when any part of the object graph could not be serialized.</exception>
        public string Serialize(object objectGraphRoot)
        {
            var stringBuilder = new StringBuilder();
            _jsonWriter = _writerFactory.CreateFromStringBuilder(stringBuilder);
            _serializedObjects = new List<object>();
            SerializeObject(objectGraphRoot);

            var json = stringBuilder.ToString();
            _jsonWriter = null;
            _serializedObjects = null;
            return json;
        }

        /// <summary>
        ///     Serializes the specified object graph to the stream encapsulated by the text writer.
        /// </summary>
        /// <param name="objectGraphRoot">The root of the object graph to be serialized.</param>
        /// <param name="textWriter">The text writer encapsulating the stream that the JSON document is written to.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="textWriter" /> is null.</exception>
        /// <exception cref="SerializationException">Thrown when any part of the object graph could not be serialized.</exception>
        public void Serialize(object objectGraphRoot, TextWriter textWriter)
        {
            _jsonWriter = _writerFactory.CreateFromTextWriter(textWriter);
            _serializedObjects = new List<object>();

            SerializeObject(objectGraphRoot);

            _jsonWriter.Dispose();
            _jsonWriter = null;
            _serializedObjects = null;
        }

        /// <summary>
        ///     Serializes the specified object graph to the stream encapsulated by the binary writer.
        /// </summary>
        /// <param name="objectGraphRoot">The root of the object graph to be serialized.</param>
        /// <param name="binaryWriter">The binary writer encapsulating the stream that JSON document is written to.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="binaryWriter" /> is null.</exception>
        /// <exception cref="SerializationException">Thrown when any part of the object graph could not be serialized.</exception>
        public void Serialize(object objectGraphRoot, BinaryWriter binaryWriter)
        {
            _jsonWriter = _writerFactory.CreateFromBinaryWriter(binaryWriter);
            _serializedObjects = new List<object>();

            SerializeObject(objectGraphRoot);

            _jsonWriter.Dispose();
            _jsonWriter = null;
            _serializedObjects = null;
        }

        private void SerializeObject(object @object)
        {
            if (++_recursionLevel == _recursionLevelLimit)
                throw new SerializationException($"The serializer probably is in an endless recursion - therefore the serialization process was stopped at recursion level {_recursionLevelLimit}.");

            if (@object == null)
            {
                _jsonWriter.WriteNull();
                return;
            }

            var actualType = @object.GetType();
            IJsonWriterInstructor targetWriterInstructor;
            lock (_instructorCache)
            {
                if (_instructorCache.TryGetValue(actualType, out targetWriterInstructor) == false)
                {
                    foreach (var writerInstructor in _writerInstructors)
                    {
                        if (writerInstructor.IsSuitableFor(@object, actualType) == false)
                            continue;
                        targetWriterInstructor = writerInstructor;
                        goto AddInstructorToCache;
                    }

                    throw new SerializationException($"Type {actualType.FullName} cannot be serialized because there is no IJsonWriterInstructor registered that can cover this type.");

                    AddInstructorToCache:
                    _instructorCache.Add(actualType, targetWriterInstructor);
                }
            }

            targetWriterInstructor.Serialize(new JsonSerializationContext(@object, actualType, SerializeObject, _jsonWriter, _serializedObjects));
            --_recursionLevel;
        }
    }
}