﻿using System;
using System.IO.Abstractions;
using System.Threading;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.DataAccess.OrmLite
{
    public class OrmLiteDataAccessGenerator : IDataAccessGenerator
    {
        public OrmLiteDataAccessGenerator(IRelationalDatabase database, INameTranslator nameTranslator, string indent = "    ")
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
            NameTranslator = nameTranslator ?? throw new ArgumentNullException(nameof(nameTranslator));
            Indent = indent ?? throw new ArgumentNullException(nameof(indent));
        }

        protected IRelationalDatabase Database { get; }

        protected INameTranslator NameTranslator { get; }

        protected string Indent { get; }

        public void Generate(IFileSystem fileSystem, string projectPath, string baseNamespace)
        {
            if (fileSystem == null)
                throw new ArgumentNullException(nameof(fileSystem));
            if (projectPath.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(projectPath));
            if (baseNamespace.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(baseNamespace));

            var projectFileInfo = fileSystem.FileInfo.FromFileName(projectPath);
            if (!string.Equals(projectFileInfo.Extension, ".csproj", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("The given path to a project must be a csproj file.", nameof(projectPath));

            if (projectFileInfo.Exists)
                projectFileInfo.Delete();

            if (!projectFileInfo.Directory.Exists)
                projectFileInfo.Directory.Create();

            fileSystem.File.WriteAllText(projectPath, ProjectDefinition);

            var tableGenerator = new OrmLiteTableGenerator(NameTranslator, baseNamespace);
            var viewGenerator = new OrmLiteViewGenerator(NameTranslator, baseNamespace);

            var tables = Database.GetAllTables(CancellationToken.None).GetAwaiter().GetResult();
            foreach (var table in tables)
            {
                var tableClass = tableGenerator.Generate(table);
                var tablePath = tableGenerator.GetFilePath(projectFileInfo.Directory, table.Name);

                if (!tablePath.Directory.Exists)
                    tablePath.Directory.Create();

                if (tablePath.Exists)
                    tablePath.Delete();

                fileSystem.File.WriteAllText(tablePath.FullName, tableClass);
            }

            var views = Database.GetAllViews(CancellationToken.None).GetAwaiter().GetResult();
            foreach (var view in views)
            {
                var viewClass = viewGenerator.Generate(view);
                var viewPath = viewGenerator.GetFilePath(projectFileInfo.Directory, view.Name);

                if (!viewPath.Directory.Exists)
                    viewPath.Directory.Create();

                if (viewPath.Exists)
                    viewPath.Delete();

                fileSystem.File.WriteAllText(viewPath.FullName, viewClass);
            }
        }

        protected const string ProjectDefinition = @"<Project Sdk=""Microsoft.NET.Sdk"">
    <PropertyGroup>
        <TargetFramework>netstandard2.0</TargetFramework>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include=""ServiceStack.OrmLite"" Version=""5.4.0"" />
    </ItemGroup>
</Project>";
    }
}
