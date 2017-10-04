﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Query;
using SJP.Schematic.Sqlite.Parsing;

namespace SJP.Schematic.Sqlite
{
    public class SqliteRelationalDatabase : RelationalDatabase, IRelationalDatabase
    {
        public SqliteRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection)
            : base(dialect, connection)
        {
            Metadata = new DatabaseMetadata { DatabaseName = connection.Database };
        }

        public string ServerName => null; // never not-null

        public string DatabaseName => Metadata.DatabaseName;

        public string DefaultSchema => Metadata.DefaultSchema;

        protected DatabaseMetadata Metadata { get; }

        #region Tables

        public bool TableExists(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (BuiltInTables.Contains(tableName.LocalName))
                return false;

            const string sql = "select count(*) from sqlite_master where type = 'table' and lower(name) = lower(@TableName)";
            return Connection.ExecuteScalar<int>(
                sql,
                new { TableName = tableName.LocalName }
            ) != 0;
        }

        public async Task<bool> TableExistsAsync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (BuiltInTables.Contains(tableName.LocalName))
                return false;

            const string sql = "select count(*) from sqlite_master where type = 'table' and lower(name) = lower(@TableName)";
            return await Connection.ExecuteScalarAsync<int>(
                sql,
                new { TableName = tableName.LocalName }
            ).ConfigureAwait(false) != 0;
        }

        public IRelationalDatabaseTable GetTable(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (BuiltInTables.Contains(tableName.LocalName))
                return null;

            return LoadTableSync(tableName.LocalName);
        }

        public Task<IRelationalDatabaseTable> GetTableAsync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (BuiltInTables.Contains(tableName.LocalName))
                return null;

            return LoadTableAsync(tableName.LocalName);
        }

        public IEnumerable<IRelationalDatabaseTable> Tables
        {
            get
            {
                const string sql = "select name from sqlite_master where type = 'table' order by name";
                var tableNames = Connection.Query<string>(sql)
                    .Where(name => !BuiltInTables.Contains(name))
                    .Select(name => new LocalIdentifier(name));

                foreach (var tableName in tableNames)
                    yield return LoadTableSync(tableName);
            }
        }

        public async Task<IAsyncEnumerable<IRelationalDatabaseTable>> TablesAsync()
        {
            const string sql = "select name from sqlite_master where type = 'table' order by name";
            var queryResult = await Connection.QueryAsync<string>(sql).ConfigureAwait(false);
            var tableNames = queryResult
                .Where(name => !BuiltInTables.Contains(name))
                .Select(name => new LocalIdentifier(name));

            return tableNames
                .Select(LoadTableSync)
                .ToAsyncEnumerable();
        }

        protected virtual IRelationalDatabaseTable LoadTableSync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            return TableExists(tableName.LocalName)
                ? new SqliteRelationalDatabaseTable(Connection, this, tableName.LocalName)
                : null;
        }

        protected virtual async Task<IRelationalDatabaseTable> LoadTableAsync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            var exists = await TableExistsAsync(tableName.LocalName).ConfigureAwait(false);
            return exists
                ? new SqliteRelationalDatabaseTable(Connection, this, tableName.LocalName)
                : null;
        }

        #endregion Tables

        #region Views

        public bool ViewExists(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            const string sql = "select count(*) from sqlite_master where type = 'view' and lower(name) = lower(@ViewName)";
            return Connection.ExecuteScalar<int>(
                sql,
                new { ViewName = viewName.LocalName }
            ) != 0;
        }

        public async Task<bool> ViewExistsAsync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            const string sql = "select count(*) from sqlite_master where type = 'view' and lower(name) = lower(@ViewName)";
            return await Connection.ExecuteScalarAsync<int>(
                sql,
                new { ViewName = viewName.LocalName }
            ).ConfigureAwait(false) != 0;
        }

        public IRelationalDatabaseView GetView(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            return LoadViewSync(viewName.LocalName);
        }

        public Task<IRelationalDatabaseView> GetViewAsync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            return LoadViewAsync(viewName.LocalName);
        }

        public IEnumerable<IRelationalDatabaseView> Views
        {
            get
            {
                const string sql = "select name from sqlite_master where type = 'view' order by name";
                var viewNames = Connection.Query<string>(sql).Select(name => new LocalIdentifier(name));

                foreach (var viewName in viewNames)
                    yield return LoadViewSync(viewName);
            }
        }

        public async Task<IAsyncEnumerable<IRelationalDatabaseView>> ViewsAsync()
        {
            const string sql = "select name from sqlite_master where type = 'view' order by name";
            var queryResult = await Connection.QueryAsync<string>(sql).ConfigureAwait(false);
            var viewNames = queryResult.Select(name => new LocalIdentifier(name));

            return viewNames
                .Select(LoadViewSync)
                .ToAsyncEnumerable();
        }

        protected virtual IRelationalDatabaseView LoadViewSync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            var exists = ViewExists(viewName.LocalName);
            return exists
                ? new SqliteRelationalDatabaseView(Connection, this, viewName)
                : null;
        }

        protected virtual async Task<IRelationalDatabaseView> LoadViewAsync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            var exists = await ViewExistsAsync(viewName).ConfigureAwait(false);
            return exists
                ? new SqliteRelationalDatabaseView(Connection, this, viewName)
                : null;
        }

        #endregion Views

        #region Sequences

        public bool SequenceExists(Identifier sequenceName) => throw new NotSupportedException(SequencesNotSupported);

        public IDatabaseSequence GetSequence(Identifier sequenceName) => throw new NotSupportedException(SequencesNotSupported);

        public IEnumerable<IDatabaseSequence> Sequences => throw new NotSupportedException(SequencesNotSupported);

        public Task<bool> SequenceExistsAsync(Identifier sequenceName) => throw new NotSupportedException(SequencesNotSupported);

        public Task<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName) => throw new NotSupportedException(SequencesNotSupported);

        public Task<IAsyncEnumerable<IDatabaseSequence>> SequencesAsync() => throw new NotSupportedException(SequencesNotSupported);

        #endregion Sequences

        #region Synonyms

        public bool SynonymExists(Identifier synonymName) => throw new NotSupportedException(SynonymsNotSupported);

        public IDatabaseSynonym GetSynonym(Identifier synonymName) => throw new NotSupportedException(SynonymsNotSupported);

        public IEnumerable<IDatabaseSynonym> Synonyms => throw new NotSupportedException(SynonymsNotSupported);

        public Task<bool> SynonymExistsAsync(Identifier synonymName) => throw new NotSupportedException(SynonymsNotSupported);

        public Task<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName) => throw new NotSupportedException(SynonymsNotSupported);

        public Task<IAsyncEnumerable<IDatabaseSynonym>> SynonymsAsync() => throw new NotSupportedException(SynonymsNotSupported);

        #endregion Synonyms

        protected static IEnumerable<string> BuiltInTables { get; } = new HashSet<string>(new[] { "sqlite_master", "sqlite_sequence" }, StringComparer.OrdinalIgnoreCase);

        private const string SequencesNotSupported = "Sequences are not available in SQLite.";
        private const string SynonymsNotSupported = "Synonyms are not available in SQLite.";
    }
}
