﻿using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
using System.Data;
using SJP.Schematic.SqlServer.Comments;

namespace SJP.Schematic.SqlServer.Tests.Comments
{
    [TestFixture]
    internal static class SqlServerTableCommentProviderTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.That(() => new SqlServerTableCommentProvider(null, identifierDefaults), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.That(() => new SqlServerTableCommentProvider(connection, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetTableComments_GivenNullTableName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var commentProvider = new SqlServerTableCommentProvider(connection, identifierDefaults);

            Assert.That(() => commentProvider.GetTableComments(null), Throws.ArgumentNullException);
        }
    }
}
