﻿using System.Data;
using LanguageExt;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Tests;

namespace SJP.Schematic.SqlServer.Tests.Integration.Versions.V2012
{
    internal static class Config2012
    {
        public static IDbConnection Connection { get; } = Prelude.Try(() => !ConnectionString.IsNullOrWhiteSpace()
            ? SqlServerDialect.CreateConnectionAsync(ConnectionString).GetAwaiter().GetResult()
            : null)
            .Match(c => c, _ => null);

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("sqlserver-test-2012.config.json")
            .AddJsonFile("sqlserver-test-2012.local.config.json", optional: true)
            .Build();
    }

    [Category("SqlServerDatabase")]
    [Category("SkipWhenLiveUnitTesting")]
    [DatabaseTestFixture(typeof(Config2012), nameof(Config2012.Connection), "No SQL Server 2012 DB available")]
    internal abstract class SqlServer2012Test
    {
        protected IDbConnection Connection { get; } = Config2012.Connection;

        protected ISqlServerDialect Dialect { get; } = new SqlServerDialect(Config2012.Connection);

        protected IIdentifierDefaults IdentifierDefaults { get; } = new SqlServerDialect(Config2012.Connection).GetIdentifierDefaultsAsync().GetAwaiter().GetResult();
    }
}
