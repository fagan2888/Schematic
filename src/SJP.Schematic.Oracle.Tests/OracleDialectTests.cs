﻿using System;
using NUnit.Framework;

namespace SJP.Schematic.Oracle.Tests
{
    [TestFixture]
    internal static class OracleDialectTests
    {
        [Test]
        public static void CreateConnection_GivenNull_ThrowsArgumentNullException()
        {
            var dialect = new OracleDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.CreateConnection(null));
        }

        [Test]
        public static void CreateConnection_GivenEmptyString_ThrowsArgumentNullException()
        {
            var dialect = new OracleDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.CreateConnection(string.Empty));
        }

        [Test]
        public static void CreateConnection_GivenWhiteSpaceString_ThrowsArgumentNullException()
        {
            var dialect = new OracleDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.CreateConnection("   "));
        }

        [Test]
        public static void CreateConnectionAsync_GivenNull_ThrowsArgumentNullException()
        {
            var dialect = new OracleDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.CreateConnectionAsync(null));
        }

        [Test]
        public static void CreateConnectionAsync_GivenEmptyString_ThrowsArgumentNullException()
        {
            var dialect = new OracleDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.CreateConnectionAsync(string.Empty));
        }

        [Test]
        public static void CreateConnectionAsync_GivenWhiteSpaceString_ThrowsArgumentNullException()
        {
            var dialect = new OracleDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.CreateConnectionAsync("   "));
        }

        [Test]
        public static void QuoteIdentifier_GivenNull_ThrowsArgumentNullException()
        {
            var dialect = new OracleDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteIdentifier(null));
        }

        [Test]
        public static void QuoteIdentifier_GivenEmptyString_ThrowsArgumentNullException()
        {
            var dialect = new OracleDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteIdentifier(string.Empty));
        }

        [Test]
        public static void QuoteIdentifier_GivenWhiteSpace_ThrowsArgumentNullException()
        {
            var dialect = new OracleDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteIdentifier("    "));
        }

        [Test]
        public static void QuoteName_GivenNull_ThrowsArgumentNullException()
        {
            var dialect = new OracleDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteName(null));
        }

        [Test]
        public static void QuoteName_GivenEmptyString_ThrowsArgumentNullException()
        {
            var dialect = new OracleDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteName(string.Empty));
        }

        [Test]
        public static void QuoteName_GivenWhiteSpace_ThrowsArgumentNullException()
        {
            var dialect = new OracleDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteName("    "));
        }

        [Test]
        public static void QuoteIdentifier_GivenRegularLocalName_ReturnsQuotedIdentifier()
        {
            const string input = "test_table";
            const string expected = "\"test_table\"";

            var dialect = new OracleDialect();
            var result = dialect.QuoteIdentifier(input);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void QuoteIdentifier_GivenNameWithWhitespace_ReturnsQuotedIdentifier()
        {
            const string input = "test table name";
            const string expected = "\"test table name\"";

            var dialect = new OracleDialect();
            var result = dialect.QuoteIdentifier(input);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void QuoteIdentifier_GivenNameWithDotSeparators_ReturnsQuotedIdentifier()
        {
            const string input = "test.table.name";
            const string expected = "\"test.table.name\"";

            var dialect = new OracleDialect();
            var result = dialect.QuoteIdentifier(input);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void QuoteIdentifier_GivenNameWithDoubleQuote_ThrowsArgumentException()
        {
            const string input = "\"test_table";

            var dialect = new OracleDialect();
            Assert.Throws<ArgumentException>(() => dialect.QuoteIdentifier(input));
        }

        [Test]
        public static void QuoteIdentifier_GivenNameNullCharacter_ThrowsArgumentException()
        {
            var input = new string(new[] { 't', 'e', '\0', 's', 't' });

            var dialect = new OracleDialect();
            Assert.Throws<ArgumentException>(() => dialect.QuoteIdentifier(input));
        }
    }
}