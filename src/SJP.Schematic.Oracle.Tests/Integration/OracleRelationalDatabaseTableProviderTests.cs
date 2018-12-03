﻿using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Tests.Integration
{
    internal partial class OracleRelationalDatabaseTableProviderTests : OracleTest
    {
        public OracleRelationalDatabaseTableProviderTests()
        {
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();
            var database = new OracleRelationalDatabase(Dialect, Connection, identifierResolver);
            IdentifierDefaults = new DatabaseIdentifierDefaultsBuilder()
                .WithServer(database.ServerName)
                .WithDatabase(database.DatabaseName)
                .WithSchema(database.DefaultSchema)
                .Build();
            TableProvider = new OracleRelationalDatabaseTableProvider(Connection, IdentifierDefaults, identifierResolver, Dialect.TypeProvider);
        }

        private IDatabaseIdentifierDefaults IdentifierDefaults { get; }
        private IRelationalDatabaseTableProvider TableProvider { get; }

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create table db_test_table_1 ( title varchar2(200) )").ConfigureAwait(false);

            await Connection.ExecuteAsync("create table table_test_table_1 ( test_column number )").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table table_test_table_2 ( test_column number not null primary key )").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_3 (
    test_column number,
    constraint pk_test_table_3 primary key (test_column)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_4 (
    first_name varchar2(50),
    middle_name varchar2(50),
    last_name varchar2(50),
    constraint pk_test_table_4 primary key (first_name, last_name, middle_name)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table table_test_table_5 ( test_column number not null unique )").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_6 (
    test_column number,
    constraint uk_test_table_6 unique (test_column)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_7 (
    first_name varchar2(50),
    middle_name varchar2(50),
    last_name varchar2(50),
    constraint uk_test_table_7 unique (first_name, last_name, middle_name)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table table_test_table_8 (test_column number)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create index ix_test_table_8 on table_test_table_8 (test_column)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_9 (
    first_name varchar2(50),
    middle_name varchar2(50),
    last_name varchar2(50)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create index ix_test_table_9 on table_test_table_9 (first_name, last_name, middle_name)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_13 (
    first_name varchar2(50),
    middle_name varchar2(50),
    last_name varchar2(50)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create unique index ix_test_table_13 on table_test_table_13 (first_name, last_name, middle_name)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_14 (
    test_column number not null,
    constraint ck_test_table_14 check (test_column > 1)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_15 (
    first_name_parent varchar2(50),
    middle_name_parent varchar2(50),
    last_name_parent varchar2(50),
    constraint pk_test_table_15 primary key (first_name_parent),
    constraint uk_test_table_15 unique (last_name_parent, middle_name_parent)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_16 (
    first_name_child varchar2(50),
    middle_name varchar2(50),
    last_name varchar2(50),
    constraint fk_test_table_16 foreign key (first_name_child) references table_test_table_15 (first_name_parent)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_17 (
    first_name varchar2(50),
    middle_name_child varchar2(50),
    last_name_child varchar2(50),
    constraint fk_test_table_17 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent)
)").ConfigureAwait(false);
            /*await Connection.ExecuteAsync(@"
create table table_test_table_18 (
    first_name_child varchar2(50),
    middle_name_child varchar2(50),
    last_name_child varchar2(50),
    constraint fk_test_table_18 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on update cascade
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_19 (
    first_name_child varchar2(50),
    middle_name_child varchar2(50),
    last_name_child varchar2(50),
    constraint fk_test_table_19 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on update set null
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_20 (
    first_name_child varchar2(50),
    middle_name_child varchar2(50),
    last_name_child varchar2(50),
    constraint fk_test_table_20 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on update set default
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_21 (
    first_name_child varchar2(50),
    middle_name_child varchar2(50),
    last_name_child varchar2(50),
    constraint fk_test_table_21 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on update cascade
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_22 (
    first_name_child varchar2(50),
    middle_name_child varchar2(50),
    last_name_child varchar2(50),
    constraint fk_test_table_22 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on update set null
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_23 (
    first_name_child varchar2(50),
    middle_name_child varchar2(50),
    last_name_child varchar2(50),
    constraint fk_test_table_23 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on update set default
)").ConfigureAwait(false);*/
            await Connection.ExecuteAsync(@"
create table table_test_table_24 (
    first_name_child varchar2(50),
    middle_name_child varchar2(50),
    last_name_child varchar2(50),
    constraint fk_test_table_24 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on delete cascade
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_25 (
    first_name_child varchar2(50),
    middle_name_child varchar2(50),
    last_name_child varchar2(50),
    constraint fk_test_table_25 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on delete set null
)").ConfigureAwait(false);
            /*await Connection.ExecuteAsync(@"
create table table_test_table_26 (
    first_name_child varchar2(50),
    middle_name_child varchar2(50),
    last_name_child varchar2(50),
    constraint fk_test_table_26 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on delete set default
)").ConfigureAwait(false);*/
            await Connection.ExecuteAsync(@"
create table table_test_table_27 (
    first_name_child varchar2(50),
    middle_name_child varchar2(50),
    last_name_child varchar2(50),
    constraint fk_test_table_27 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on delete cascade
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_28 (
    first_name_child varchar2(50),
    middle_name_child varchar2(50),
    last_name_child varchar2(50),
    constraint fk_test_table_28 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on delete set null
)").ConfigureAwait(false);
            /*await Connection.ExecuteAsync(@"
create table table_test_table_29 (
    first_name_child varchar2(50),
    middle_name_child varchar2(50),
    last_name_child varchar2(50),
    constraint fk_test_table_29 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on delete set default
)").ConfigureAwait(false);*/
            await Connection.ExecuteAsync(@"
create table table_test_table_30 (
    first_name_child varchar2(50),
    middle_name_child varchar2(50),
    last_name_child varchar2(50),
    constraint fk_test_table_30 foreign key (first_name_child) references table_test_table_15 (first_name_parent)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("alter table table_test_table_30 disable constraint fk_test_table_30").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_31 (
    first_name_child varchar2(50),
    middle_name_child varchar2(50),
    last_name_child varchar2(50),
    constraint fk_test_table_31 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("alter table table_test_table_31 disable constraint fk_test_table_31").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_32 (
    test_column number not null,
    constraint ck_test_table_32 check (test_column > 1)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("alter table table_test_table_32 disable constraint ck_test_table_32").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table table_test_table_33 ( test_column number default 1 not null )").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"create table table_test_table_34 (
    test_column_1 number,
    test_column_2 number,
    test_column_3 as (test_column_1 + test_column_2)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table table_test_table_35 ( test_column number primary key )").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table trigger_test_table_1 (table_id number primary key not null)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table trigger_test_table_2 (table_id number primary key not null)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_1
before insert on trigger_test_table_1
for each row
begin
    null;
end;
").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_2
before update on trigger_test_table_1
for each row
begin
    null;
end;
").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_3
before delete on trigger_test_table_1
for each row
begin
    null;
end;
").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_4
after insert on trigger_test_table_1
for each row
begin
    null;
end;
").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_5
after update on trigger_test_table_1
for each row
begin
    null;
end;
").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_6
after delete on trigger_test_table_1
for each row
begin
    null;
end;
").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_7
after insert or update or delete on trigger_test_table_1
for each row
begin
    null;
end;
").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop table db_test_table_1").ConfigureAwait(false);

            await Connection.ExecuteAsync("drop table table_test_table_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_3").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_4").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_5").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_6").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_7").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_8").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_9").ConfigureAwait(false);
            //await Connection.ExecuteAsync("drop table table_test_table_10").ConfigureAwait(false);
            //await Connection.ExecuteAsync("drop table table_test_table_11").ConfigureAwait(false);
            //await Connection.ExecuteAsync("drop table table_test_table_12").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_13").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_14").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_16").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_17").ConfigureAwait(false);
            //await Connection.ExecuteAsync("drop table table_test_table_18").ConfigureAwait(false);
            //await Connection.ExecuteAsync("drop table table_test_table_19").ConfigureAwait(false);
            //await Connection.ExecuteAsync("drop table table_test_table_20").ConfigureAwait(false);
            //await Connection.ExecuteAsync("drop table table_test_table_21").ConfigureAwait(false);
            //await Connection.ExecuteAsync("drop table table_test_table_22").ConfigureAwait(false);
            //await Connection.ExecuteAsync("drop table table_test_table_23").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_24").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_25").ConfigureAwait(false);
            //await Connection.ExecuteAsync("drop table table_test_table_26").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_27").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_28").ConfigureAwait(false);
            //await Connection.ExecuteAsync("drop table table_test_table_29").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_30").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_31").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_15").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_32").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_33").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_34").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_35").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table trigger_test_table_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table trigger_test_table_2").ConfigureAwait(false);
        }

        [Test]
        public void GetTable_WhenTablePresent_ReturnsTable()
        {
            var table = TableProvider.GetTable("db_test_table_1");
            Assert.IsTrue(table.IsSome);
        }

        [Test]
        public void GetTable_WhenTablePresent_ReturnsTableWithCorrectName()
        {
            const string tableName = "db_test_table_1";
            const string expectedTableName = "DB_TEST_TABLE_1";
            var table = TableProvider.GetTable(tableName).UnwrapSome();

            Assert.AreEqual(expectedTableName, table.Name.LocalName);
        }

        [Test]
        public void GetTable_WhenTablePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier("db_test_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_TABLE_1");

            var table = TableProvider.GetTable(tableName).UnwrapSome();

            Assert.AreEqual(expectedTableName, table.Name);
        }

        [Test]
        public void GetTable_WhenTablePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier(IdentifierDefaults.Schema, "db_test_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_TABLE_1");

            var table = TableProvider.GetTable(tableName).UnwrapSome();

            Assert.AreEqual(expectedTableName, table.Name);
        }

        [Test]
        public void GetTable_WhenTablePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_TABLE_1");

            var table = TableProvider.GetTable(tableName).UnwrapSome();

            Assert.AreEqual(expectedTableName, table.Name);
        }

        [Test]
        public void GetTable_WhenTablePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_TABLE_1");

            var table = TableProvider.GetTable(tableName).UnwrapSome();

            Assert.AreEqual(tableName, table.Name);
        }

        [Test]
        public void GetTable_WhenTablePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_TABLE_1");

            var table = TableProvider.GetTable(tableName).UnwrapSome();

            Assert.AreEqual(expectedTableName, table.Name);
        }

        [Test]
        public void GetTable_WhenTablePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier("A", "B", IdentifierDefaults.Schema, "db_test_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_TABLE_1");

            var table = TableProvider.GetTable(tableName).UnwrapSome();

            Assert.AreEqual(expectedTableName, table.Name);
        }

        [Test]
        public void GetTable_WhenTableMissing_ReturnsNone()
        {
            var table = TableProvider.GetTable("table_that_doesnt_exist");
            Assert.IsTrue(table.IsNone);
        }

        [Test]
        public async Task GetTableAsync_WhenTablePresent_ReturnsTable()
        {
            var tableIsSome = await TableProvider.GetTableAsync("db_test_table_1").IsSome.ConfigureAwait(false);
            Assert.IsTrue(tableIsSome);
        }

        [Test]
        public async Task GetTableAsync_WhenTablePresent_ReturnsTableWithCorrectName()
        {
            const string tableName = "db_test_table_1";
            const string expectedTableName = "DB_TEST_TABLE_1";
            var table = await TableProvider.GetTableAsync(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedTableName, table.Name.LocalName);
        }

        [Test]
        public async Task GetTableAsync_WhenTablePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier("db_test_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_TABLE_1");

            var table = await TableProvider.GetTableAsync(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedTableName, table.Name);
        }

        [Test]
        public async Task GetTableAsync_WhenTablePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier(IdentifierDefaults.Schema, "db_test_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_TABLE_1");

            var table = await TableProvider.GetTableAsync(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedTableName, table.Name);
        }

        [Test]
        public async Task GetTableAsync_WhenTablePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_TABLE_1");

            var table = await TableProvider.GetTableAsync(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedTableName, table.Name);
        }

        [Test]
        public async Task GetTableAsync_WhenTablePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_TABLE_1");

            var table = await TableProvider.GetTableAsync(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(tableName, table.Name);
        }

        [Test]
        public async Task GetTableAsync_WhenTablePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_TABLE_1");

            var table = await TableProvider.GetTableAsync(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedTableName, table.Name);
        }

        [Test]
        public async Task GetTableAsync_WhenTablePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier("A", "B", IdentifierDefaults.Schema, "db_test_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_TABLE_1");

            var table = await TableProvider.GetTableAsync(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedTableName, table.Name);
        }

        [Test]
        public async Task GetTableAsync_WhenTableMissing_ReturnsNone()
        {
            var tableIsNone = await TableProvider.GetTableAsync("table_that_doesnt_exist").IsNone.ConfigureAwait(false);
            Assert.IsTrue(tableIsNone);
        }

        [Test]
        public void Tables_WhenEnumerated_ContainsTables()
        {
            var tables = TableProvider.Tables.ToList();

            Assert.NotZero(tables.Count);
        }

        [Test]
        public void Tables_WhenEnumerated_ContainsTestTable()
        {
            const string expectedTableName = "DB_TEST_TABLE_1";
            var containsTestTable = TableProvider.Tables.Any(t => t.Name.LocalName == expectedTableName);

            Assert.True(containsTestTable);
        }

        [Test]
        public async Task TablesAsync_WhenEnumerated_ContainsTables()
        {
            var tables = await TableProvider.TablesAsync().ConfigureAwait(false);

            Assert.NotZero(tables.Count);
        }

        [Test]
        public async Task TablesAsync_WhenEnumerated_ContainsTestTable()
        {
            const string expectedTableName = "DB_TEST_TABLE_1";
            var tables = await TableProvider.TablesAsync().ConfigureAwait(false);
            var containsTestTable = tables.Any(t => t.Name.LocalName == expectedTableName);

            Assert.True(containsTestTable);
        }
    }
}