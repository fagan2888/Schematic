﻿using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle.Tests.Integration
{
    internal sealed class OracleDatabaseSequenceTests : OracleTest
    {
        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create sequence db_test_sequence_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("create sequence db_test_sequence_2 start with 20").ConfigureAwait(false);
            await Connection.ExecuteAsync("create sequence db_test_sequence_3 start with 100 increment by 100").ConfigureAwait(false);
            await Connection.ExecuteAsync("create sequence db_test_sequence_4 start with 1000 minvalue -99").ConfigureAwait(false);
            await Connection.ExecuteAsync("create sequence db_test_sequence_5 start with 1000 nominvalue").ConfigureAwait(false);
            await Connection.ExecuteAsync("create sequence db_test_sequence_6 start with 1 maxvalue 333").ConfigureAwait(false);
            await Connection.ExecuteAsync("create sequence db_test_sequence_7 start with 1 nomaxvalue").ConfigureAwait(false);
            await Connection.ExecuteAsync("create sequence db_test_sequence_8 cycle maxvalue 1000").ConfigureAwait(false);
            await Connection.ExecuteAsync("create sequence db_test_sequence_9 nocycle").ConfigureAwait(false);
            await Connection.ExecuteAsync("create sequence db_test_sequence_10 cache 10").ConfigureAwait(false);
            await Connection.ExecuteAsync("create sequence db_test_sequence_11 nocache").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop sequence db_test_sequence_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop sequence db_test_sequence_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop sequence db_test_sequence_3").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop sequence db_test_sequence_4").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop sequence db_test_sequence_5").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop sequence db_test_sequence_6").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop sequence db_test_sequence_7").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop sequence db_test_sequence_8").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop sequence db_test_sequence_9").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop sequence db_test_sequence_10").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop sequence db_test_sequence_11").ConfigureAwait(false);
        }

        private const int SequenceDefaultCache = 20;
        private const int SequenceDefaultMinValue = 1;
        private const decimal OracleNumberMaxValue = 9999999999999999999999999999m;

        private IRelationalDatabase Database => new OracleRelationalDatabase(Dialect, Connection);

        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseSequence(null, database, "test"));
        }

        [Test]
        public static void Ctor_GivenNullDatabase_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseSequence(connection, null, "test"));
        }

        [Test]
        public static void Ctor_GivenNullName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = Mock.Of<IRelationalDatabase>();
            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseSequence(connection, database, null));
        }

        [Test]
        public void Database_PropertyGet_ShouldMatchCtorArg()
        {
            var database = Database;
            var sequence = new OracleDatabaseSequence(Connection, database, "test");

            Assert.AreSame(database, sequence.Database);
        }

        [Test]
        public void Name_GivenLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var database = Database;
            var sequenceName = new Identifier("SEQUENCE_TEST_SEQUENCE_1");
            var expectedSequenceName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "SEQUENCE_TEST_SEQUENCE_1");

            var sequence = new OracleDatabaseSequence(Connection, database, sequenceName);

            Assert.AreEqual(expectedSequenceName, sequence.Name);
        }

        [Test]
        public void Name_GivenSchemaAndLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var database = Database;
            var sequenceName = new Identifier("ASD", "SEQUENCE_TEST_SEQUENCE_1");
            var expectedSequenceName = new Identifier(database.ServerName, database.DatabaseName, "ASD", "SEQUENCE_TEST_SEQUENCE_1");

            var sequence = new OracleDatabaseSequence(Connection, database, sequenceName);

            Assert.AreEqual(expectedSequenceName, sequence.Name);
        }

        [Test]
        public void Name_GivenDatabaseAndSchemaAndLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var database = Database;
            var sequenceName = new Identifier("QWE", "ASD", "SEQUENCE_TEST_SEQUENCE_1");
            var expectedSequenceName = new Identifier(database.ServerName, "QWE", "ASD", "SEQUENCE_TEST_SEQUENCE_1");

            var sequence = new OracleDatabaseSequence(Connection, database, sequenceName);

            Assert.AreEqual(expectedSequenceName, sequence.Name);
        }

        [Test]
        public void Name_GivenFullyQualifiedNameInCtor_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier("QWE", "ASD", "ZXC", "SEQUENCE_TEST_SEQUENCE_1");
            var expectedSequenceName = new Identifier("QWE", "ASD", "ZXC", "SEQUENCE_TEST_SEQUENCE_1");

            var sequence = new OracleDatabaseSequence(Connection, Database, sequenceName);

            Assert.AreEqual(expectedSequenceName, sequence.Name);
        }

        [Test]
        public void Start_GivenDefaultSequence_ReturnsOne()
        {
            var sequence = Database.GetSequence("DB_TEST_SEQUENCE_1");

            Assert.AreEqual(1, sequence.Start);
        }

        [Test]
        public void Start_GivenSequenceWithCustomStart_ReturnsCorrectValue()
        {
            var sequence = Database.GetSequence("DB_TEST_SEQUENCE_2");

            Assert.AreEqual(1, sequence.Start);
        }

        [Test]
        public void Increment_GivenDefaultSequence_ReturnsOne()
        {
            var sequence = Database.GetSequence("DB_TEST_SEQUENCE_1");

            Assert.AreEqual(1, sequence.Increment);
        }

        [Test]
        public void Increment_GivenSequenceWithCustomIncrement_ReturnsCorrectValue()
        {
            var sequence = Database.GetSequence("DB_TEST_SEQUENCE_3");

            Assert.AreEqual(100, sequence.Increment);
        }

        [Test]
        public void MinValue_GivenDefaultSequence_ReturnsOne()
        {
            var sequence = Database.GetSequence("DB_TEST_SEQUENCE_1");

            Assert.AreEqual(SequenceDefaultMinValue, sequence.MinValue);
        }

        [Test]
        public void MinValue_GivenSequenceWithCustomMinValue_ReturnsCorrectValue()
        {
            var sequence = Database.GetSequence("DB_TEST_SEQUENCE_4");

            Assert.AreEqual(-99, sequence.MinValue);
        }

        [Test]
        public void MinValue_GivenAscendingSequenceWithNoMinValue_ReturnsOne()
        {
            var sequence = Database.GetSequence("DB_TEST_SEQUENCE_5");

            Assert.AreEqual(1, sequence.MinValue);
        }

        [Test]
        public void MaxValue_GivenDefaultSequence_ReturnsOracleNumberMaxValue()
        {
            var sequence = Database.GetSequence("DB_TEST_SEQUENCE_1");

            Assert.AreEqual(OracleNumberMaxValue, sequence.MaxValue);
        }

        [Test]
        public void MaxValue_GivenSequenceWithCustomMaxValue_ReturnsCorrectValue()
        {
            var sequence = Database.GetSequence("DB_TEST_SEQUENCE_6");

            Assert.AreEqual(333, sequence.MaxValue);
        }

        [Test]
        public void MaxValue_GivenSequenceWithNoMaxValue_ReturnsOracleNumberMaxValue()
        {
            var sequence = Database.GetSequence("DB_TEST_SEQUENCE_7");

            Assert.AreEqual(OracleNumberMaxValue, sequence.MaxValue);
        }

        [Test]
        public void Cycle_GivenDefaultSequence_ReturnsTrue()
        {
            var sequence = Database.GetSequence("DB_TEST_SEQUENCE_1");

            Assert.IsFalse(sequence.Cycle);
        }

        [Test]
        public void Cycle_GivenSequenceWithCycle_ReturnsTrue()
        {
            var sequence = Database.GetSequence("DB_TEST_SEQUENCE_8");

            Assert.IsTrue(sequence.Cycle);
        }

        [Test]
        public void Cycle_GivenSequenceWithNoCycle_ReturnsTrue()
        {
            var sequence = Database.GetSequence("DB_TEST_SEQUENCE_9");

            Assert.IsFalse(sequence.Cycle);
        }

        [Test]
        public void Cache_GivenDefaultSequence_ReturnsDefaultCacheSize()
        {
            var sequence = Database.GetSequence("DB_TEST_SEQUENCE_1");

            Assert.AreEqual(SequenceDefaultCache, sequence.Cache);
        }

        [Test]
        public void Cache_GivenSequenceWithCacheSet_ReturnsCorrectValue()
        {
            var sequence = Database.GetSequence("DB_TEST_SEQUENCE_10");

            Assert.AreEqual(10, sequence.Cache);
        }

        [Test]
        public void Cache_GivenSequenceWithNoCacheSet_ReturnsCorrectValue()
        {
            var sequence = Database.GetSequence("DB_TEST_SEQUENCE_11");

            Assert.AreEqual(0, sequence.Cache);
        }
    }
}