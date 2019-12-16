using System;
using System.Linq;
using NUnit.Framework;
using SJP.Schematic.DataAccess.Extensions;

namespace SJP.Schematic.DataAccess.Tests
{
    [TestFixture]
    internal static class EnumerableStringExtensionsTests
    {
        [Test]
        public static void OrderNamespaces_GivenNullCollection_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => EnumerableStringExtensions.OrderNamespaces(null));
        }

        [Test]
        public static void OrderNamespaces_GivenEmptyCollection_ReturnsEmptyCollection()
        {
            var input = Array.Empty<string>();
            var result = input.OrderNamespaces();

            Assert.IsFalse(result.Any());
        }

        [Test]
        public static void OrderNamespaces_GivenSystemInput_ReturnsOrderedCollection()
        {
            var input = new[]
            {
                "System.Data",
                "System.Collections.Generic",
                "System.Linq",
                "System"
            };
            var expected = new[]
            {
                "System",
                "System.Collections.Generic",
                "System.Data",
                "System.Linq"
            };

            var result = input.OrderNamespaces();

            var seqEqual = expected.SequenceEqual(result);
            Assert.IsTrue(seqEqual);
        }

        [Test]
        public static void OrderNamespaces_GivenNonSystemInput_ReturnsOrderedCollection()
        {
            var input = new[]
            {
                "Test.Data",
                "Test.Collections.Generic",
                "Test.Linq",
                "Test"
            };
            var expected = new[]
            {
                "Test",
                "Test.Collections.Generic",
                "Test.Data",
                "Test.Linq"
            };

            var result = input.OrderNamespaces();

            var seqEqual = expected.SequenceEqual(result);
            Assert.IsTrue(seqEqual);
        }

        [Test]
        public static void OrderNamespaces_GivenMixedInput_ReturnsOrderedCollection()
        {
            var input = new[]
            {
                "Test.Data",
                "System.Collections.Generic",
                "Test.Linq",
                "System"
            };
            var expected = new[]
            {
                "System",
                "System.Collections.Generic",
                "Test.Data",
                "Test.Linq"
            };

            var result = input.OrderNamespaces();

            var seqEqual = expected.SequenceEqual(result);
            Assert.IsTrue(seqEqual);
        }

        [Test]
        public static void OrderNamespaces_GivenOrderedInput_ReturnsOrderedCollection()
        {
            var input = new[]
            {
                "System",
                "System.Collections.Generic",
                "System.Data",
                "System.Linq"
            };

            var result = input.OrderNamespaces();

            var seqEqual = input.SequenceEqual(result);
            Assert.IsTrue(seqEqual);
        }
    }
}