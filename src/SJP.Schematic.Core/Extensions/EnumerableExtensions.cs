﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SJP.Schematic.Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool Empty<T>(this IEnumerable<T> source) => !source.Any();

        public static bool Empty<T>(this IEnumerable<T> source, Func<T, bool> predicate) => !source.Any(predicate);

        public static bool AnyNull<T>(this IEnumerable<T> source) where T : class => source.Any(x => x == null);

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) => source.DistinctBy(keySelector, null);

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
           Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            return _(); IEnumerable<TSource> _()
            {
                var knownKeys = new HashSet<TKey>(comparer);
                foreach (var element in source)
                {
                    if (knownKeys.Add(keySelector(element)))
                        yield return element;
                }
            }
        }
    }
}