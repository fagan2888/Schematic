﻿using System.IO;
using McMaster.Extensions.CommandLineUtils;
using SJP.Schematic.Core.Caching;
using SJP.Schematic.DataAccess.Poco;
using System.IO.Abstractions;
using System;

namespace SJP.Schematic.Tool
{
    [Command(Description = "Generate POCO classes for basic mapping ORMs, e.g. Dapper.")]
    internal sealed class GeneratePocoCommand
    {
        private DatabaseCommand DatabaseParent { get; set; }

        private GenerateCommand GenerateParent { get; set; }

        private int OnExecute(CommandLineApplication application)
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            var hasConnectionString = DatabaseParent.TryGetConnectionString(out var connectionString);
            if (!hasConnectionString)
            {
                application.Error.WriteLine();
                application.Error.WriteLine("Unable to continue without a connection string. Exiting.");
                return 1;
            }

            var status = DatabaseParent.GetConnectionStatus(connectionString);
            if (!status.IsConnected)
            {
                application.Error.WriteLine("Could not connect to the database.");
                return 1;
            }

            var nameProvider = GenerateParent.GetNameTranslator();
            if (nameProvider == null)
            {
                application.Error.WriteLine("Unknown or unsupported database name translator: " + GenerateParent.Translator);
                return 1;
            }

            try
            {
                var cachedConnection = status.Connection.AsCachedConnection();
                var dialect = DatabaseParent.GetDatabaseDialect(cachedConnection);
                var database = dialect.GetRelationalDatabaseAsync().GetAwaiter().GetResult();
                var commentProvider = dialect.GetRelationalDatabaseCommentProviderAsync().GetAwaiter().GetResult();

                var fileSystem = new FileSystem();
                var generator = new PocoDataAccessGenerator(fileSystem, database, commentProvider, nameProvider);
                generator.Generate(GenerateParent.ProjectPath, GenerateParent.BaseNamespace);

                var dirName = Path.GetDirectoryName(GenerateParent.ProjectPath);
                application.Out.WriteLine("The OrmLite project has been exported to: " + dirName);
                return 0;
            }
            catch (Exception ex)
            {
                application.Error.WriteLine("An error occurred generating an OrmLite project.");
                application.Error.WriteLine();
                application.Error.WriteLine("Error message: " + ex.Message);
                application.Error.WriteLine("Stack trace: " + ex.StackTrace);

                return 1;
            }
            finally
            {
                status.Connection.Close();
            }
        }
    }
}
