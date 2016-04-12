using System;
using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.Serialization.Json.ObjectMetadata
{
    /// <summary>
    ///     Contains an extension method for checking reference equality for a list of objects.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        ///     Finds the index of the object in the list that is the same one as the specified reference.
        /// </summary>
        /// <param name="list">The list to be checked</param>
        /// <param name="reference">The object reference used for comparison.</param>
        /// <returns>The index of the object that corresponds to the specified reference, or -1 if the object could not be found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="list" /> is null.</exception>
        public static int GetIndexOfSame(this List<object> list, object reference)
        {
            list.MustNotBeNull(nameof(list));

            for (var i = 0; i < list.Count; i++)
            {
                if (ReferenceEquals(list[i], reference))
                    return i;
            }
            return -1;
        }
    }
}