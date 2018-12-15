﻿using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.Schematic.PostgreSql.Tests.Integration
{
    internal partial class PostgreSqlRelationalDatabaseTableProviderTests : PostgreSqlTest
    {
        [Test]
        public async Task Columns_WhenGivenTableWithOneColumn_ReturnsColumnCollectionWithOneValue()
        {
            var table = await GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var count = table.Columns.Count;

            Assert.AreEqual(1, count);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithOneColumn_ReturnsColumnWithCorrectName()
        {
            var table = await GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var column = table.Columns.Single();
            const string columnName = "test_column";

            Assert.AreEqual(columnName, column.Name.LocalName);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithMultipleColumns_ReturnsColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "middle_name", "last_name" };
            var table = await GetTableAsync("table_test_table_4").ConfigureAwait(false);
            var columns = table.Columns;
            var columnNames = columns.Select(c => c.Name.LocalName);

            Assert.IsTrue(expectedColumnNames.SequenceEqual(columnNames));
        }

        [Test]
        public async Task Columns_WhenGivenTableWithNullableColumn_ColumnReturnsIsNullableTrue()
        {
            const string tableName = "table_test_table_1";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Single();

            Assert.IsTrue(column.IsNullable);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithNotNullableColumn_ColumnReturnsIsNullableFalse()
        {
            const string tableName = "table_test_table_2";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Single();

            Assert.IsFalse(column.IsNullable);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithColumnWithNoDefaultValue_ColumnReturnsNullDefaultValue()
        {
            const string tableName = "table_test_table_1";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Single();

            Assert.IsNull(column.DefaultValue);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithNonComputedColumn_ReturnsIsComputedFalse()
        {
            const string tableName = "table_test_table_1";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Single();

            Assert.IsFalse(column.IsComputed);
        }

        [Test]
        public async Task Columns_WhenGivenTableColumnWithoutIdentity_ReturnsNullAutoincrement()
        {
            const string tableName = "table_test_table_1";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Single();

            Assert.IsNull(column.AutoIncrement);
        }

        [Test]
        public async Task Columns_WhenGivenTableColumnWithIdentity_ReturnsNotNullAutoincrement()
        {
            const string tableName = "table_test_table_35";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Last();

            Assert.IsNotNull(column.AutoIncrement);
        }

        [Test]
        public async Task Columns_WhenGivenTableColumnWithIdentity_ReturnsCorrectInitialValue()
        {
            const string tableName = "table_test_table_35";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Last();

            Assert.AreEqual(1, column.AutoIncrement.InitialValue);
        }

        [Test]
        public async Task Columns_WhenGivenTableColumnWithIdentity_ReturnsCorrectIncrement()
        {
            const string tableName = "table_test_table_35";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Last();

            Assert.AreEqual(1, column.AutoIncrement.Increment);
        }
    }
}