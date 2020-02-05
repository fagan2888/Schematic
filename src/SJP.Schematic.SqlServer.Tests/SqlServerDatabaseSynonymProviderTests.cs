﻿using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
using System.Data;

namespace SJP.Schematic.SqlServer.Tests
{
    [TestFixture]
    internal static class SqlServerRelationalDatabaseSynonymProviderTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.That(() => new SqlServerDatabaseSynonymProvider(null, identifierDefaults), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.That(() => new SqlServerDatabaseSynonymProvider(connection, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetSynonym_GivenNullSynonymName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var synonymProvider = new SqlServerDatabaseSynonymProvider(connection, identifierDefaults);

            Assert.That(() => synonymProvider.GetSynonym(null), Throws.ArgumentNullException);
        }
    }
}
