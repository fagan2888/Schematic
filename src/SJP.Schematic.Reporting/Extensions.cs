﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using System.Data;
using Dapper;

namespace SJP.Schematic.Reporting
{
    internal static class CollectionExtensions
    {
        public static IEnumerable<TResult> SelectNotNull<T, TResult>(this IEnumerable<T> input, Func<T, TResult> selector)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            var result = new List<TResult>();

            foreach (var item in input)
            {
                var mapResult = selector(item);
                if (!ReferenceEquals(mapResult, null))
                    result.Add(mapResult);
            }

            return result;
        }

        public static Task<IEnumerable<TResult>> SelectNotNullAsync<T, TResult>(this IEnumerable<T> input, Func<T, Task<TResult>> selector)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return SelectNotNullAsyncCore(input, selector);
        }

        private static async Task<IEnumerable<TResult>> SelectNotNullAsyncCore<T, TResult>(IEnumerable<T> input, Func<T, Task<TResult>> selector)
        {
            var tasks = input.Select(selector).ToArray();
            var completedTasks = await Task.WhenAll(tasks).ConfigureAwait(false);
            return completedTasks.Where(item => !ReferenceEquals(item, null)).ToList();
        }

        public static Task<IEnumerable<TResult>> SelectManyAsync<T, TResult>(this IEnumerable<T> input, Func<T, Task<IEnumerable<TResult>>> selector)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return SelectManyAsyncCore(input, selector);
        }

        private static async Task<IEnumerable<TResult>> SelectManyAsyncCore<T, TResult>(IEnumerable<T> input, Func<T, Task<IEnumerable<TResult>>> selector)
        {
            var tasks = input.Select(selector).ToArray();
            var completedTasks = await Task.WhenAll(tasks).ConfigureAwait(false);
            return completedTasks.SelectMany(x => x).ToList();
        }

        public static uint UCount<T>(this IReadOnlyCollection<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            var count = collection.Count;
            if (count < 0)
                throw new ArgumentException("The given collection has a negative count. This is not supported.", nameof(collection));

            return (uint)count;
        }

        public static uint UCount<T>(this IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            var count = collection.Count();
            if (count < 0)
                throw new ArgumentException("The given collection has a negative count. This is not supported.", nameof(collection));

            return (uint)count;
        }

        public static ulong ULongCount<T>(this IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            var count = collection.LongCount();
            if (count < 0)
                throw new ArgumentException("The given collection has a negative count. This is not supported.", nameof(collection));

            return (ulong)count;
        }
    }

    internal static class ConnectionExtensions
    {
        public static ulong GetRowCount(this IDbConnection connection, IDatabaseDialect dialect, Identifier tableName)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var name = Identifier.CreateQualifiedIdentifier(tableName.Schema, tableName.LocalName);
            var quotedName = dialect.QuoteName(name);

            var query = "SELECT COUNT(*) FROM " + quotedName;
            return connection.ExecuteScalar<ulong>(query);
        }

        public static Task<ulong> GetRowCountAsync(this IDbConnection connection, IDatabaseDialect dialect, Identifier tableName)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var name = Identifier.CreateQualifiedIdentifier(tableName.Schema, tableName.LocalName);
            var quotedName = dialect.QuoteName(name);

            var query = "SELECT COUNT(*) FROM " + quotedName;
            return connection.ExecuteScalarAsync<ulong>(query);
        }
    }

    internal static class DatabaseKeyExtensions
    {
        public static int GetKeyHash(this IDatabaseKey key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            unchecked
            {
                var hash = 17;
                hash = (hash * 23) + key.Table.Name.GetHashCode();
                hash = (hash * 23) + key.KeyType.GetHashCode();
                foreach (var column in key.Columns)
                    hash = (hash * 23) + (column.Name?.LocalName?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}
