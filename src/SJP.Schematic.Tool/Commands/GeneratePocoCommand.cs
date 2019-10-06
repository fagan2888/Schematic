﻿using System.IO;
using System.IO.Abstractions;
using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.DataAccess.Poco;

namespace SJP.Schematic.Tool
{
    [Command(Description = "Generate POCO classes for basic mapping ORMs, e.g. Dapper.")]
    internal sealed class GeneratePocoCommand
    {
        private DatabaseCommand DatabaseParent { get; set; }

        private GenerateCommand GenerateParent { get; set; }

        private Task<int> OnExecuteAsync(CommandLineApplication application)
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            return OnExecuteAsyncCore(application);
        }

        private async Task<int> OnExecuteAsyncCore(CommandLineApplication application)
        {
            var connectionString = await DatabaseParent.TryGetConnectionStringAsync().ConfigureAwait(false);
            if (connectionString.IsNullOrWhiteSpace())
            {
                await application.Error.WriteLineAsync().ConfigureAwait(false);
                await application.Error.WriteLineAsync("Unable to continue without a connection string. Exiting.").ConfigureAwait(false);
                return 1;
            }

            var status = await DatabaseParent.GetConnectionStatusAsync(connectionString).ConfigureAwait(false);
            if (!status.IsConnected)
            {
                await application.Error.WriteLineAsync("Could not connect to the database.").ConfigureAwait(false);
                return 1;
            }

            var nameProvider = GenerateParent.GetNameTranslator();
            if (nameProvider == null)
            {
                await application.Error.WriteLineAsync("Unknown or unsupported database name translator: " + GenerateParent.Translator).ConfigureAwait(false);
                return 1;
            }

            try
            {
                var dialect = DatabaseParent.GetDatabaseDialect(status.Connection);
                var database = await dialect.GetRelationalDatabaseAsync().ConfigureAwait(false);
                var commentProvider = await dialect.GetRelationalDatabaseCommentProviderAsync().ConfigureAwait(false);

                var fileSystem = new FileSystem();
                var generator = new PocoDataAccessGenerator(fileSystem, database, commentProvider, nameProvider);
                await generator.Generate(GenerateParent.ProjectPath, GenerateParent.BaseNamespace).ConfigureAwait(false);

                var dirName = Path.GetDirectoryName(GenerateParent.ProjectPath);
                application.Out.WriteLine("The POCO project has been exported to: " + dirName);
                return 0;
            }
            catch (Exception ex)
            {
                await application.Error.WriteLineAsync("An error occurred generating an OrmLite project.").ConfigureAwait(false);
                await application.Error.WriteLineAsync().ConfigureAwait(false);
                await application.Error.WriteLineAsync("Error message: " + ex.Message).ConfigureAwait(false);
                await application.Error.WriteLineAsync("Stack trace: " + ex.StackTrace).ConfigureAwait(false);

                return 1;
            }
            finally
            {
                status.Connection.Close();
            }
        }
    }
}
