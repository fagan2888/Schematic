﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Oracle.Comments;

namespace SJP.Schematic.Oracle.Tests.Integration.Comments
{
    internal sealed class OracleTableCommentProviderTests : OracleTest
    {
        private IRelationalDatabaseTableCommentProvider TableCommentProvider => new OracleTableCommentProvider(Connection, IdentifierDefaults, IdentifierResolver);

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create table table_comment_table_1 ( test_column_1 number )").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table table_comment_table_2 ( test_column_1 number, test_column_2 number, test_column_3 number )").ConfigureAwait(false);

            await Connection.ExecuteAsync("comment on table table_comment_table_2 is 'This is a test table comment.'").ConfigureAwait(false);
            await Connection.ExecuteAsync("comment on column table_comment_table_2.test_column_2 is 'This is a column comment.'").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop table table_comment_table_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_comment_table_2").ConfigureAwait(false);
        }

        private Task<IRelationalDatabaseTableComments> GetTableCommentsAsync(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            lock (_lock)
            {
                if (!_commentsCache.TryGetValue(tableName, out var lazyComment))
                {
                    lazyComment = new AsyncLazy<IRelationalDatabaseTableComments>(() => TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync());
                    _commentsCache[tableName] = lazyComment;
                }

                return lazyComment.Task;
            }
        }

        private readonly object _lock = new object();
        private readonly Dictionary<Identifier, AsyncLazy<IRelationalDatabaseTableComments>> _commentsCache = new Dictionary<Identifier, AsyncLazy<IRelationalDatabaseTableComments>>();

        [Test]
        public async Task GetTableComments_WhenTablePresent_ReturnsTableComment()
        {
            var tableIsSome = await TableCommentProvider.GetTableComments("table_comment_table_1").IsSome.ConfigureAwait(false);
            Assert.IsTrue(tableIsSome);
        }

        [Test]
        public async Task GetTableComments_WhenTablePresent_ReturnsTableWithCorrectName()
        {
            const string tableName = "TABLE_COMMENT_TABLE_1";
            var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(tableName, tableComments.TableName.LocalName);
        }

        [Test]
        public async Task GetTableComments_WhenTablePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier("table_comment_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "TABLE_COMMENT_TABLE_1");

            var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedTableName, tableComments.TableName);
        }

        [Test]
        public async Task GetTableComments_WhenTablePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier(IdentifierDefaults.Schema, "table_comment_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "TABLE_COMMENT_TABLE_1");

            var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedTableName, tableComments.TableName);
        }

        [Test]
        public async Task GetTableComments_WhenTablePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "table_comment_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "TABLE_COMMENT_TABLE_1");

            var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedTableName, tableComments.TableName);
        }

        [Test]
        public async Task GetTableComments_WhenTablePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "TABLE_COMMENT_TABLE_1");

            var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(tableName, tableComments.TableName);
        }

        [Test]
        public async Task GetTableComments_WhenTablePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "table_comment_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "TABLE_COMMENT_TABLE_1");

            var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedTableName, tableComments.TableName);
        }

        [Test]
        public async Task GetTableComments_WhenTablePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier("A", "B", IdentifierDefaults.Schema, "table_comment_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "TABLE_COMMENT_TABLE_1");

            var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedTableName, tableComments.TableName);
        }

        [Test]
        public async Task GetTableComments_WhenTableMissing_ReturnsNone()
        {
            var tableIsNone = await TableCommentProvider.GetTableComments("table_that_doesnt_exist").IsNone.ConfigureAwait(false);
            Assert.IsTrue(tableIsNone);
        }

        [Test]
        public async Task GetAllTableComments_WhenEnumerated_ContainsTableComments()
        {
            var tableComments = await TableCommentProvider.GetAllTableComments().ConfigureAwait(false);

            Assert.NotZero(tableComments.Count);
        }

        [Test]
        public async Task GetAllTableComments_WhenEnumerated_ContainsTestTableComment()
        {
            var tableComments = await TableCommentProvider.GetAllTableComments().ConfigureAwait(false);
            var containsTestTable = tableComments.Any(t => t.TableName.LocalName == "TABLE_COMMENT_TABLE_1");

            Assert.IsTrue(containsTestTable);
        }

        [Test]
        public async Task GetTableComments_WhenTableMissingComment_ReturnsNone()
        {
            var comments = await GetTableCommentsAsync("TABLE_COMMENT_TABLE_1").ConfigureAwait(false);

            Assert.IsTrue(comments.Comment.IsNone);
        }

        [Test]
        public async Task GetTableComments_WhenTableMissingColumnComments_ReturnsLookupKeyedWithColumnNames()
        {
            var comments = await GetTableCommentsAsync("TABLE_COMMENT_TABLE_1").ConfigureAwait(false);

            var columnComments = comments.ColumnComments;
            var hasColumns = columnComments.Count == 1 && columnComments.Keys.Single().LocalName == "TEST_COLUMN_1";

            Assert.IsTrue(hasColumns);
        }

        [Test]
        public async Task GetTableComments_WhenTableMissingColumnComments_ReturnsLookupWithOnlyNoneValues()
        {
            var comments = await GetTableCommentsAsync("TABLE_COMMENT_TABLE_1").ConfigureAwait(false);

            var columnComments = comments.ColumnComments;
            var hasOnlyNones = columnComments.Count == 1 && columnComments.Values.Single().IsNone;

            Assert.IsTrue(hasOnlyNones);
        }

        [Test]
        public async Task GetTableComments_WhenTableContainsComment_ReturnsExpectedValue()
        {
            const string expectedComment = "This is a test table comment.";
            var comments = await GetTableCommentsAsync("TABLE_COMMENT_TABLE_2").ConfigureAwait(false);

            var tableComment = comments.Comment.UnwrapSome();

            Assert.AreEqual(expectedComment, tableComment);
        }

        [Test]
        public async Task GetTableComments_PropertyGetForColumnComments_ReturnsAllColumnNamesAsKeys()
        {
            var columnNames = new[]
            {
                new Identifier("TEST_COLUMN_1"),
                new Identifier("TEST_COLUMN_2"),
                new Identifier("TEST_COLUMN_3")
            };
            var comments = await GetTableCommentsAsync("TABLE_COMMENT_TABLE_2").ConfigureAwait(false);

            var columnComments = comments.ColumnComments;
            var allKeysPresent = columnNames.All(columnComments.ContainsKey);

            Assert.IsTrue(allKeysPresent);
        }

        [Test]
        public async Task GetTableComments_PropertyGetForColumnComments_ReturnsCorrectNoneOrSome()
        {
            var expectedNoneStates = new[]
            {
                true,
                false,
                true
            };
            var comments = await GetTableCommentsAsync("TABLE_COMMENT_TABLE_2").ConfigureAwait(false);

            var columnComments = comments.ColumnComments;
            var noneStates = new[]
            {
                columnComments["TEST_COLUMN_1"].IsNone,
                columnComments["TEST_COLUMN_2"].IsNone,
                columnComments["TEST_COLUMN_3"].IsNone,
            };

            var seqEqual = expectedNoneStates.SequenceEqual(noneStates);

            Assert.IsTrue(seqEqual);
        }

        [Test]
        public async Task GetTableComments_PropertyGetForColumnComments_ReturnsCorrectCommentValue()
        {
            const string expectedComment = "This is a column comment.";
            var comments = await GetTableCommentsAsync("TABLE_COMMENT_TABLE_2").ConfigureAwait(false);

            var comment = comments.ColumnComments["TEST_COLUMN_2"].UnwrapSome();

            Assert.AreEqual(expectedComment, comment);
        }
    }
}
