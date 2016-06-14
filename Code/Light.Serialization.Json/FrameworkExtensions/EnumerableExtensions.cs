using System;
using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.Serialization.Json.FrameworkExtensions
{
    /// <summary>
    ///     Contains extension methods for the <see cref="IEnumerable{T}" /> type.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        ///     Performs the specified action on all items in the collection.
        /// </summary>
        /// <typeparam name="T">The item type of the collection.</typeparam>
        /// <param name="collection">The collection containing the items where the action is applied to.</param>
        /// <param name="action">The delegate that is executed for every collection item.</param>
        /// <returns>The collection for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="collection"/> or <paramref name="action"/> is null.</exception>
        public static IEnumerable<T> Do<T>(this IEnumerable<T> collection, Action<T> action)
        {
            // ReSharper disable PossibleMultipleEnumeration
            collection.MustNotBeNull(nameof(collection));
            action.MustNotBeNull(nameof(action));

            foreach (var item in collection)
            {
                action(item);
            }

            return collection;
            // ReSharper restore PossibleMultipleEnumeration
        }
    }
}