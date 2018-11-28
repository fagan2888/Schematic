﻿using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Oracle.Query;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using LanguageExt;

namespace SJP.Schematic.Oracle
{
    public class OracleRelationalDatabase : RelationalDatabase, IRelationalDatabase
    {
        public OracleRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection, IIdentifierResolutionStrategy identifierResolver)
            : base(dialect, connection)
        {
            IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
            _metadata = new AsyncLazy<DatabaseMetadata>(LoadDatabaseMetadataAsync);
        }

        protected IIdentifierResolutionStrategy IdentifierResolver { get; }

        public string ServerName => Metadata.ServerName;

        public string DatabaseName => Metadata.DatabaseName;

        public string DefaultSchema => Metadata.DefaultSchema;

        public string DatabaseVersion => Metadata.DatabaseVersion;

        protected DatabaseMetadata Metadata => _metadata.Task.GetAwaiter().GetResult();

        protected Option<Identifier> GetResolvedTableName(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return ResolveFirstExistingObjectName(tableName, GetResolvedTableNameStrict);
        }

        protected OptionAsync<Identifier> GetResolvedTableNameAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return ResolveFirstExistingObjectNameAsync(tableName, GetResolvedTableNameStrictAsync, cancellationToken);
        }

        protected Option<Identifier> GetResolvedTableNameStrict(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            var qualifiedTableName = Connection.QueryFirstOrNone<QualifiedName>(
                TableNameQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName }
            );

            return qualifiedTableName.Map(name => Identifier.CreateQualifiedIdentifier(tableName.Server, tableName.Database, name.SchemaName, name.ObjectName));
        }

        protected OptionAsync<Identifier> GetResolvedTableNameStrictAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            var qualifiedTableName = Connection.QueryFirstOrNoneAsync<QualifiedName>(
                TableNameQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName }
            );

            return qualifiedTableName.Map(name => Identifier.CreateQualifiedIdentifier(tableName.Server, tableName.Database, name.SchemaName, name.ObjectName));
        }

        protected virtual string TableNameQuery => TableNameQuerySql;

        private const string TableNameQuerySql = @"
select t.OWNER as SchemaName, t.TABLE_NAME as ObjectName
from ALL_TABLES t
inner join ALL_OBJECTS o on t.OWNER = o.OWNER and t.TABLE_NAME = o.OBJECT_NAME
where t.OWNER = :SchemaName and t.TABLE_NAME = :TableName and o.ORACLE_MAINTAINED <> 'Y'";

        public Option<IRelationalDatabaseTable> GetTable(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            return LoadTableSync(tableName);
        }

        public OptionAsync<IRelationalDatabaseTable> GetTableAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            return LoadTableAsync(tableName, cancellationToken);
        }

        public IReadOnlyCollection<IRelationalDatabaseTable> Tables
        {
            get
            {
                var tableNames = Connection.Query<QualifiedName>(TablesQuery)
                    .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                    .ToList();

                var tables = tableNames
                    .Select(LoadTableSync)
                    .Somes();
                return new ReadOnlyCollectionSlim<IRelationalDatabaseTable>(tableNames.Count, tables);
            }
        }

        public async Task<IReadOnlyCollection<IRelationalDatabaseTable>> TablesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResults = await Connection.QueryAsync<QualifiedName>(TablesQuery).ConfigureAwait(false);
            var tableNames = queryResults
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                .ToList();

            var tables = await tableNames
                .Select(name => LoadTableAsync(name, cancellationToken))
                .Somes()
                .ConfigureAwait(false);

            return tables.ToList();
        }

        protected virtual string TablesQuery => TablesQuerySql;

        private const string TablesQuerySql = @"
select
    t.OWNER as SchemaName,
    t.TABLE_NAME as ObjectName
from ALL_TABLES t
inner join ALL_OBJECTS o on t.OWNER = o.OWNER and t.TABLE_NAME = o.OBJECT_NAME
where o.ORACLE_MAINTAINED <> 'Y'
order by t.OWNER, t.TABLE_NAME";

        protected virtual Option<IRelationalDatabaseTable> LoadTableSync(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var resolvedTableName = ResolveFirstExistingObjectName(tableName, GetResolvedTableName);
            return resolvedTableName
                .Map<IRelationalDatabaseTable>(name => new OracleRelationalDatabaseTable(Connection, this, Dialect.TypeProvider, name, IdentifierResolver));
        }

        protected virtual OptionAsync<IRelationalDatabaseTable> LoadTableAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var resolvedTableName = ResolveFirstExistingObjectNameAsync(tableName, GetResolvedTableNameAsync, cancellationToken);
            return resolvedTableName
                .Map<IRelationalDatabaseTable>(name => new OracleRelationalDatabaseTable(Connection, this, Dialect.TypeProvider, name, IdentifierResolver));
        }

        protected Option<Identifier> GetResolvedViewName(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return ResolveFirstExistingObjectName(viewName, GetResolvedViewNameStrict);
        }

        protected OptionAsync<Identifier> GetResolvedViewNameAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return ResolveFirstExistingObjectNameAsync(viewName, GetResolvedViewNameStrictAsync, cancellationToken);
        }

        protected Option<Identifier> GetResolvedViewNameStrict(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            var qualifiedViewName = Connection.QueryFirstOrNone<QualifiedName>(
                ViewNameQuery,
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName }
            );

            return qualifiedViewName.Map(name => Identifier.CreateQualifiedIdentifier(viewName.Server, viewName.Database, name.SchemaName, name.ObjectName));
        }

        protected OptionAsync<Identifier> GetResolvedViewNameStrictAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            var qualifiedViewName = Connection.QueryFirstOrNoneAsync<QualifiedName>(
                ViewNameQuery,
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName }
            );

            return qualifiedViewName.Map(name => Identifier.CreateQualifiedIdentifier(viewName.Server, viewName.Database, name.SchemaName, name.ObjectName));
        }

        protected virtual string ViewNameQuery => ViewNameQuerySql;

        private const string ViewNameQuerySql = @"
select v.OWNER as SchemaName, v.VIEW_NAME as ObjectName
from ALL_VIEWS v
inner join ALL_OBJECTS o on v.OWNER = o.OWNER and v.VIEW_NAME = o.OBJECT_NAME
where v.OWNER = :SchemaName and v.VIEW_NAME = :ViewName and o.ORACLE_MAINTAINED <> 'Y'";

        public Option<IRelationalDatabaseView> GetView(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            return LoadViewSync(viewName);
        }

        public OptionAsync<IRelationalDatabaseView> GetViewAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            return LoadViewAsync(viewName, cancellationToken);
        }

        public IReadOnlyCollection<IRelationalDatabaseView> Views
        {
            get
            {
                var viewNames = Connection.Query<QualifiedName>(ViewsQuery)
                    .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                    .ToList();

                var views = viewNames
                    .Select(LoadViewSync)
                    .Somes();
                return new ReadOnlyCollectionSlim<IRelationalDatabaseView>(viewNames.Count, views);
            }
        }

        public async Task<IReadOnlyCollection<IRelationalDatabaseView>> ViewsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResult = await Connection.QueryAsync<QualifiedName>(ViewsQuery).ConfigureAwait(false);
            var viewNames = queryResult
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                .ToList();

            var views = await viewNames
                .Select(name => LoadViewAsync(name, cancellationToken))
                .Somes()
                .ConfigureAwait(false);

            return views.ToList();
        }

        protected virtual string ViewsQuery => ViewsQuerySql;

        private const string ViewsQuerySql = @"
select
    v.OWNER as SchemaName,
    v.VIEW_NAME as ObjectName
from ALL_VIEWS v
inner join ALL_OBJECTS o on v.OWNER = o.OWNER and v.VIEW_NAME = o.OBJECT_NAME
where o.ORACLE_MAINTAINED <> 'Y'
order by v.OWNER, v.VIEW_NAME";

        protected virtual Option<IRelationalDatabaseView> LoadViewSync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var resolvedViewName = ResolveFirstExistingObjectName(viewName, GetResolvedViewName);
            return resolvedViewName
                .Map<IRelationalDatabaseView>(name => new OracleRelationalDatabaseView(Connection, Dialect.TypeProvider, name, IdentifierResolver));
        }

        protected virtual OptionAsync<IRelationalDatabaseView> LoadViewAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var resolvedViewName = ResolveFirstExistingObjectNameAsync(viewName, GetResolvedViewNameAsync, cancellationToken);
            return resolvedViewName
                .Map<IRelationalDatabaseView>(name => new OracleRelationalDatabaseView(Connection, Dialect.TypeProvider, name, IdentifierResolver));
        }

        public Option<Identifier> GetResolvedSequenceName(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return ResolveFirstExistingObjectName(sequenceName, GetResolvedSequenceNameStrict);
        }

        public OptionAsync<Identifier> GetResolvedSequenceNameAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return ResolveFirstExistingObjectNameAsync(sequenceName, GetResolvedSequenceNameStrictAsync, cancellationToken);
        }

        protected Option<Identifier> GetResolvedSequenceNameStrict(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            var qualifiedSequenceName = Connection.QueryFirstOrNone<QualifiedName>(
                SequenceNameQuery,
                new { SchemaName = sequenceName.Schema, SequenceName = sequenceName.LocalName }
            );

            return qualifiedSequenceName.Map(name => Identifier.CreateQualifiedIdentifier(sequenceName.Server, sequenceName.Database, name.SchemaName, name.ObjectName));
        }

        protected OptionAsync<Identifier> GetResolvedSequenceNameStrictAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            var qualifiedSequenceName = Connection.QueryFirstOrNoneAsync<QualifiedName>(
                SequenceNameQuery,
                new { SchemaName = sequenceName.Schema, SequenceName = sequenceName.LocalName }
            );

            return qualifiedSequenceName.Map(name => Identifier.CreateQualifiedIdentifier(sequenceName.Server, sequenceName.Database, name.SchemaName, name.ObjectName));
        }

        protected virtual string SequenceNameQuery => SequenceNameQuerySql;

        private const string SequenceNameQuerySql = @"
select s.SEQUENCE_OWNER as SchemaName, s.SEQUENCE_NAME as ObjectName
from ALL_SEQUENCES s
inner join ALL_OBJECTS o on s.SEQUENCE_OWNER = o.OWNER and s.SEQUENCE_NAME = o.OBJECT_NAME
where s.SEQUENCE_OWNER = :SchemaName and s.SEQUENCE_NAME = :SequenceName and o.ORACLE_MAINTAINED <> 'Y'";

        public Option<IDatabaseSequence> GetSequence(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            return LoadSequenceSync(sequenceName);
        }

        public OptionAsync<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            return LoadSequenceAsync(sequenceName);
        }

        public IReadOnlyCollection<IDatabaseSequence> Sequences
        {
            get
            {
                var sequenceNames = Connection.Query<QualifiedName>(SequencesQuery)
                    .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                    .ToList();

                var sequences = sequenceNames
                    .Select(LoadSequenceSync)
                    .Somes();
                return new ReadOnlyCollectionSlim<IDatabaseSequence>(sequenceNames.Count, sequences);
            }
        }

        public async Task<IReadOnlyCollection<IDatabaseSequence>> SequencesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResult = await Connection.QueryAsync<QualifiedName>(SequencesQuery).ConfigureAwait(false);
            var sequenceNames = queryResult
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                .ToList();

            var sequences = await sequenceNames
                .Select(name => LoadSequenceAsync(name, cancellationToken))
                .Somes()
                .ConfigureAwait(false);

            return sequences.ToList();
        }

        protected virtual string SequencesQuery => SequencesQuerySql;

        private const string SequencesQuerySql = @"
select
    s.SEQUENCE_OWNER as SchemaName,
    s.SEQUENCE_NAME as ObjectName
from ALL_SEQUENCES s
inner join ALL_OBJECTS o on s.SEQUENCE_OWNER = o.OWNER and s.SEQUENCE_NAME = o.OBJECT_NAME
where o.ORACLE_MAINTAINED <> 'Y'
order by s.SEQUENCE_OWNER, s.SEQUENCE_NAME";

        protected virtual Option<IDatabaseSequence> LoadSequenceSync(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return ResolveFirstExistingObjectName(sequenceName, GetResolvedSequenceName)
                .Map<IDatabaseSequence>(name => new OracleDatabaseSequence(Connection, name));
        }

        protected virtual OptionAsync<IDatabaseSequence> LoadSequenceAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var resolvedSequenceName = ResolveFirstExistingObjectNameAsync(sequenceName, GetResolvedSequenceNameAsync, cancellationToken);
            return resolvedSequenceName
                .Map<IDatabaseSequence>(name => new OracleDatabaseSequence(Connection, name));
        }

        public Option<Identifier> GetResolvedSynonymName(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return ResolveFirstExistingObjectName(synonymName, GetResolvedSynonymNameStrict);
        }

        public OptionAsync<Identifier> GetResolvedSynonymNameAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return ResolveFirstExistingObjectNameAsync(synonymName, GetResolvedSynonymNameStrictAsync, cancellationToken);
        }

        protected Option<Identifier> GetResolvedSynonymNameStrict(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);

            if (synonymName.Database == DatabaseName && synonymName.Schema == DefaultSchema)
                return GetResolvedUserSynonymNameStrict(synonymName.LocalName);

            var qualifiedSynonymName = Connection.QueryFirstOrNone<QualifiedName>(
                SynonymNameQuery,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName }
            );

            return qualifiedSynonymName.Map(name => Identifier.CreateQualifiedIdentifier(synonymName.Server, synonymName.Database, name.SchemaName, name.ObjectName));
        }

        protected Option<Identifier> GetResolvedUserSynonymNameStrict(string synonymName)
        {
            if (synonymName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(synonymName));

            var name = Connection.QueryFirstOrNone<string>(UserSynonymNameQuery, new { SynonymName = synonymName });
            return name.Map(n => Identifier.CreateQualifiedIdentifier(ServerName, DatabaseName, DefaultSchema, n));
        }

        protected OptionAsync<Identifier> GetResolvedSynonymNameStrictAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);

            // fast path, ALL_SYNONYMS is much slower than USER_SYNONYMS so prefer the latter where possible
            if (synonymName.Database == DatabaseName && synonymName.Schema == DefaultSchema)
            {
                var name = Connection.QueryFirstOrNoneAsync<string>(
                    UserSynonymNameQuery,
                    new { SynonymName = synonymName.LocalName }
                );

                return name.Map(n => Identifier.CreateQualifiedIdentifier(ServerName, DatabaseName, DefaultSchema, n));
            }

            var qualifiedSynonymName = Connection.QueryFirstOrNoneAsync<QualifiedName>(
                SynonymNameQuery,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName }
            );

            return qualifiedSynonymName.Map(name => Identifier.CreateQualifiedIdentifier(synonymName.Server, synonymName.Database, name.SchemaName, name.ObjectName));
        }

        protected virtual string SynonymNameQuery => SynonymNameQuerySql;

        private const string SynonymNameQuerySql = @"
select s.OWNER as SchemaName, s.SYNONYM_NAME as ObjectName
from ALL_SYNONYMS s
inner join ALL_OBJECTS o on s.OWNER = o.OWNER and s.SYNONYM_NAME = o.OBJECT_NAME
where s.OWNER = :SchemaName and s.SYNONYM_NAME = :SynonymName and o.ORACLE_MAINTAINED <> 'Y'";

        protected virtual string UserSynonymNameQuery => UserSynonymNameQuerySql;

        private const string UserSynonymNameQuerySql = @"
select s.SYNONYM_NAME
from USER_SYNONYMS s
inner join ALL_OBJECTS o on s.SYNONYM_NAME = o.OBJECT_NAME
where o.OWNER = SYS_CONTEXT('USERENV', 'CURRENT_USER') and s.SYNONYM_NAME = :SynonymName and o.ORACLE_MAINTAINED <> 'Y'";

        public Option<IDatabaseSynonym> GetSynonym(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            return LoadSynonymSync(synonymName);
        }

        public OptionAsync<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            return LoadSynonymAsync(synonymName, cancellationToken);
        }

        public IReadOnlyCollection<IDatabaseSynonym> Synonyms
        {
            get
            {
                var synonymNames = Connection.Query<QualifiedName>(SynonymsQuery)
                    .Select(dto => Identifier.CreateQualifiedIdentifier(dto.DatabaseName, dto.SchemaName, dto.ObjectName))
                    .ToList();

                var synonyms = synonymNames
                    .Select(LoadSynonymSync)
                    .Somes();
                return new ReadOnlyCollectionSlim<IDatabaseSynonym>(synonymNames.Count, synonyms);
            }
        }

        public async Task<IReadOnlyCollection<IDatabaseSynonym>> SynonymsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResult = await Connection.QueryAsync<QualifiedName>(SynonymsQuery).ConfigureAwait(false);
            var synonymNames = queryResult
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.DatabaseName, dto.SchemaName, dto.ObjectName))
                .ToList();

            var synonyms = await synonymNames
                .Select(name => LoadSynonymAsync(name, cancellationToken))
                .Somes()
                .ConfigureAwait(false);

            return synonyms.ToList();
        }

        protected virtual string SynonymsQuery => SynonymsQuerySql;

        private const string SynonymsQuerySql = @"
select distinct
    s.DB_LINK as DatabaseName,
    s.OWNER as SchemaName,
    s.SYNONYM_NAME as ObjectName
from ALL_SYNONYMS s
inner join ALL_OBJECTS o on s.OWNER = o.OWNER and s.SYNONYM_NAME = o.OBJECT_NAME
where o.ORACLE_MAINTAINED <> 'Y'
order by s.DB_LINK, s.OWNER, s.SYNONYM_NAME";

        protected virtual Option<IDatabaseSynonym> LoadSynonymSync(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var resolvedSynonym = ResolveFirstExistingObjectName(synonymName, GetResolvedSynonymNameStrict);
            if (resolvedSynonym.IsNone)
                return Option<IDatabaseSynonym>.None;

            var resolvedSynonymName = resolvedSynonym.UnwrapSome();
            if (resolvedSynonymName.Database == DatabaseName && resolvedSynonymName.Schema == DefaultSchema)
                return LoadUserSynonymSync(resolvedSynonymName.LocalName);

            var queryResult = Connection.QueryFirstOrNone<SynonymData>(
                LoadSynonymQuery,
                new { SchemaName = resolvedSynonymName.Schema, SynonymName = resolvedSynonymName.LocalName }
            );

            return queryResult.Map<IDatabaseSynonym>(synonymData =>
            {
                var databaseName = !synonymData.TargetDatabaseName.IsNullOrWhiteSpace() ? synonymData.TargetDatabaseName : null;
                var schemaName = !synonymData.TargetSchemaName.IsNullOrWhiteSpace() ? synonymData.TargetSchemaName : null;
                var localName = !synonymData.TargetObjectName.IsNullOrWhiteSpace() ? synonymData.TargetObjectName : null;

                var qualifiedSynonymName = CreateQualifiedIdentifier(synonymName);
                var targetName = Identifier.CreateQualifiedIdentifier(databaseName, schemaName, localName);
                var qualifiedTargetName = CreateQualifiedIdentifier(targetName);

                return new DatabaseSynonym(qualifiedSynonymName, qualifiedTargetName);
            });
        }

        private Option<IDatabaseSynonym> LoadUserSynonymSync(string synonymName)
        {
            var queryResult = Connection.QueryFirstOrNone<SynonymData>(
                LoadUserSynonymQuery,
                new { SynonymName = synonymName }
            );

            return queryResult.Map<IDatabaseSynonym>(synonymData =>
            {
                var databaseName = !synonymData.TargetDatabaseName.IsNullOrWhiteSpace() ? synonymData.TargetDatabaseName : null;
                var schemaName = !synonymData.TargetSchemaName.IsNullOrWhiteSpace() ? synonymData.TargetSchemaName : null;
                var localName = !synonymData.TargetObjectName.IsNullOrWhiteSpace() ? synonymData.TargetObjectName : null;

                var qualifiedSynonymName = CreateQualifiedIdentifier(synonymName);
                var targetName = Identifier.CreateQualifiedIdentifier(ServerName, databaseName ?? DatabaseName, schemaName ?? DefaultSchema, localName);

                return new DatabaseSynonym(qualifiedSynonymName, targetName);
            });
        }

        protected virtual OptionAsync<IDatabaseSynonym> LoadSynonymAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return LoadSynonymAsyncCore(synonymName, cancellationToken).ToAsync();
        }

        private async Task<Option<IDatabaseSynonym>> LoadSynonymAsyncCore(Identifier synonymName, CancellationToken cancellationToken)
        {
            var resolvedSynonymNameOption = ResolveFirstExistingObjectNameAsync(synonymName, GetResolvedSynonymNameStrictAsync, cancellationToken);
            var resolvedNameIsNone = await resolvedSynonymNameOption.IsNone.ConfigureAwait(false);
            if (resolvedNameIsNone)
                return Option<IDatabaseSynonym>.None;

            var resolvedSynonymName = await resolvedSynonymNameOption.UnwrapSomeAsync().ConfigureAwait(false);
            if (resolvedSynonymName.Database == DatabaseName && resolvedSynonymName.Schema == DefaultSchema)
                return await LoadUserSynonymAsyncCore(resolvedSynonymName.LocalName, cancellationToken).ToOption().ConfigureAwait(false);

            var queryResult = Connection.QueryFirstOrNoneAsync<SynonymData>(
                LoadSynonymQuery,
                new { SchemaName = resolvedSynonymName.Schema, SynonymName = resolvedSynonymName.LocalName }
            );

            var synonym = queryResult.Map<IDatabaseSynonym>(synonymData =>
            {
                var databaseName = !synonymData.TargetDatabaseName.IsNullOrWhiteSpace() ? synonymData.TargetDatabaseName : null;
                var schemaName = !synonymData.TargetSchemaName.IsNullOrWhiteSpace() ? synonymData.TargetSchemaName : null;
                var localName = !synonymData.TargetObjectName.IsNullOrWhiteSpace() ? synonymData.TargetObjectName : null;

                var qualifiedSynonymName = CreateQualifiedIdentifier(synonymName);
                var targetName = Identifier.CreateQualifiedIdentifier(ServerName, databaseName ?? DatabaseName, schemaName ?? DefaultSchema, localName);

                return new DatabaseSynonym(qualifiedSynonymName, targetName);
            });

            return await synonym.ToOption().ConfigureAwait(false);
        }

        private OptionAsync<IDatabaseSynonym> LoadUserSynonymAsyncCore(string synonymName, CancellationToken cancellationToken)
        {
            var queryResult = Connection.QueryFirstOrNoneAsync<SynonymData>(
                LoadUserSynonymQuery,
                new { SynonymName = synonymName }
            );

            return queryResult.Map<IDatabaseSynonym>(synonymData =>
            {
                var databaseName = !synonymData.TargetDatabaseName.IsNullOrWhiteSpace() ? synonymData.TargetDatabaseName : null;
                var schemaName = !synonymData.TargetSchemaName.IsNullOrWhiteSpace() ? synonymData.TargetSchemaName : null;
                var localName = !synonymData.TargetObjectName.IsNullOrWhiteSpace() ? synonymData.TargetObjectName : null;

                var qualifiedSynonymName = CreateQualifiedIdentifier(synonymName);
                var targetName = Identifier.CreateQualifiedIdentifier(ServerName, databaseName ?? DatabaseName, schemaName ?? DefaultSchema, localName);

                return new DatabaseSynonym(qualifiedSynonymName, targetName);
            });
        }

        protected virtual string LoadSynonymQuery => LoadSynonymQuerySql;

        private const string LoadSynonymQuerySql = @"
select distinct
    s.DB_LINK as TargetDatabaseName,
    s.TABLE_OWNER as TargetSchemaName,
    s.TABLE_NAME as TargetObjectName
from ALL_SYNONYMS s
inner join ALL_OBJECTS o on s.OWNER = o.OWNER and s.SYNONYM_NAME = o.OBJECT_NAME
where s.OWNER = :SchemaName and s.SYNONYM_NAME = :SynonymName and o.ORACLE_MAINTAINED <> 'Y'";

        protected virtual string LoadUserSynonymQuery => LoadUserSynonymQuerySql;

        private const string LoadUserSynonymQuerySql = @"
select distinct
    s.DB_LINK as TargetDatabaseName,
    s.TABLE_OWNER as TargetSchemaName,
    s.TABLE_NAME as TargetObjectName
from USER_SYNONYMS s
inner join ALL_OBJECTS o on s.SYNONYM_NAME = o.OBJECT_NAME
where s.SYNONYM_NAME = :SynonymName and o.OWNER = SYS_CONTEXT('USERENV', 'CURRENT_USER') and o.ORACLE_MAINTAINED <> 'Y'";

        protected Option<Identifier> ResolveFirstExistingObjectName(Identifier objectName, Func<Identifier, Option<Identifier>> objectExistsFunc)
        {
            if (objectName == null)
                throw new ArgumentNullException(nameof(objectName));
            if (objectExistsFunc == null)
                throw new ArgumentNullException(nameof(objectExistsFunc));

            var resolvedNames = IdentifierResolver
                .GetResolutionOrder(objectName)
                .Select(CreateQualifiedIdentifier);

            return resolvedNames
                .Select(objectExistsFunc)
                .FirstOrDefault(name => name.IsSome);
        }

        protected OptionAsync<Identifier> ResolveFirstExistingObjectNameAsync(Identifier objectName, Func<Identifier, CancellationToken, OptionAsync<Identifier>> objectExistsFunc, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (objectName == null)
                throw new ArgumentNullException(nameof(objectName));
            if (objectExistsFunc == null)
                throw new ArgumentNullException(nameof(objectExistsFunc));

            return ResolveFirstExistingObjectNameAsyncCore(objectName, objectExistsFunc, cancellationToken).ToAsync();
        }

        private async Task<Option<Identifier>> ResolveFirstExistingObjectNameAsyncCore(Identifier objectName, Func<Identifier, CancellationToken, OptionAsync<Identifier>> objectExistsFunc, CancellationToken cancellationToken)
        {
            var resolvedNames = IdentifierResolver
                .GetResolutionOrder(objectName)
                .Select(CreateQualifiedIdentifier);

            foreach (var resolvedName in resolvedNames)
            {
                var existingName = objectExistsFunc(resolvedName, cancellationToken);

                var existingNameIsSome = await existingName.IsSome.ConfigureAwait(false);
                if (existingNameIsSome)
                    return await existingName.UnwrapSomeAsync().ConfigureAwait(false);
            }

            return Option<Identifier>.None;
        }

        /// <summary>
        /// Qualifies an identifier with information from the database. For example, sets the schema if it is missing.
        /// </summary>
        /// <param name="identifier">An identifier which may or may not be fully qualified.</param>
        /// <returns>A new identifier, which will have components present where they were previously missing.</returns>
        /// <remarks>No components of an identifier when present will be modified. For example, when given a fully qualified identifier, a new identifier will be returned that is equal in value to the argument.</remarks>
        protected Identifier CreateQualifiedIdentifier(Identifier identifier)
        {
            if (identifier == null)
                throw new ArgumentNullException(nameof(identifier));

            var schema = identifier.Schema ?? DefaultSchema;
            return Identifier.CreateQualifiedIdentifier(ServerName, DatabaseName, schema, identifier.LocalName);
        }

        private async Task<DatabaseMetadata> LoadDatabaseMetadataAsync()
        {
            const string hostSql = @"
select
    SYS_CONTEXT('USERENV', 'SERVER_HOST') as ServerHost,
    SYS_CONTEXT('USERENV', 'INSTANCE_NAME') as ServerSid,
    SYS_CONTEXT('USERENV', 'DB_NAME') as DatabaseName,
    SYS_CONTEXT('USERENV', 'CURRENT_USER') as DefaultSchema
from DUAL";
            var hostInfoOption = Connection.QueryFirstOrNoneAsync<DatabaseHost>(hostSql);

            const string versionSql = @"
select
    PRODUCT as ProductName,
    VERSION as VersionNumber
from PRODUCT_COMPONENT_VERSION
where PRODUCT like 'Oracle Database%'";
            var versionInfoOption = Connection.QueryFirstOrNoneAsync<DatabaseVersion>(versionSql);

            var qualifiedServerName = await hostInfoOption.MatchUnsafe(
                dbHost => dbHost.ServerHost + "/" + dbHost.ServerSid,
                () => null
            ).ConfigureAwait(false);
            var dbName = await hostInfoOption.MatchUnsafe(h => h.DatabaseName, () => null).ConfigureAwait(false);
            var defaultSchema = await hostInfoOption.MatchUnsafe(h => h.DefaultSchema, () => null).ConfigureAwait(false);
            var versionName = await versionInfoOption.MatchUnsafe(
                vInfo => vInfo.ProductName + vInfo.VersionNumber,
                () => null
            ).ConfigureAwait(false);

            return new DatabaseMetadata
            {
                 ServerName = qualifiedServerName,
                 DatabaseName = dbName,
                 DefaultSchema = defaultSchema,
                 DatabaseVersion = versionName
            };
        }

        private readonly AsyncLazy<DatabaseMetadata> _metadata;
    }
}
