﻿using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
using System.Data;

namespace SJP.Schematic.Oracle.Tests
{
    [TestFixture]
    internal static class OracleDatabaseMaterializedViewProviderTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            Assert.That(() => new OracleDatabaseMaterializedViewProvider(null, identifierDefaults, identifierResolver, typeProvider), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            Assert.That(() => new OracleDatabaseMaterializedViewProvider(connection, null, identifierResolver, typeProvider), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierResolver_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            Assert.That(() => new OracleDatabaseMaterializedViewProvider(connection, identifierDefaults, null, typeProvider), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullTypeProvider_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();

            Assert.That(() => new OracleDatabaseMaterializedViewProvider(connection, identifierDefaults, identifierResolver, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetView_GivenNullViewName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            var viewProvider = new OracleDatabaseMaterializedViewProvider(connection, identifierDefaults, identifierResolver, typeProvider);

            Assert.That(() => viewProvider.GetView(null), Throws.ArgumentNullException);
        }
    }
}
