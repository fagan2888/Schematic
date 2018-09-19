﻿using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle.Tests
{
    [TestFixture]
    internal static class OracleCheckConstraintTests
    {
        [Test]
        public static void Ctor_GivenNullTable_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OracleCheckConstraint(null, "test_check", "test_check", true));
        }

        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Assert.Throws<ArgumentNullException>(() => new OracleCheckConstraint(table, null, "test_check", true));
        }

        [Test]
        public static void Ctor_GivenNullDefinition_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Assert.Throws<ArgumentNullException>(() => new OracleCheckConstraint(table, "test_check", null, true));
        }

        [Test]
        public static void Ctor_GivenEmptyDefinition_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Assert.Throws<ArgumentNullException>(() => new OracleCheckConstraint(table, "test_check", string.Empty, true));
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceDefinition_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Assert.Throws<ArgumentNullException>(() => new OracleCheckConstraint(table, "test_check", "      ", true));
        }

        [Test]
        public static void Table_PropertyGet_EqualsCtorArg()
        {
            Identifier tableName = "test_table";
            var table = new Mock<IRelationalDatabaseTable>();
            table.Setup(t => t.Name).Returns(tableName);
            var tableArg = table.Object;

            var check = new OracleCheckConstraint(tableArg, "test_check", "test_check", true);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(tableName, check.Table.Name);
                Assert.AreSame(tableArg, check.Table);
            });
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            Identifier checkName = "test_check";
            var table = Mock.Of<IRelationalDatabaseTable>();
            var check = new OracleCheckConstraint(table, checkName, "test_check", true);

            Assert.AreEqual(checkName, check.Name);
        }

        [Test]
        public static void Definition_PropertyGet_EqualsCtorArg()
        {
            const string checkDefinition = "test_check_definition";
            var table = Mock.Of<IRelationalDatabaseTable>();
            var check = new OracleCheckConstraint(table, "test_check", checkDefinition, true);

            Assert.AreEqual(checkDefinition, check.Definition);
        }

        [Test]
        public static void IsEnabled_PropertyGetWhenCtorGivenTrue_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            var check = new OracleCheckConstraint(table, "test_check", "test_check_definition", true);

            Assert.IsTrue(check.IsEnabled);
        }

        [Test]
        public static void IsEnabled_PropertyGetWhenCtorGivenFalse_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            var check = new OracleCheckConstraint(table, "test_check", "test_check_definition", false);

            Assert.IsFalse(check.IsEnabled);
        }
    }
}