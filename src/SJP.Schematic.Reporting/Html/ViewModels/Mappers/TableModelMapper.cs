﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Dot;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers
{
    internal sealed class TableModelMapper
    {
        public TableModelMapper(IDbConnection connection, IRelationalDatabase database, IDatabaseDialect dialect)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
        }

        private IDbConnection Connection { get; }

        private IRelationalDatabase Database { get; }

        private IDatabaseDialect Dialect { get; }

        public Task<Table> MapAsync(IRelationalDatabaseTable table, CancellationToken cancellationToken)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            return MapAsyncCore(table, cancellationToken);
        }

        private async Task<Table> MapAsyncCore(IRelationalDatabaseTable table, CancellationToken cancellationToken)
        {
            var rowCount = await Connection.GetRowCountAsync(Dialect, table.Name, cancellationToken).ConfigureAwait(false);
            var tableColumns = table.Columns.Select((c, i) => new { Column = c, Ordinal = i + 1 }).ToList();
            var primaryKey = table.PrimaryKey;
            var uniqueKeys = table.UniqueKeys.ToList();
            var parentKeys = table.ParentKeys.ToList();
            var childKeys = table.ChildKeys.ToList();
            var checks = table.Checks.ToList();

            var columns = new List<Table.Column>();
            foreach (var tableColumn in tableColumns)
            {
                var col = tableColumn.Column;
                var columnName = col.Name.LocalName;
                var qualifiedColumnName = table.Name.ToVisibleName() + "." + columnName;

                var isPrimaryKey = primaryKey.Match(pk => pk.Columns.Any(c => c.Name.LocalName == columnName), () => false);
                var isUniqueKey = uniqueKeys.Any(uk => uk.Columns.Any(ukc => ukc.Name.LocalName == columnName));
                var isParentKey = parentKeys.Any(fk => fk.ChildKey.Columns.Any(fkc => fkc.Name.LocalName == columnName));

                var matchingParentKeys = parentKeys.Where(fk => fk.ChildKey.Columns.Any(fkc => fkc.Name.LocalName == columnName)).ToList();
                var columnParentKeys = new List<Table.ParentKey>();
                foreach (var parentKey in matchingParentKeys)
                {
                    var columnIndexes = parentKey.ChildKey.Columns
                        .Select((c, i) => c.Name.LocalName == columnName ? i : -1)
                        .Where(i => i >= 0)
                        .ToList();

                    var parentColumnNames = parentKey.ParentKey.Columns
                        .Where((_, i) => columnIndexes.Contains(i))
                        .Select(c => c.Name.LocalName)
                        .ToList();

                    var childKeyName = parentKey.ChildKey.Name.Match(name => name.LocalName, () => string.Empty);
                    var columnFks = parentColumnNames.Select(colName =>
                        new Table.ParentKey(
                            childKeyName,
                            parentKey.ParentTable,
                            colName,
                            qualifiedColumnName
                        )).ToList();

                    columnParentKeys.AddRange(columnFks);
                }

                var matchingChildKeys = childKeys.Where(ck => ck.ParentKey.Columns.Any(ckc => ckc.Name.LocalName == columnName)).ToList();
                var columnChildKeys = new List<Table.ChildKey>();
                foreach (var childKey in matchingChildKeys)
                {
                    var columnIndexes = childKey.ParentKey.Columns
                        .Select((c, i) => c.Name.LocalName == columnName ? i : -1)
                        .Where(i => i >= 0)
                        .ToList();

                    var childColumnNames = childKey.ChildKey.Columns
                        .Where((_, i) => columnIndexes.Contains(i))
                        .Select(c => c.Name.LocalName)
                        .ToList();

                    var childKeyName = childKey.ChildKey.Name.Match(name => name.LocalName, () => string.Empty);
                    var columnFks = childColumnNames.Select(colName =>
                        new Table.ChildKey(
                            childKeyName,
                            childKey.ChildTable,
                            colName,
                            qualifiedColumnName
                        )).ToList();

                    columnChildKeys.AddRange(columnFks);
                }

                var column = new Table.Column(
                    columnName,
                    tableColumn.Ordinal,
                    tableColumn.Column.IsNullable,
                    tableColumn.Column.Type.Definition,
                    tableColumn.Column.DefaultValue,
                    isPrimaryKey,
                    isUniqueKey,
                    isParentKey,
                    columnChildKeys,
                    columnParentKeys
                );
                columns.Add(column);
            }

            var tableIndexes = table.Indexes.ToList();
            var mappedIndexes = tableIndexes.Select(index =>
                new Table.Index(
                    index.Name?.LocalName,
                    index.IsUnique,
                    index.Columns.Select(c => c.Expression).ToList(),
                    index.Columns.Select(c => c.Order).ToList(),
                    index.IncludedColumns.Select(c => c.Name.LocalName).ToList()
                )).ToList();

            var renderPrimaryKey = primaryKey
                .Map(pk => new Table.PrimaryKeyConstraint(
                    pk.Name.Match(name => name.LocalName, () => string.Empty),
                    pk.Columns.Select(c => c.Name.LocalName).ToList()
                ));

            var renderUniqueKeys = uniqueKeys
                .Select(uk => new Table.UniqueKey(
                    uk.Name.Match(name => name.LocalName, () => string.Empty),
                    uk.Columns.Select(c => c.Name.LocalName).ToList()
                )).ToList();

            var renderParentKeys = parentKeys.Select(pk =>
                new Table.ForeignKey(
                    pk.ChildKey.Name.Match(name => name.LocalName, () => string.Empty),
                    pk.ChildKey.Columns.Select(c => c.Name.LocalName).ToList(),
                    pk.ParentTable,
                    pk.ParentKey.Name.Match(name => name.LocalName, () => string.Empty),
                    pk.ParentKey.Columns.Select(c => c.Name.LocalName).ToList(),
                    pk.DeleteRule,
                    pk.UpdateRule
                )).ToList();

            var renderChecks = checks.Select(c =>
                new Table.CheckConstraint(
                    c.Name.Match(name => name.LocalName, () => string.Empty),
                    c.Definition
                )).ToList();

            var relationshipBuilder = new RelationshipFinder(Database);
            var oneDegreeTables = await relationshipBuilder.GetTablesByDegreesAsync(table, 1, cancellationToken).ConfigureAwait(false);
            var twoDegreeTables = await relationshipBuilder.GetTablesByDegreesAsync(table, 2, cancellationToken).ConfigureAwait(false);

            var dotFormatter = new DatabaseDotFormatter(Connection, Database);
            var renderOptions = new DotRenderOptions { HighlightedTable = table.Name };
            var oneDegreeDot = await dotFormatter.RenderTablesAsync(oneDegreeTables, renderOptions, cancellationToken).ConfigureAwait(false);
            var twoDegreeDot = await dotFormatter.RenderTablesAsync(twoDegreeTables, renderOptions, cancellationToken).ConfigureAwait(false);

            var diagrams = new[]
            {
                new Table.Diagram(table.Name, "One", oneDegreeDot, true),
                new Table.Diagram(table.Name, "Two", twoDegreeDot, false)
            };

            return new Table(
                table.Name,
                columns,
                renderPrimaryKey,
                renderUniqueKeys,
                renderParentKeys,
                renderChecks,
                mappedIndexes,
                diagrams,
                "../",
                rowCount
            );
        }
    }
}
