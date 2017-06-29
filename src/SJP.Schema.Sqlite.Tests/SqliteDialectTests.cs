﻿using System;
using NUnit.Framework;

namespace SJP.Schema.Sqlite.Tests
{
    [TestFixture]
    public class SqliteDialectTests
    {
        [Test]
        public void QuotingIdentifierThrowsOnNullInput()
        {
            var dialect = SqliteDialect.Instance;
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteIdentifier(null));
        }

        [Test]
        public void QuotingIdentifierThrowsOnEmptyInput()
        {
            var dialect = SqliteDialect.Instance;
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteIdentifier(string.Empty));
        }

        [Test]
        public void QuotingIdentifierThrowsOnWhiteSpaceInput()
        {
            var dialect = SqliteDialect.Instance;
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteIdentifier("    "));
        }

        [Test]
        public void QuotingNameThrowsOnNullInput()
        {
            var dialect = SqliteDialect.Instance;
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteName(null));
        }

        [Test]
        public void QuotingNameThrowsOnEmptyInput()
        {
            var dialect = SqliteDialect.Instance;
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteName(string.Empty));
        }

        [Test]
        public void QuotingNameThrowsOnWhiteSpaceInput()
        {
            var dialect = SqliteDialect.Instance;
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteName("    "));
        }
    }
}