using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;

namespace Light.Serialization.Json.BuilderHelpers
{
    /// <summary>
    ///     Represents a pool of objects whose instances can be easily configured via delegates that usually perform property injection.
    /// </summary>
    public sealed class PropertyInjectionPool
    {
        private readonly List<object> _objects;

        /// <summary>
        ///     Creates a new instance of <see cref="PropertyInjectionPool" />.
        /// </summary>
        public PropertyInjectionPool() : this(new List<object>()) { }

        /// <summary>
        ///     Creates a new instance of <see cref="PropertyInjectionPool" /> with the given list of objects.
        /// </summary>
        /// <param name="objects">The list that contains the objects that are managed by the pool.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="objects" /> is null.</exception>
        public PropertyInjectionPool(List<object> objects)
        {
            objects.MustNotBeNull(nameof(objects));

            _objects = objects;
        }

        /// <summary>
        ///     Gets a read-only list of the objects that are managed by the pool.
        /// </summary>
        public IReadOnlyList<object> Objects => _objects;

        /// <summary>
        ///     Registers the specified object with the pool.
        /// </summary>
        /// <typeparam name="T">The type of the object that will be registered.</typeparam>
        /// <param name="object">The object to be registered.</param>
        /// <returns>The object that will be registered.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="object" /> is null.</exception>
        public T Register<T>(T @object) where T : class
        {
            @object.MustNotBeNull(nameof(@object));

            _objects.Add(@object);
            return @object;
        }

        /// <summary>
        ///     Registers all specified objects with the pool.
        /// </summary>
        /// <typeparam name="T">The type of the objects that will be registered.</typeparam>
        /// <param name="objects">The objects to be registered.</param>
        /// <returns>The pool for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="objects" /> is null.</exception>
        /// <exception cref="CollectionException">Thrown when <paramref name="objects" /> contains null.</exception>
        public PropertyInjectionPool RegisterAll<T>(IEnumerable<T> objects) where T : class
        {
            // ReSharper disable PossibleMultipleEnumeration
            objects.MustNotContainNull(nameof(objects));

            foreach (var @object in objects)
            {
                _objects.Add(@object);
            }
            return this;
            // ReSharper restore PossibleMultipleEnumeration
        }

        /// <summary>
        ///     Removes the specified object from the pool.
        /// </summary>
        /// <param name="object">The object to be removed.</param>
        /// <returns>The pool for method chaining.</returns>
        public PropertyInjectionPool Remove<T>(T @object) where T : class
        {
            _objects.Remove(@object);
            return this;
        }

        /// <summary>
        ///     Removes all specified objects from the pool.
        /// </summary>
        /// <typeparam name="T">The type of the objects to be removed.</typeparam>
        /// <param name="objects">The objects to be removed.</param>
        /// <returns>The pool for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="objects" /> is null.</exception>
        public PropertyInjectionPool RemoveAll<T>(IEnumerable<T> objects)
        {
            // ReSharper disable PossibleMultipleEnumeration
            objects.MustNotBeNull(nameof(objects));

            foreach (var @object in objects)
            {
                _objects.Remove(@object);
            }
            return this;
            // ReSharper restore PossibleMultipleEnumeration
        }

        /// <summary>
        ///     Sets the field with the specified value and adds it to the pool. The old value is removed from the pool.
        /// </summary>
        /// <typeparam name="T">The type of the field and value.</typeparam>
        /// <param name="field">The field to be set.</param>
        /// <param name="value">The new value for the field.</param>
        /// <returns>The pool for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        public PropertyInjectionPool SetFieldAndReplaceInPool<T>(ref T field, T value) where T : class
        {
            Remove(field);
            Register(value);
            field = value;
            return this;
        }
    }
}