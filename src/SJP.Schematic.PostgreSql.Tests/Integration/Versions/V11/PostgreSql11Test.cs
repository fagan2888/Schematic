﻿using System.Data;
using NUnit.Framework;
using SJP.Schematic.Core;
using Microsoft.Extensions.Configuration;

namespace SJP.Schematic.PostgreSql.Tests.Integration.Versions.V11
{
    internal static class Config11
    {
        public static IDbConnection Connection { get; } = PostgreSqlDialect.CreateConnectionAsync(ConnectionString).GetAwaiter().GetResult();

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("postgresql-test-11.json.config")
            .AddJsonFile("postgresql-test-11.json.config.local", optional: true)
            .Build();
    }

    [Category("PostgreSqlDatabase")]
    [Category("SkipWhenLiveUnitTesting")]
    [TestFixture(Ignore = "No v11 Postgres CI DB available")]
    internal abstract class PostgreSql11Test
    {
        protected IDbConnection Connection { get; } = Config11.Connection;

        protected IDatabaseDialect Dialect { get; } = new PostgreSqlDialect(Config11.Connection);

        protected IIdentifierDefaults IdentifierDefaults { get; } = new PostgreSqlDialect(Config11.Connection).GetIdentifierDefaultsAsync().GetAwaiter().GetResult();

        protected IIdentifierResolutionStrategy IdentifierResolver { get; } = new DefaultPostgreSqlIdentifierResolutionStrategy();
    }
}