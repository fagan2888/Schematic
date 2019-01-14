﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.SqlServer.Query;

namespace SJP.Schematic.SqlServer
{
    public class SqlServerDatabaseRoutineProvider : IDatabaseRoutineProvider
    {
        public SqlServerDatabaseRoutineProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        }

        protected IDbConnection Connection { get; }

        protected IIdentifierDefaults IdentifierDefaults { get; }

        public async Task<IReadOnlyCollection<IDatabaseRoutine>> GetAllRoutines(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResult = await Connection.QueryAsync<QualifiedName>(RoutinesQuery, cancellationToken).ConfigureAwait(false);
            var routineNames = queryResult
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                .ToList();

            var routines = await routineNames
                .Select(name => LoadRoutine(name, cancellationToken))
                .Somes()
                .ConfigureAwait(false);

            return routines.ToList();
        }

        protected virtual string RoutinesQuery => RoutinesQuerySql;

        private const string RoutinesQuerySql = @"
select schema_name(schema_id) as SchemaName, name as ObjectName
from sys.objects
where type in ('P', 'FN', 'IF', 'TF')
order by schema_name(schema_id), name";

        public OptionAsync<IDatabaseRoutine> GetRoutine(Identifier routineName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            var candidateRoutineName = QualifyRoutineName(routineName);
            return LoadRoutine(candidateRoutineName, cancellationToken);
        }

        protected OptionAsync<Identifier> GetResolvedRoutineName(Identifier routineName, CancellationToken cancellationToken)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            var candidateRoutineName = QualifyRoutineName(routineName);
            var qualifiedRoutineName = Connection.QueryFirstOrNone<QualifiedName>(
                RoutineNameQuery,
                new { SchemaName = candidateRoutineName.Schema, RoutineName = candidateRoutineName.LocalName },
                cancellationToken
            );

            return qualifiedRoutineName.Map(name => Identifier.CreateQualifiedIdentifier(candidateRoutineName.Server, candidateRoutineName.Database, name.SchemaName, name.ObjectName));
        }

        protected virtual string RoutineNameQuery => RoutineNameQuerySql;

        private const string RoutineNameQuerySql = @"
select top 1 schema_name(schema_id) as SchemaName, name as ObjectName
from sys.objects
where schema_id = schema_id(@SchemaName) and name = @RoutineName
    and type in ('P', 'FN', 'IF', 'TF')";

        protected virtual OptionAsync<IDatabaseRoutine> LoadRoutine(Identifier routineName, CancellationToken cancellationToken)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            var candidateRoutineName = QualifyRoutineName(routineName);
            return LoadRoutineAsyncCore(candidateRoutineName, cancellationToken).ToAsync();
        }

        private async Task<Option<IDatabaseRoutine>> LoadRoutineAsyncCore(Identifier routineName, CancellationToken cancellationToken)
        {
            var resolvedRoutineNameOption = GetResolvedRoutineName(routineName, cancellationToken);
            var resolvedRoutineNameOptionIsNone = await resolvedRoutineNameOption.IsNone.ConfigureAwait(false);
            if (resolvedRoutineNameOptionIsNone)
                return Option<IDatabaseRoutine>.None;

            var resolvedRoutineName = await resolvedRoutineNameOption.UnwrapSomeAsync().ConfigureAwait(false);
            var definition = await LoadDefinitionAsync(resolvedRoutineName, cancellationToken).ConfigureAwait(false);

            var routine = new DatabaseRoutine(resolvedRoutineName, definition);
            return Option<IDatabaseRoutine>.Some(routine);
        }

        protected virtual string DefinitionQuery => DefinitionQuerySql;

        private const string DefinitionQuerySql = @"
select m.definition
from sys.sql_modules m
inner join sys.objects o on o.object_id = m.object_id
where schema_name(o.schema_id) = @SchemaName and o.name = @RoutineName";

        protected virtual Task<string> LoadDefinitionAsync(Identifier routineName, CancellationToken cancellationToken)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            return Connection.ExecuteScalarAsync<string>(
                DefinitionQuery,
                new { SchemaName = routineName.Schema, RoutineName = routineName.LocalName },
                cancellationToken
            );
        }

        protected Identifier QualifyRoutineName(Identifier routineName)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            var schema = routineName.Schema ?? IdentifierDefaults.Schema;
            return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, routineName.LocalName);
        }
    }
}