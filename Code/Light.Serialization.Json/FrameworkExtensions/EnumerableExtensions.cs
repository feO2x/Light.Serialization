using System;
using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.Serialization.Json.FrameworkExtensions
{
    /// <summary>
    ///     Provides extension methods for the <see cref="IEnumerable{T}" /> interface.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        ///     Gets the index of the first item that corresponds to the given predicate.
        /// </summary>
        /// <typeparam name="T">The item type of the collection.</typeparam>
        /// <param name="collection">The collection to be enumerated.</param>
        /// <param name="predicate">The delegate that selects the item whose index will be returned.</param>
        /// <returns>The index of the first item that corresponds to the predicate in the collection, or -1, if no item could be found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="collection" /> or <paramref name="predicate" /> is null.</exception>
        public static int IndexOf<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
        {
            // ReSharper disable PossibleMultipleEnumeration
            collection.MustNotBeNull(nameof(collection));
            predicate.MustNotBeNull(nameof(predicate));

            var currentIndex = 0;
            foreach (var item in collection)
            {
                if (predicate(item))
                    return currentIndex;
                ++currentIndex;
            }
            return -1;
            // ReSharper restore PossibleMultipleEnumeration
        }
    }
}