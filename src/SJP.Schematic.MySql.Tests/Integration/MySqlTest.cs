﻿using System.Data;
using NUnit.Framework;
using SJP.Schematic.Core;
using Microsoft.Extensions.Configuration;

namespace SJP.Schematic.MySql.Tests.Integration
{
    internal static class Config
    {
        public static IDbConnection Connection { get; } = MySqlDialect.CreateConnectionAsync(ConnectionString).GetAwaiter().GetResult();

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("mysql-test.config.json")
            .AddJsonFile("mysql-test.local.config.json", optional: true)
            .Build();
    }

    [Category("MySqlDatabase")]
    [Category("SkipWhenLiveUnitTesting")]
    [TestFixture]
    internal abstract class MySqlTest
    {
        protected IDbConnection Connection { get; } = Config.Connection;

        protected IDatabaseDialect Dialect { get; } = new MySqlDialect(Config.Connection);

        protected IIdentifierDefaults IdentifierDefaults { get; } = new MySqlDialect(Config.Connection).GetIdentifierDefaultsAsync().GetAwaiter().GetResult();
    }
}
