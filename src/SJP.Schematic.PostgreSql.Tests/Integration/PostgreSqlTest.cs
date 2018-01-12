﻿using System;
using System.Data;
using NUnit.Framework;
using SJP.Schematic.Core;
using Microsoft.Extensions.Configuration;

namespace SJP.Schematic.PostgreSql.Tests.Integration
{
    internal static class Config
    {
        public static IDbConnection Connection => new PostgreSqlDialect().CreateConnection(ConnectionString);

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("postgresql-test.json.config")
            .AddJsonFile("postgresql-test.json.config.local", optional: true)
            .Build();
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    internal sealed class DatabaseDependentAttribute : CategoryAttribute
    {
        public DatabaseDependentAttribute()
            : base("PostgreSqlDatabase")
        {
        }
    }

    [DatabaseDependent]
    internal abstract class PostgreSqlTest
    {
        protected IDbConnection Connection { get; } = Config.Connection;

        protected IDatabaseDialect Dialect { get; } = new PostgreSqlDialect();
    }
}