﻿using System.Data;
using Microsoft.Extensions.Configuration;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite;

namespace SJP.Schematic.DataAccess.Poco.Tests
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

    internal abstract class SqliteTest
    {
        protected IDbConnection Connection { get; } = Config.Connection;

        protected IDatabaseDialect Dialect { get; } = new SqliteDialect();
    }
}