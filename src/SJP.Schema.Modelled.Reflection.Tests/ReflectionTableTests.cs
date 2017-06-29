﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schema.Modelled.Reflection.Model;
using SJP.Schema.Modelled.Reflection.Tests.Fakes;
using SJP.Schema.Modelled.Reflection.Tests.Fakes.ColumnTypes;

namespace SJP.Schema.Modelled.Reflection.Tests
{
    [TestFixture]
    public class ReflectionTableTests
    {
        [Test]
        public void ReflectionDatabaseTThrowsArgumentExceptions()
        {
            Assert.Throws<ArgumentNullException>(() => new ReflectionRelationalDatabase<SampleDatabase>(null));
        }

        [Test]
        public void ReflectionDatabaseThrowsArgumentExceptions()
        {
            var dialect = FakeDialect.Instance;
            var dbType = typeof(SampleDatabase);
            Assert.Throws<ArgumentNullException>(() => new ReflectionRelationalDatabase(null, dbType));
            Assert.Throws<ArgumentNullException>(() => new ReflectionRelationalDatabase(dialect, null));
        }

        [Test]
        public void ReflectionDatabaseTestTableExists()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(FakeDialect.Instance);
            var tableExists = db.TableExists("TestTable1");
            Assert.IsTrue(tableExists);
        }

        [Test]
        public void ReflectionDatabaseReturnsTestTable()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(FakeDialect.Instance);
            var table = db.Table["TestTable1"];
            Assert.NotNull(table);
        }

        [Test]
        public async Task ReflectionDatabaseTestTableExistsAsync()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(FakeDialect.Instance);
            var tableExists = await db.TableExistsAsync("TestTable1");
            Assert.IsTrue(tableExists);
        }

        [Test]
        public async Task ReflectionDatabaseReturnsTestTableAsync()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(FakeDialect.Instance);
            var table = await db.TableAsync("TestTable1");
            Assert.NotNull(table);
        }

        private class SampleDatabase
        {
            public Table<TestTable1> FirstTestTable { get; }

            public class TestTable1
            {
                public Column<BigInteger> TEST_TABLE_ID { get; }

                public Column<Varchar200> TEST_STRING { get; }

                public Key PK_TEST_TABLE => new Key.Primary(TEST_TABLE_ID);
            }
        }
    }
}