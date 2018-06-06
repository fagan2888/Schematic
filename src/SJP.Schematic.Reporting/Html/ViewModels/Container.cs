﻿using System.Diagnostics;
using System.Reflection;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels
{
    internal class Container : ITemplateParameter
    {
        public Container(
            string content,
            string databaseName,
            string rootPath
        )
        {
            Content = content ?? string.Empty;
            DatabaseName = !databaseName.IsNullOrWhiteSpace()
                ? databaseName + " Database"
                : "Database";
            RootPath = rootPath ?? string.Empty;
        }

        public ReportTemplate Template { get; } = ReportTemplate.Container;

        public string RootPath { get; }

        public string DatabaseName { get; }

        public string Content { get; }

        public string ProjectVersion => _projectVersion;

        private readonly static string _projectVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
    }
}