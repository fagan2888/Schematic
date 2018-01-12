﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.MySql.Tests.Integration
{
    [TestFixture]
    internal class MySqlRelationalDatabaseTests : MySqlTest
    {
        private IRelationalDatabase Database => new MySqlRelationalDatabase(Dialect, Connection);

        [Test]
        public void Database_PropertyGet_ShouldMatchConnectionDatabase()
        {
            Assert.AreEqual(Database.DatabaseName, Connection.Database);
        }

        [Test]
        public void DefaultSchema_PropertyGet_ShouldEqualConnectionDefaultSchema()
        {
            Assert.AreEqual(Connection.Database, Database.DefaultSchema);
        }

        [TestFixture]
        internal class TableTests : MySqlTest
        {
            [OneTimeSetUp]
            public async Task Init()
            {
                await Connection.ExecuteAsync("create table db_test_table_1 ( title nvarchar(200) )").ConfigureAwait(false);
            }

            [OneTimeTearDown]
            public async Task CleanUp()
            {
                await Connection.ExecuteAsync("drop table db_test_table_1").ConfigureAwait(false);
            }

            private IRelationalDatabase Database => new MySqlRelationalDatabase(Dialect, Connection);

            [Test]
            public void TableExists_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.TableExists(null));
            }

            [Test]
            public void TableExists_WhenTablePresent_ReturnsTrue()
            {
                var tableExists = Database.TableExists("db_test_table_1");
                Assert.IsTrue(tableExists);
            }

            [Test]
            public void TableExists_WhenTableMissing_ReturnsFalse()
            {
                var tableExists = Database.TableExists("table_that_doesnt_exist");
                Assert.IsFalse(tableExists);
            }

            [Test]
            public void GetTable_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetTable(null));
            }

            [Test]
            public void GetTable_WhenTablePresent_ReturnsTable()
            {
                var table = Database.GetTable("db_test_table_1");
                Assert.NotNull(table);
            }

            [Test]
            public void GetTable_WhenTablePresent_ReturnsTableWithCorrectName()
            {
                const string tableName = "db_test_table_1";
                var table = Database.GetTable(tableName);

                Assert.AreEqual(tableName, table.Name.LocalName);
            }

            [Test]
            public void GetTable_WhenTableMissing_ReturnsNull()
            {
                var table = Database.GetTable("table_that_doesnt_exist");
                Assert.IsNull(table);
            }

            [Test]
            public void TableExistsAsync_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.ThrowsAsync<ArgumentNullException>(async () => await Database.TableExistsAsync(null).ConfigureAwait(false));
            }

            [Test]
            public async Task TableExistsAsync_WhenTablePresent_ReturnsTrue()
            {
                var tableExists = await Database.TableExistsAsync("db_test_table_1").ConfigureAwait(false);
                Assert.IsTrue(tableExists);
            }

            [Test]
            public async Task TableExistsAsync_WhenTableMissing_ReturnsFalse()
            {
                var tableExists = await Database.TableExistsAsync("table_that_doesnt_exist").ConfigureAwait(false);
                Assert.IsFalse(tableExists);
            }

            [Test]
            public void GetTableAsync_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.ThrowsAsync<ArgumentNullException>(async () => await Database.GetTableAsync(null).ConfigureAwait(false));
            }

            [Test]
            public async Task GetTableAsync_WhenTablePresent_ReturnsTable()
            {
                var table = await Database.GetTableAsync("db_test_table_1").ConfigureAwait(false);
                Assert.NotNull(table);
            }

            [Test]
            public async Task GetTableAsync_WhenTablePresent_ReturnsTableWithCorrectName()
            {
                const string tableName = "db_test_table_1";
                var table = await Database.GetTableAsync(tableName).ConfigureAwait(false);

                Assert.AreEqual(tableName, table.Name.LocalName);
            }

            [Test]
            public async Task GetTableAsync_WhenTableMissing_ReturnsNull()
            {
                var table = await Database.GetTableAsync("table_that_doesnt_exist").ConfigureAwait(false);
                Assert.IsNull(table);
            }

            [Test]
            public void Tables_WhenEnumerated_ContainsTables()
            {
                var tables = Database.Tables.ToList();

                Assert.NotZero(tables.Count);
            }

            [Test]
            public void Tables_WhenEnumerated_ContainsTestTable()
            {
                var containsTestTable = Database.Tables.Any(t => t.Name.LocalName == "db_test_table_1");

                Assert.True(containsTestTable);
            }

            [Test]
            public async Task TablesAsync_WhenSubscribed_ContainsTables()
            {
                var tables = await Database.TablesAsync().ConfigureAwait(false);
                var count = await tables.Count().ConfigureAwait(false);

                Assert.NotZero(count);
            }

            [Test]
            public async Task TablesAsync_WhenSubscribed_ContainsTestTable()
            {
                var tables = await Database.TablesAsync().ConfigureAwait(false);
                var containsTestTable = await tables.Any(t => t.Name.LocalName == "db_test_table_1").ConfigureAwait(false);

                Assert.True(containsTestTable);
            }
        }

        [TestFixture]
        internal class ViewTests : MySqlTest
        {
            [OneTimeSetUp]
            public async Task Init()
            {
                await Connection.ExecuteAsync("create view db_test_view_1 as select 1 as dummy").ConfigureAwait(false);
            }

            [OneTimeTearDown]
            public async Task CleanUp()
            {
                await Connection.ExecuteAsync("drop view db_test_view_1").ConfigureAwait(false);
            }

            private IRelationalDatabase Database => new MySqlRelationalDatabase(Dialect, Connection);

            [Test]
            public void ViewExists_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.ViewExists(null));
            }

            [Test]
            public void ViewExists_WhenViewPresent_ReturnsTrue()
            {
                var viewExists = Database.ViewExists("db_test_view_1");
                Assert.IsTrue(viewExists);
            }

            [Test]
            public void ViewExists_WhenViewMissing_ReturnsFalse()
            {
                var viewExists = Database.ViewExists("view_that_doesnt_exist");
                Assert.IsFalse(viewExists);
            }

            [Test]
            public void GetView_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetView(null));
            }

            [Test]
            public void GetView_WhenViewPresent_ReturnsView()
            {
                var view = Database.GetView("db_test_view_1");
                Assert.NotNull(view);
            }

            [Test]
            public void GetView_WhenViewPresent_ReturnsViewWithCorrectName()
            {
                var database = Database;
                var viewName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_view_1");
                var view = database.GetView(viewName);

                Assert.AreEqual(viewName, view.Name);
            }

            [Test]
            public void GetView_WhenViewMissing_ReturnsNull()
            {
                var view = Database.GetView("view_that_doesnt_exist");
                Assert.IsNull(view);
            }

            [Test]
            public void ViewExistsAsync_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.ThrowsAsync<ArgumentNullException>(async () => await Database.ViewExistsAsync(null).ConfigureAwait(false));
            }

            [Test]
            public async Task ViewExistsAsync_WhenViewPresent_ReturnsTrue()
            {
                var viewExists = await Database.ViewExistsAsync("db_test_view_1").ConfigureAwait(false);
                Assert.IsTrue(viewExists);
            }

            [Test]
            public async Task ViewExistsAsync_WhenViewMissing_ReturnsFalse()
            {
                var viewExists = await Database.ViewExistsAsync("view_that_doesnt_exist").ConfigureAwait(false);
                Assert.IsFalse(viewExists);
            }

            [Test]
            public void GetViewAsync_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.ThrowsAsync<ArgumentNullException>(async () => await Database.GetViewAsync(null).ConfigureAwait(false));
            }

            [Test]
            public async Task GetViewAsync_WhenViewPresent_ReturnsView()
            {
                var view = await Database.GetViewAsync("db_test_view_1").ConfigureAwait(false);
                Assert.NotNull(view);
            }

            [Test]
            public async Task GetViewAsync_WhenViewPresent_ReturnsViewWithCorrectName()
            {
                var database = Database;
                var viewName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_view_1");
                var view = await database.GetViewAsync(viewName).ConfigureAwait(false);

                Assert.AreEqual(viewName, view.Name);
            }

            [Test]
            public async Task GetViewAsync_WhenViewMissing_ReturnsNull()
            {
                var view = await Database.GetViewAsync("view_that_doesnt_exist").ConfigureAwait(false);
                Assert.IsNull(view);
            }

            [Test]
            public void Views_WhenEnumerated_ContainsViews()
            {
                var views = Database.Views.ToList();

                Assert.NotZero(views.Count);
            }

            [Test]
            public void Views_WhenEnumerated_ContainsTestView()
            {
                const string viewName = "db_test_view_1";
                var containsTestView = Database.Views.Any(v => v.Name.LocalName == viewName);

                Assert.True(containsTestView);
            }

            [Test]
            public async Task ViewsAsync_WhenSubscribed_ContainsViews()
            {
                var views = await Database.ViewsAsync().ConfigureAwait(false);
                var count = await views.Count().ConfigureAwait(false);

                Assert.NotZero(count);
            }

            [Test]
            public async Task ViewsAsync_WhenSubscribed_ContainsTestView()
            {
                const string viewName = "db_test_view_1";
                var views = await Database.ViewsAsync().ConfigureAwait(false);
                var containsTestView = await views.Any(v => v.Name.LocalName == viewName).ConfigureAwait(false);

                Assert.True(containsTestView);
            }
        }

        [TestFixture]
        internal class SequenceTests : MySqlTest
        {
            private IRelationalDatabase Database => new MySqlRelationalDatabase(Dialect, Connection);

            [Test]
            public void SequenceExists_GivenNullSequenceName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.SequenceExists(null));
            }

            [Test]
            public void SequenceExists_GivenInvalidSequenceName_ThrowsArgumentNullException()
            {
                var schemaName = new SchemaIdentifier("asd");
                Assert.Throws<ArgumentNullException>(() => Database.SequenceExists(schemaName));
            }

            [Test]
            public void SequenceExists_GivenValidSequenceName_ReturnsFalse()
            {
                var sequenceName = new LocalIdentifier("asd");
                var sequenceExists = Database.SequenceExists(sequenceName);

                Assert.IsFalse(sequenceExists);
            }

            [Test]
            public void GetSequence_GivenNullSequenceName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetSequence(null));
            }

            [Test]
            public void GetSequence_GivenInvalidSequenceName_ThrowsArgumentNullException()
            {
                var schemaName = new SchemaIdentifier("asd");
                Assert.Throws<ArgumentNullException>(() => Database.GetSequence(schemaName));
            }

            [Test]
            public void GetSequence_GivenValidSequenceName_ReturnsNull()
            {
                var sequenceName = new LocalIdentifier("asd");
                var sequence = Database.GetSequence(sequenceName);

                Assert.IsNull(sequence);
            }

            [Test]
            public void Sequences_PropertyGet_ReturnsEmptyCollection()
            {
                var sequences = Database.Sequences.ToList();
                var count = sequences.Count;

                Assert.Zero(count);
            }

            [Test]
            public void SequenceExistsAsync_GivenNullSequenceName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.SequenceExistsAsync(null));
            }

            [Test]
            public void SequenceExistsAsync_GivenInvalidSequenceName_ThrowsArgumentNullException()
            {
                var schemaName = new SchemaIdentifier("asd");
                Assert.Throws<ArgumentNullException>(() => Database.SequenceExistsAsync(schemaName));
            }

            [Test]
            public async Task SequenceExistsAsync_GivenValidSequenceName_ReturnsFalse()
            {
                var sequenceName = new LocalIdentifier("asd");
                var sequenceExists = await Database.SequenceExistsAsync(sequenceName).ConfigureAwait(false);

                Assert.IsFalse(sequenceExists);
            }

            [Test]
            public void GetSequenceAsync_GivenNullSequenceName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetSequenceAsync(null));
            }

            [Test]
            public void GetSequenceAsync_GivenInvalidSequenceName_ThrowsArgumentNullException()
            {
                var schemaName = new SchemaIdentifier("asd");
                Assert.Throws<ArgumentNullException>(() => Database.GetSequenceAsync(schemaName));
            }

            [Test]
            public async Task GetSequenceAsync_GivenValidSequenceName_ReturnsNull()
            {
                var sequenceName = new LocalIdentifier("asd");
                var sequence = await Database.GetSequenceAsync(sequenceName).ConfigureAwait(false);

                Assert.IsNull(sequence);
            }

            [Test]
            public async Task SequencesAsync_PropertyGet_ReturnsEmptyCollection()
            {
                var sequences = await Database.SequencesAsync().ConfigureAwait(false);
                var count = await sequences.Count().ConfigureAwait(false);

                Assert.Zero(count);
            }
        }

        [TestFixture]
        internal class SynonymTests : MySqlTest
        {
            private IRelationalDatabase Database => new MySqlRelationalDatabase(Dialect, Connection);

            [Test]
            public void SynonymExists_GivenNullSynonymName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.SynonymExists(null));
            }

            [Test]
            public void SynonymExists_GivenInvalidSynonymName_ThrowsArgumentNullException()
            {
                var schemaName = new SchemaIdentifier("asd");
                Assert.Throws<ArgumentNullException>(() => Database.SynonymExists(schemaName));
            }

            [Test]
            public void SynonymExists_GivenValidSynonymName_ReturnsFalse()
            {
                var synonymName = new LocalIdentifier("asd");
                var synonymExists = Database.SynonymExists(synonymName);

                Assert.IsFalse(synonymExists);
            }

            [Test]
            public void GetSynonym_GivenNullSynonymName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetSynonym(null));
            }

            [Test]
            public void GetSynonym_GivenInvalidSynonymName_ThrowsArgumentNullException()
            {
                var schemaName = new SchemaIdentifier("asd");
                Assert.Throws<ArgumentNullException>(() => Database.GetSynonym(schemaName));
            }

            [Test]
            public void GetSynonym_GivenValidSynonymName_ReturnsNull()
            {
                var synonymName = new LocalIdentifier("asd");
                var synonym = Database.GetSynonym(synonymName);

                Assert.IsNull(synonym);
            }

            [Test]
            public void Synonyms_PropertyGet_ReturnsEmptyCollection()
            {
                var synonyms = Database.Synonyms.ToList();
                var count = synonyms.Count;

                Assert.Zero(count);
            }

            [Test]
            public void SynonymExistsAsync_GivenNullSynonymName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.SynonymExistsAsync(null));
            }

            [Test]
            public void SynonymExistsAsync_GivenInvalidSynonymName_ThrowsArgumentNullException()
            {
                var schemaName = new SchemaIdentifier("asd");
                Assert.Throws<ArgumentNullException>(() => Database.SynonymExistsAsync(schemaName));
            }

            [Test]
            public async Task SynonymExistsAsync_GivenValidSynonymName_ReturnsFalse()
            {
                var synonymName = new LocalIdentifier("asd");
                var synonymExists = await Database.SynonymExistsAsync(synonymName).ConfigureAwait(false);

                Assert.IsFalse(synonymExists);
            }

            [Test]
            public void GetSynonymAsync_GivenNullSynonymName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetSynonymAsync(null));
            }

            [Test]
            public void GetSynonymAsync_GivenInvalidSynonymName_ThrowsArgumentNullException()
            {
                var schemaName = new SchemaIdentifier("asd");
                Assert.Throws<ArgumentNullException>(() => Database.GetSynonymAsync(schemaName));
            }

            [Test]
            public async Task GetSynonymAsync_GivenValidSynonymName_ReturnsNull()
            {
                var synonymName = new LocalIdentifier("asd");
                var synonym = await Database.GetSynonymAsync(synonymName).ConfigureAwait(false);

                Assert.IsNull(synonym);
            }

            [Test]
            public async Task SynonymsAsync_PropertyGet_ReturnsEmptyCollection()
            {
                var synonyms = await Database.SynonymsAsync().ConfigureAwait(false);
                var count = await synonyms.Count().ConfigureAwait(false);

                Assert.Zero(count);
            }
        }
    }
}