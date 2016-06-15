﻿using System.Collections.Generic;
using System.Linq;

namespace Light.Serialization.Json.BuilderHelpers
{
    public sealed class PropertyInjectionPool
    {
        private readonly List<object> _objects;

        public PropertyInjectionPool() : this(new List<object>()) { }

        public PropertyInjectionPool(List<object> objects)
        {
            _objects = objects;
        }

        public IReadOnlyList<object> Objects => _objects;

        public T Register<T>(T @object)
        {
            _objects.Add(@object);
            return @object;
        }

        public PropertyInjectionPool RegisterAll<T>(IEnumerable<T> objects)
        {
            foreach (var @object in objects)
            {
                _objects.Add(@object);
            }
            return this;
        }

        public PropertyInjectionPool Remove(object @object)
        {
            _objects.Remove(@object);
            return this;
        }

        public PropertyInjectionPool RemoveAll<T>(IEnumerable<T> objects)
        {
            foreach (var @object in objects)
            {
                _objects.Remove(@object);
            }
            return this;
        }

        public PropertyInjectionPool SetFieldAndReplaceInPool<T>(out T field, T value)
        {
            field = value;
            Remove(field);
            Register(value);
            return this;
        }

        public IEnumerable<T> AndUpdateObjectsOfType<T>()
        {
            return Objects.OfType<T>();
        }
    }
}