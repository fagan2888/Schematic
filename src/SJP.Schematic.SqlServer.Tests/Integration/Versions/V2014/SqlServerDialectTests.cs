﻿using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Tests.Integration.Versions.V2014
{
    internal sealed class SqlServerDialectTests : SqlServer2014Test
    {
        [Test]
        public async Task GetDatabaseDisplayVersionAsync_WhenInvoked_ReturnsNonEmptyString()
        {
            var versionStr = await Dialect.GetDatabaseDisplayVersionAsync(Connection).ConfigureAwait(false);
            var validStr = !versionStr.IsNullOrWhiteSpace();

            Assert.That(validStr, Is.True);
        }

        [Test]
        public async Task GetDatabaseVersionAsync_WhenInvoked_ReturnsNonNullVersion()
        {
            var version = await Dialect.GetDatabaseVersionAsync(Connection).ConfigureAwait(false);

            Assert.That(version, Is.Not.Null);
        }

        [Test]
        public async Task GetServerProperties2008_WhenInvoked_ReturnsNonNullObject()
        {
            var properties = await Dialect.GetServerProperties2008(DbConnection).ConfigureAwait(false);

            Assert.That(properties, Is.Not.Null);
        }

        [Test]
        public async Task GetServerProperties2012_WhenInvoked_ReturnsNonNullObject()
        {
            var properties = await Dialect.GetServerProperties2012(DbConnection).ConfigureAwait(false);

            Assert.That(properties, Is.Not.Null);
        }

        [Test]
        public async Task GetServerProperties2014_WhenInvoked_ReturnsNonNullObject()
        {
            var properties = await Dialect.GetServerProperties2014(DbConnection).ConfigureAwait(false);

            Assert.That(properties, Is.Not.Null);
        }
    }
}
