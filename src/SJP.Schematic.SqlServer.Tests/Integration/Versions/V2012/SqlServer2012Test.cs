﻿using System.Data;
using NUnit.Framework;
using SJP.Schematic.Core;
using Microsoft.Extensions.Configuration;

namespace SJP.Schematic.SqlServer.Tests.Integration.Versions.V2012
{
    internal static class Config2012
    {
        public static IDbConnection Connection { get; } = SqlServerDialect.CreateConnectionAsync(ConnectionString).GetAwaiter().GetResult();

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("sqlserver-test-2012.json.config")
            .AddJsonFile("sqlserver-test-2012.json.config.local", optional: true)
            .Build();
    }

    [Category("SqlServerDatabase")]
    [Category("SkipWhenLiveUnitTesting")]
    [TestFixture(Ignore = "No CI 2012 DB available")]
    internal abstract class SqlServer2012Test
    {
        protected IDbConnection Connection { get; } = Config2012.Connection;

        protected ISqlServerDialect Dialect { get; } = new SqlServerDialect(Config2012.Connection);

        protected IIdentifierDefaults IdentifierDefaults { get; } = new SqlServerDialect(Config2012.Connection).GetIdentifierDefaultsAsync().GetAwaiter().GetResult();
    }
}