﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Integration
{
    internal sealed class ForeignKeyMissingRuleTests : SqliteTest
    {
        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create table no_foreign_key_parent_1 ( column_1 integer not null primary key autoincrement )").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table NoForeignKeyParent1 ( Column1 integer not null primary key autoincrement )").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table no_foreign_key_child_with_key (
    column_1 integer,
    no_foreign_key_parent_1_id integer,
    constraint no_foreign_key_child_with_key_fk1 foreign key (no_foreign_key_parent_1_id) references no_foreign_key_parent_1 (column_1)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table no_foreign_key_child_without_key (
    column_1 integer,
    no_foreign_key_parent_1_id integer
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table NoForeignKeyChildWithKey (
    Column1 integer,
    NoForeignKeyParent1Id integer,
    constraint NoForeignKeyChildWithKeyFk1 foreign key (NoForeignKeyParent1Id) references NoForeignKeyParent1 (Column1)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table NoForeignKeyChildWithoutKey (
    Column1 integer,
    NoForeignKeyParent1Id integer
)").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop table no_foreign_key_child_with_key").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table no_foreign_key_child_without_key").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table NoForeignKeyChildWithKey").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table NoForeignKeyChildWithoutKey").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table no_foreign_key_parent_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table NoForeignKeyParent1").ConfigureAwait(false);
        }

        [Test]
        public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new ForeignKeyMissingRule(level));
        }

        [Test]
        public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
        {
            var rule = new ForeignKeyMissingRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseTables(null));
        }

        [Test]
        public static void AnalyseTablesAsync_GivenNullTables_ThrowsArgumentNullException()
        {
            var rule = new ForeignKeyMissingRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseTablesAsync(null));
        }

        [Test]
        public void AnalyseTables_GivenSnakeCaseTablesContainingTableWithValidForeignKey_ProducesNoMessages()
        {
            var rule = new ForeignKeyMissingRule(RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                database.GetTable("no_foreign_key_parent_1").UnwrapSomeAsync().GetAwaiter().GetResult(),
                database.GetTable("no_foreign_key_child_with_key").UnwrapSomeAsync().GetAwaiter().GetResult()
            };

            var messages = rule.AnalyseTables(tables);

            Assert.Zero(messages.Count());
        }

        [Test]
        public async Task AnalyseTablesAsync_GivenSnakeCaseTablesContainingTableWithValidForeignKey_ProducesNoMessages()
        {
            var rule = new ForeignKeyMissingRule(RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                database.GetTable("no_foreign_key_parent_1").UnwrapSomeAsync().GetAwaiter().GetResult(),
                database.GetTable("no_foreign_key_child_with_key").UnwrapSomeAsync().GetAwaiter().GetResult()
            };

            var messages = await rule.AnalyseTablesAsync(tables).ConfigureAwait(false);

            Assert.Zero(messages.Count());
        }

        [Test]
        public void AnalyseTables_GivenCamelCaseTablesContainingTableWithValidForeignKey_ProducesNoMessages()
        {
            var rule = new ForeignKeyMissingRule(RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                database.GetTable("NoForeignKeyChildWithKey").UnwrapSomeAsync().GetAwaiter().GetResult(),
                database.GetTable("NoForeignKeyParent1").UnwrapSomeAsync().GetAwaiter().GetResult()
            };

            var messages = rule.AnalyseTables(tables);

            Assert.Zero(messages.Count());
        }

        [Test]
        public async Task AnalyseTablesAsync_GivenCamelCaseTablesContainingTableWithValidForeignKey_ProducesNoMessages()
        {
            var rule = new ForeignKeyMissingRule(RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                database.GetTable("NoForeignKeyChildWithKey").UnwrapSomeAsync().GetAwaiter().GetResult(),
                database.GetTable("NoForeignKeyParent1").UnwrapSomeAsync().GetAwaiter().GetResult()
            };

            var messages = await rule.AnalyseTablesAsync(tables).ConfigureAwait(false);

            Assert.Zero(messages.Count());
        }

        [Test]
        public void AnalyseTables_GivenSnakeCaseTablesContainingTableWithValidForeignKey_ProducesMessages()
        {
            var rule = new ForeignKeyMissingRule(RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                database.GetTable("no_foreign_key_parent_1").UnwrapSomeAsync().GetAwaiter().GetResult(),
                database.GetTable("no_foreign_key_child_without_key").UnwrapSomeAsync().GetAwaiter().GetResult()
            };

            var messages = rule.AnalyseTables(tables);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public async Task AnalyseTablesAsync_GivenSnakeCaseTablesContainingTableWithValidForeignKey_ProducesMessages()
        {
            var rule = new ForeignKeyMissingRule(RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                database.GetTable("no_foreign_key_parent_1").UnwrapSomeAsync().GetAwaiter().GetResult(),
                database.GetTable("no_foreign_key_child_without_key").UnwrapSomeAsync().GetAwaiter().GetResult()
            };

            var messages = await rule.AnalyseTablesAsync(tables).ConfigureAwait(false);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public void AnalyseTables_GivenCamelCaseTablesContainingTableWithValidForeignKey_ProducesMessages()
        {
            var rule = new ForeignKeyMissingRule(RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                database.GetTable("NoForeignKeyChildWithoutKey").UnwrapSomeAsync().GetAwaiter().GetResult(),
                database.GetTable("NoForeignKeyParent1").UnwrapSomeAsync().GetAwaiter().GetResult()
            };

            var messages = rule.AnalyseTables(tables);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public async Task AnalyseTablesAsync_GivenCamelCaseTablesContainingTableWithValidForeignKey_ProducesMessages()
        {
            var rule = new ForeignKeyMissingRule(RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                database.GetTable("NoForeignKeyChildWithoutKey").UnwrapSomeAsync().GetAwaiter().GetResult(),
                database.GetTable("NoForeignKeyParent1").UnwrapSomeAsync().GetAwaiter().GetResult()
            };

            var messages = await rule.AnalyseTablesAsync(tables).ConfigureAwait(false);

            Assert.NotZero(messages.Count());
        }
    }
}
