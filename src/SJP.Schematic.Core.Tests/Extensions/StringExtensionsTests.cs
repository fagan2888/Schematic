﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests.Extensions
{
    [TestFixture]
    internal static class StringExtensionsTests
    {
        [Test]
        public static void IsNullOrEmpty_GivenNullInput_ReturnsTrue()
        {
            const string input = null;
            Assert.That(input.IsNullOrEmpty(), Is.True);
        }

        [Test]
        public static void IsNullOrEmpty_GivenEmptyInput_ReturnsTrue()
        {
            Assert.That(string.Empty.IsNullOrEmpty(), Is.True);
        }

        [Test]
        public static void IsNullOrEmpty_GivenNonEmptyInput_ReturnsFalse()
        {
            Assert.That("a".IsNullOrEmpty(), Is.False);
        }

        [Test]
        public static void IsNullOrWhiteSpace_GivenNullInput_ReturnsTrue()
        {
            const string input = null;
            Assert.That(input.IsNullOrWhiteSpace(), Is.True);
        }

        [Test]
        public static void IsNullOrWhiteSpace_GivenEmptyInput_ReturnsTrue()
        {
            Assert.That(string.Empty.IsNullOrWhiteSpace(), Is.True);
        }

        [Test]
        public static void IsNullOrWhiteSpace_GivenWhiteSpaceInput_ReturnsTrue()
        {
            Assert.That("   ".IsNullOrWhiteSpace(), Is.True);
        }

        [Test]
        public static void IsNullOrWhiteSpace_GivenInputContainingNonWhiteSpace_ReturnsFalse()
        {
            Assert.That("  a ".IsNullOrWhiteSpace(), Is.False);
        }

        [Test]
        public static void Join_GivenNullStringCollection_ThrowsArgumentNullException()
        {
            IEnumerable<string> values = null;
            Assert.That(() => values.Join(","), Throws.ArgumentNullException);
        }

        [Test]
        public static void Join_GivenNullSeparator_ThrowsArgumentNullException()
        {
            var values = Array.Empty<string>();
            Assert.That(() => values.Join(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void Join_GivenSingleString_ReturnsInput()
        {
            var values = new[] { "test" };
            var result = values.Join(",");

            Assert.That(result, Is.EqualTo(values[0]));
        }

        [Test]
        public static void Join_GivenManyStringsWithNonEmptySeparator_ReturnsStringSeparatedBySeparator()
        {
            const string expectedResult = "test1,test2,test3";
            var values = new[] { "test1", "test2", "test3" };
            var result = values.Join(",");

            Assert.That(expectedResult, Is.EqualTo(result));
        }

        [Test]
        public static void Join_GivenManyStringsWithEmptySeparator_ReturnsStringsConcatenated()
        {
            const string expectedResult = "test1test2test3";
            var values = new[] { "test1", "test2", "test3" };
            var result = values.Join(string.Empty);

            Assert.That(expectedResult, Is.EqualTo(result));
        }

        [Test]
        public static void Contains_WhenInvokedOnNullString_ThrowsArgumentNullException()
        {
            const string input = null;
            Assert.That(() => StringExtensions.Contains(input, string.Empty, StringComparison.OrdinalIgnoreCase), Throws.ArgumentNullException);
        }

        [Test]
        public static void Contains_GivenNullValue_ThrowsArgumentNullException()
        {
            var input = string.Empty;
            Assert.That(() => StringExtensions.Contains(input, null, StringComparison.OrdinalIgnoreCase), Throws.ArgumentNullException);
        }

        [Test]
        public static void Contains_GivenInvalidStringComparison_ThrowsArgumentException()
        {
            var input = string.Empty;
            const StringComparison comparison = (StringComparison)55;
            Assert.That(() => StringExtensions.Contains(input, string.Empty, comparison), Throws.ArgumentException);
        }

        [Test]
        public static void Contains_GivenEmptyStringWithNonEmptyValue_ReturnsFalse()
        {
            var input = string.Empty;
            Assert.That(StringExtensions.Contains(input, "A", StringComparison.Ordinal), Is.False);
        }

        [Test]
        public static void Contains_GivenNonEmptyStringWithEmptyValue_ReturnsTrue()
        {
            const string input = "A";
            Assert.That(StringExtensions.Contains(input, string.Empty, StringComparison.Ordinal), Is.True);
        }

        [Test]
        public static void Contains_GivenStringWithNonMatchingSubstring_ReturnsFalse()
        {
            const string input = "A";
            Assert.That(StringExtensions.Contains(input, "B", StringComparison.Ordinal), Is.False);
        }

        [Test]
        public static void Contains_GivenStringWithMatchingSubstring_ReturnsTrue()
        {
            const string input = "test input test";
            Assert.That(StringExtensions.Contains(input, "input", StringComparison.Ordinal), Is.True);
        }

        [Test]
        public static void Contains_GivenStringWithMatchingSubstringButIgnoringCase_ReturnsTrue()
        {
            const string input = "test input test";
            Assert.That(StringExtensions.Contains(input, "INPUT", StringComparison.OrdinalIgnoreCase), Is.True);
        }
    }
}
