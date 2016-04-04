using System;
using Light.GuardClauses;
using Light.Serialization.Json.SerializationRules;
using Microsoft.Practices.Unity;

namespace Light.Serialization.Json.Unity
{
    /// <summary>
    /// Provides custom serialization rule support for the Unity container.
    /// </summary>
    public static class RulesSupport
    {
        /// <summary>
        ///     Creates a serialization rule for the given type that is configured with the specified delegate.
        /// </summary>
        /// <typeparam name="T">The type that should be configured for serialization.</typeparam>
        /// <param name="container">The Unity container.</param>
        /// <param name="configureRule">The delegate that configures the actual formatter instance.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public static IUnityContainer WithSerializationRuleFor<T>(this IUnityContainer container, Action<Rule<T>> configureRule)
        {
            container.MustNotBeNull(nameof(container));
            configureRule.MustNotBeNull(nameof(configureRule));

            var newRule = container.Resolve<Rule<T>>();
            configureRule(newRule);
            var customInstructor = newRule.CreateInstructor();
            container.RegisterInstance(typeof (T).FullName, customInstructor);
            return container;
        }

        /// <summary>
        ///     Configures the Unity container to use the specified rule for serialization.
        /// </summary>
        /// <typeparam name="T">The type that should be configured for serialization.</typeparam>
        /// <param name="container">The Unity container.</param>
        /// <param name="rule">The rule that describes how the type is serialized.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters is null.</exception>
        public static IUnityContainer WithSerializationRuleFor<T>(this IUnityContainer container, Rule<T> rule)
        {
            container.MustNotBeNull(nameof(container));
            rule.MustNotBeNull(nameof(rule));

            var customInstructor = rule.CreateInstructor();
            container.RegisterInstance(typeof (T).FullName, customInstructor);
            return container;
        }
    }
}