﻿using System;
using NUnit.Framework;

namespace SJP.Schematic.Sqlite.Tests
{
    [TestFixture]
    internal class SqliteExpressionComparerTests
    {
        [Test]
        public void Ctor_GivenNullComparer_CreatesWithoutError()
        {
            var argComparer = StringComparer.Ordinal;
            var comparer = new SqliteExpressionComparer(sqlStringComparer: argComparer);
            Assert.Pass();
        }

        [Test]
        public void Ctor_GivenNullSqlStringComparer_CreatesWithoutError()
        {
            var argComparer = StringComparer.Ordinal;
            var comparer = new SqliteExpressionComparer(argComparer);
            Assert.Pass();
        }

        [Test]
        public void Ctor_GivenNoComparers_CreatesWithoutError()
        {
            var comparer = new SqliteExpressionComparer();
            Assert.Pass();
        }

        [Test]
        public void Equals_GivenEqualSqlStringArguments_ReturnsTrue()
        {
            const string input = "'abc'";
            var comparer = new SqliteExpressionComparer();

            var equals = comparer.Equals(input, input);

            Assert.IsTrue(equals);
        }

        [Test]
        public void Equals_GivenDifferentSqlStringArguments_ReturnsFalse()
        {
            const string inputX = "'abc'";
            const string inputY = "'zxc'";
            var comparer = new SqliteExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.IsFalse(equals);
        }

        [Test]
        public void Equals_GivenEqualDateArguments_ReturnsTrue()
        {
            const string input = "getdate()";
            var comparer = new SqliteExpressionComparer();

            var equals = comparer.Equals(input, input);

            Assert.IsTrue(equals);
        }

        [Test]
        public void Equals_GivenDifferentDateArguments_ReturnsFalse()
        {
            const string inputX = "getdate()";
            const string inputY = "getutcdate()";
            var comparer = new SqliteExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.IsFalse(equals);
        }

        [Test]
        public void Equals_GivenEqualNumberArguments_ReturnsTrue()
        {
            const string input = "123";
            var comparer = new SqliteExpressionComparer();

            var equals = comparer.Equals(input, input);

            Assert.IsTrue(equals);
        }

        [Test]
        public void Equals_GivenDifferentNumberArguments_ReturnsFalse()
        {
            const string inputX = "123";
            const string inputY = "456";
            var comparer = new SqliteExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.IsFalse(equals);
        }

        [Test]
        public void Equals_GivenEqualComplexExpressions_ReturnsTrue()
        {
            const string input = "[test_column_1] > len(left([abc], 50))";
            var comparer = new SqliteExpressionComparer();

            var equals = comparer.Equals(input, input);

            Assert.IsTrue(equals);
        }

        [Test]
        public void Equals_GivenDifferentComplexExpressions_ReturnsFalse()
        {
            const string inputX = "[test_column_1] > len(left([abc], 50))";
            const string inputY = "[test_column_2] < len(left([abc], 50))";
            var comparer = new SqliteExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.IsFalse(equals);
        }

        [Test]
        public void Equals_GivenDefaultTextComparerAndEqualComplexExpressionsButDifferentCase_ReturnsFalse()
        {
            const string inputX = "([test_column_1] > len(left([abc], (50))))";
            const string inputY = "([TEST_Column_1] > len(left([abc], (50))))";
            var comparer = new SqliteExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.IsFalse(equals);
        }

        [Test]
        public void Equals_GivenIgnoreCaseTextComparerAndEqualComplexExpressionsButDifferentCase_ReturnsTrue()
        {
            const string inputX = "([test_column_1] > len(left([abc], (50))))";
            const string inputY = "([TEST_Column_1] > len(left([abc], (50))))";
            var comparer = new SqliteExpressionComparer(StringComparer.OrdinalIgnoreCase);

            var equals = comparer.Equals(inputX, inputY);

            Assert.IsTrue(equals);
        }

        [Test]
        public void Equals_GivenDefaultTextComparerAndEqualComplexExpressionsButDifferentStringCase_ReturnsFalse()
        {
            const string inputX = "([test_column_1] > len(left('asd', (50))))";
            const string inputY = "([test_column_1] > len(left('ASD', (50))))";
            var comparer = new SqliteExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.IsFalse(equals);
        }

        [Test]
        public void Equals_GivenIgnoreCaseTextComparerAndEqualComplexExpressionsButDifferentStringCase_ReturnsTrue()
        {
            const string inputX = "([test_column_1] > len(left('asd', (50))))";
            const string inputY = "([test_column_1] > len(left('ASD', (50))))";
            var comparer = new SqliteExpressionComparer(sqlStringComparer: StringComparer.OrdinalIgnoreCase);

            var equals = comparer.Equals(inputX, inputY);

            Assert.IsTrue(equals);
        }
    }
}