﻿using System;
using McMaster.Extensions.CommandLineUtils;

namespace SJP.Schematic.Tool
{
    [Command(Name = "lint", Description = "Provides a lint report for potential schema issues.")]
    internal class LintCommand
    {
        private DatabaseCommand Parent { get; set; }

        private int OnExecute(CommandLineApplication application)
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            var dialect = Parent.GetDatabaseDialect();
            var hasConnectionString = Parent.TryGetConnectionString(out var connectionString);
            if (!hasConnectionString)
            {
                application.Error.WriteLine();
                application.Error.WriteLine("Unable to continue without a connection string. Exiting.");
                return 1;
            }

            var status = DatabaseCommand.GetConnectionStatus(dialect, connectionString);
            if (status.IsConnected)
            {
                application.Out.WriteLine("Successfully connected to the database.");
                return 0;
            }

            application.ShowHelp();
            return 1;
        }
    }
}