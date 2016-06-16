using System;
using System.Linq;
using Light.GuardClauses;

namespace Light.Serialization.Json.BuilderHelpers
{
    /// <summary>
    ///     Represents the base class for builders that want to incorporate a property injection pool.
    /// </summary>
    public abstract class BaseBuilderWithPropertyInjectionPool<TBuilder> where TBuilder : BaseBuilderWithPropertyInjectionPool<TBuilder>
    {
        private readonly TBuilder _this;

        /// <summary>
        ///     Gets the property injection pool.
        /// </summary>
        protected readonly PropertyInjectionPool Pool = new PropertyInjectionPool();

        /// <summary>
        ///     Initializes this instance of <see cref="BaseBuilderWithPropertyInjectionPool{TBuilder}" />.
        /// </summary>
        protected BaseBuilderWithPropertyInjectionPool()
        {
            _this = (TBuilder) this;
        }

        /// <summary>
        ///     Configures the specified instance with the given delegate.
        /// </summary>
        /// <typeparam name="T">The type of the instance you want to configure.</typeparam>
        /// <param name="configureInstance">The delegate that configures the instance (usually a lambda performing property injection).</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <see cref="configureInstance" /> is null.</exception>
        public TBuilder Configure<T>(Action<T> configureInstance)
        {
            configureInstance.MustNotBeNull(nameof(configureInstance));
            configureInstance(Pool.Objects.OfType<T>().First());
            return _this;
        }

        /// <summary>
        ///     Configures all instances of the specified type with the given delegate.
        /// </summary>
        /// <typeparam name="T">The type whose instances you want to configure.</typeparam>
        /// <param name="configureInstance">The delegate that configures a single instance (usually a lambda performing property injection).</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configureInstance" /> is null.</exception>
        public TBuilder ConfigureAll<T>(Action<T> configureInstance)
        {
            configureInstance.MustNotBeNull(nameof(configureInstance));

            foreach (var @object in Pool.Objects.OfType<T>())
            {
                configureInstance(@object);
            }

            return _this;
        }

        /// <summary>
        ///     Configures all instances that apply to the given predicate using the <paramref name="configureInstance" /> delegate.
        /// </summary>
        /// <typeparam name="T">The type whose instances you want to configure.</typeparam>
        /// <param name="predicate">The delegate that filters the objects that should be configured. If this delegate returns false, the object will not be included.</param>
        /// <param name="configureInstance">The delegate that configures a single instance (usually a lambda performing property injection).</param>
        /// <returns>The builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate" /> or <paramref name="configureInstance" /> is null.</exception>
        public TBuilder ConfigureWhere<T>(Func<T, bool> predicate, Action<T> configureInstance)
        {
            predicate.MustNotBeNull(nameof(predicate));
            configureInstance.MustNotBeNull(nameof(configureInstance));

            foreach (var @object in Pool.Objects.OfType<T>().Where(predicate))
            {
                configureInstance(@object);
            }

            return _this;
        }
    }
}