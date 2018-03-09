﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;
using SJP.Schematic.Lint.Tests.Fakes;

namespace SJP.Schematic.Lint.Tests.Rules
{
    [TestFixture]
    internal class UniqueIndexWithNullableColumnsRuleTests
    {
        [Test]
        public void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new UniqueIndexWithNullableColumnsRule(level));
        }

        [Test]
        public void AnalyseDatabase_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var rule = new UniqueIndexWithNullableColumnsRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseDatabase(null));
        }

        [Test]
        public void AnalyseDatabase_GivenTableWithNoIndexes_ProducesNoMessages()
        {
            var rule = new UniqueIndexWithNullableColumnsRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var table = new RelationalDatabaseTable(
                database,
                "test",
                new List<IDatabaseTableColumn>(),
                null,
                Enumerable.Empty<IDatabaseKey>(),
                Enumerable.Empty<IDatabaseRelationalKey>(),
                Enumerable.Empty<IDatabaseRelationalKey>(),
                Enumerable.Empty<IDatabaseTableIndex>(),
                Enumerable.Empty<IDatabaseCheckConstraint>(),
                Enumerable.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = rule.AnalyseDatabase(database);

            Assert.Zero(messages.Count());
        }

        [Test]
        public void AnalyseDatabase_GivenTableWithNoUniqueIndexes_ProducesNoMessages()
        {
            var rule = new UniqueIndexWithNullableColumnsRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testColumn = new DatabaseTableColumn(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_column_1",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );

            var index = new DatabaseTableIndex(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_index_name",
                false,
                new[] { new DatabaseIndexColumn(testColumn, IndexColumnOrder.Ascending) },
                Enumerable.Empty<IDatabaseTableColumn>(),
                true
            );

            var table = new RelationalDatabaseTable(
                database,
                "test",
                new List<IDatabaseTableColumn>(),
                null,
                Enumerable.Empty<IDatabaseKey>(),
                Enumerable.Empty<IDatabaseRelationalKey>(),
                Enumerable.Empty<IDatabaseRelationalKey>(),
                new[] { index },
                Enumerable.Empty<IDatabaseCheckConstraint>(),
                Enumerable.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = rule.AnalyseDatabase(database);

            Assert.Zero(messages.Count());
        }

        [Test]
        public void AnalyseDatabase_GivenTableWithNoNullableColumnsInUniqueIndex_ProducesNoMessages()
        {
            var rule = new UniqueIndexWithNullableColumnsRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testColumn = new DatabaseTableColumn(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_column_1",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );

            var uniqueIndex = new DatabaseTableIndex(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_index_name",
                true,
                new[] { new DatabaseIndexColumn(testColumn, IndexColumnOrder.Ascending) },
                Enumerable.Empty<IDatabaseTableColumn>(),
                true
            );

            var table = new RelationalDatabaseTable(
                database,
                "test",
                new List<IDatabaseTableColumn>(),
                null,
                Enumerable.Empty<IDatabaseKey>(),
                Enumerable.Empty<IDatabaseRelationalKey>(),
                Enumerable.Empty<IDatabaseRelationalKey>(),
                new[] { uniqueIndex },
                Enumerable.Empty<IDatabaseCheckConstraint>(),
                Enumerable.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = rule.AnalyseDatabase(database);

            Assert.Zero(messages.Count());
        }

        [Test]
        public void AnalyseDatabase_GivenTableWithNullableColumnsInUniqueIndex_ProducesMessages()
        {
            var rule = new UniqueIndexWithNullableColumnsRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testColumn = new DatabaseTableColumn(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_column_1",
                Mock.Of<IDbType>(),
                true,
                null,
                null
            );

            var uniqueIndex = new DatabaseTableIndex(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_index_name",
                true,
                new[] { new DatabaseIndexColumn(testColumn, IndexColumnOrder.Ascending) },
                Enumerable.Empty<IDatabaseTableColumn>(),
                true
            );

            var table = new RelationalDatabaseTable(
                database,
                "test",
                new List<IDatabaseTableColumn>(),
                null,
                Enumerable.Empty<IDatabaseKey>(),
                Enumerable.Empty<IDatabaseRelationalKey>(),
                Enumerable.Empty<IDatabaseRelationalKey>(),
                new[] { uniqueIndex },
                Enumerable.Empty<IDatabaseCheckConstraint>(),
                Enumerable.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = rule.AnalyseDatabase(database);

            Assert.NotZero(messages.Count());
        }

        private static FakeRelationalDatabase CreateFakeDatabase()
        {
            var dialect = new FakeDatabaseDialect();
            var connection = Mock.Of<IDbConnection>();

            return new FakeRelationalDatabase(dialect, connection);
        }
    }
}