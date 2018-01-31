﻿using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite;

namespace SJP.Schematic.DataAccess.OrmLite.Tests.Integration
{
    [TestFixture]
    internal class ViewGeneratorTests : SqliteTest
    {
        private IRelationalDatabase Database => new SqliteRelationalDatabase(Dialect, Connection);

        private IRelationalDatabaseView GetView(Identifier viewName) => Database.GetView(viewName);

        private IDatabaseViewGenerator ViewGenerator => new ViewGenerator(new PascalCaseNameProvider(), TestNamespace);

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync(@"
create view test_view_1 as
select
    1 as testint,
    2.45 as testdouble,
    X'DEADBEEF' as testblob,
    CURRENT_TIMESTAMP as testdatetime,
    'asd' as teststring
").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"create table view_test_table_1 (
    testint integer not null primary key autoincrement,
    testdecimal numeric default 2.45,
    testblob blob default X'DEADBEEF',
    testdatetime datetime default CURRENT_TIMESTAMP,
    teststring text default 'asd'
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create view test_view_2 as select * from view_test_table_1").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop view test_view_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop view test_view_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table view_test_table_1").ConfigureAwait(false);
        }

        [Test]
        public void Generate_GivenViewWithLiteralColumnTypes_GeneratesExpectedOutput()
        {
            var view = GetView("test_view_1");
            var generator = ViewGenerator;

            var expected = TestView1Output;
            var result = generator.Generate(view);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Generate_GivenViewSelectingFromTable_GeneratesExpectedOutput()
        {
            var view = GetView("test_view_2");
            var generator = ViewGenerator;

            var expected = TestView2Output;
            var result = generator.Generate(view);

            Assert.AreEqual(expected, result);
        }

        private const string TestNamespace = "PocoTestNamespace";

        private readonly string TestView1Output = @"using System;
using ServiceStack.DataAnnotations;

namespace PocoTestNamespace.Main
{
    /// <summary>
    /// A mapping class to query the <c>test_view_1</c> view.
    /// </summary>
    [Schema(""main"")]
    [Alias(""test_view_1"")]
    public class TestView1
    {
        /// <summary>
        /// The <c>testint</c> column.
        /// </summary>
        [Alias(""testint"")]
        public long? Testint { get; set; }

        /// <summary>
        /// The <c>testdouble</c> column.
        /// </summary>
        [Alias(""testdouble"")]
        public double? Testdouble { get; set; }

        /// <summary>
        /// The <c>testblob</c> column.
        /// </summary>
        [Alias(""testblob"")]
        public byte[] Testblob { get; set; }

        /// <summary>
        /// The <c>testdatetime</c> column.
        /// </summary>
        [Alias(""testdatetime"")]
        public string Testdatetime { get; set; }

        /// <summary>
        /// The <c>teststring</c> column.
        /// </summary>
        [Alias(""teststring"")]
        public string Teststring { get; set; }
    }
}";
        private readonly string TestView2Output = @"using System;
using ServiceStack.DataAnnotations;

namespace PocoTestNamespace.Main
{
    /// <summary>
    /// A mapping class to query the <c>test_view_2</c> view.
    /// </summary>
    [Schema(""main"")]
    [Alias(""test_view_2"")]
    public class TestView2
    {
        /// <summary>
        /// The <c>testint</c> column.
        /// </summary>
        [Alias(""testint"")]
        public long? Testint { get; set; }

        /// <summary>
        /// The <c>testdecimal</c> column.
        /// </summary>
        [Alias(""testdecimal"")]
        public decimal? Testdecimal { get; set; }

        /// <summary>
        /// The <c>testblob</c> column.
        /// </summary>
        [Alias(""testblob"")]
        public byte[] Testblob { get; set; }

        /// <summary>
        /// The <c>testdatetime</c> column.
        /// </summary>
        [Alias(""testdatetime"")]
        public decimal? Testdatetime { get; set; }

        /// <summary>
        /// The <c>teststring</c> column.
        /// </summary>
        [Alias(""teststring"")]
        public string Teststring { get; set; }
    }
}";
    }
}