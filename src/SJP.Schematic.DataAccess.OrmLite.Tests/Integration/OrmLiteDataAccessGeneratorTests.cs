﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite;

namespace SJP.Schematic.DataAccess.OrmLite.Tests.Integration
{
    internal class OrmLiteDataAccessGeneratorTests : SqliteTest
    {
        private IRelationalDatabase Database => new SqliteRelationalDatabase(Dialect, Connection);

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
        public void Generate_GivenDatabaseWithTablesAndViews_GeneratesFilesInExpectedLocations()
        {
            var nameProvider = new PascalCaseNameProvider();
            var generator = new OrmLiteDataAccessGenerator(Database, nameProvider);

            var testProjectDir = Path.Combine(Environment.CurrentDirectory, "ormlitetest");

            var projectPath = Path.Combine(testProjectDir, "DataAccessGeneratorTest.csproj");
            var tablesDir = Path.Combine(testProjectDir, "Tables");
            var viewsDir = Path.Combine(testProjectDir, "Views");

            var expectedTable1Path = Path.Combine(tablesDir, "Main", "ViewTestTable1.cs");
            var expectedView1Path = Path.Combine(viewsDir, "Main", "TestView1.cs");
            var expectedView2Path = Path.Combine(viewsDir, "Main", "TestView2.cs");

            var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                [testProjectDir + "\\"] = new MockDirectoryData(),
                [expectedTable1Path] = MockFileData.NullObject,
                [expectedView1Path] = MockFileData.NullObject,
                [expectedView2Path] = MockFileData.NullObject
            });

            generator.Generate(mockFs, projectPath, TestNamespace);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(mockFs.FileExists(projectPath));
                Assert.IsTrue(mockFs.Directory.Exists(tablesDir));
                Assert.IsTrue(mockFs.Directory.Exists(viewsDir));
                Assert.IsTrue(mockFs.FileExists(expectedTable1Path));
                Assert.IsTrue(mockFs.FileExists(expectedView1Path));
                Assert.IsTrue(mockFs.FileExists(expectedView2Path));
            });
        }

        private const string TestNamespace = "OrmLiteTestNamespace";
    }
}
