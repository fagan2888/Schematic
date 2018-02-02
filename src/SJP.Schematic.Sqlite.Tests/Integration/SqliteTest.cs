﻿using System.Data;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite.Tests.Integration
{
    internal static class Config
    {
        public static IDbConnection Connection => new SqliteDialect().CreateConnection(ConnectionString);

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("sqlite-test.json.config")
            .AddJsonFile("sqlite-test.json.config.local", optional: true)
            .Build();
    }

    [Category("SqliteDatabase")]
    [Category("SkipWhenLiveUnitTesting")]
    internal abstract class SqliteTest
    {
        protected IDbConnection Connection { get; } = Config.Connection;

        protected IDatabaseDialect Dialect { get; } = new SqliteDialect();
    }
}
