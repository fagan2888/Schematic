﻿using System;
using System.Security;
using System.Text;
using Humanizer;
using SJP.Schematic.Core;
using SJP.Schematic.DataAccess.Extensions;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore
{
    public class DbContextBuilder
    {
        public DbContextBuilder(INameProvider nameProvider, string baseNamespace, IRelationalDatabase database)
        {
            if (baseNamespace.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(baseNamespace));

            NameProvider = nameProvider ?? throw new ArgumentNullException(nameof(nameProvider));
            Namespace = baseNamespace;
            Database = database ?? throw new ArgumentNullException(nameof(database));
        }

        protected INameProvider NameProvider { get; }

        protected string Namespace { get; }

        protected IRelationalDatabase Database { get; }

        public string Generate()
        {
            var builder = new StringBuilder();

            builder
                .AppendLine("using System;")
                .AppendLine("using Microsoft.EntityFrameworkCore;")
                .AppendLine()
                .Append("namespace ")
                .AppendLine(Namespace)
                .AppendLine("{")
                .Append(IndentLevel)
                .AppendLine("public class AppContext : DbContext")
                .Append(IndentLevel)
                .AppendLine("{");

            const string tableIndent = IndentLevel + IndentLevel;
            const string contextIndent = tableIndent + IndentLevel;
            var modelBuilder = new ModelBuilder(NameProvider, contextIndent, IndentLevel);

            var missingFirstLine = true;
            foreach (var table in Database.Tables)
            {
                if (!missingFirstLine)
                    builder.AppendLine();
                missingFirstLine = false;

                var schemaNamespace = NameProvider.SchemaToNamespace(table.Name);
                var className = NameProvider.TableToClassName(table.Name);
                var qualifiedClassName = !schemaNamespace.IsNullOrWhiteSpace()
                    ? schemaNamespace + "." + className
                    : className;

                var setName = className.Pluralize();

                var escapedTableName = !schemaNamespace.IsNullOrWhiteSpace()
                    ? SecurityElement.Escape(table.Name.Schema + "." + table.Name.LocalName)
                    : SecurityElement.Escape(table.Name.LocalName);
                var dbSetComment = "Accesses the <c>" + escapedTableName + "</c> table.";
                builder.AppendComment(tableIndent, dbSetComment);

                builder.Append(tableIndent)
                    .Append("public DbSet<")
                    .Append(qualifiedClassName)
                    .Append("> ")
                    .Append(setName)
                    .AppendLine(" { get; set; }");

                modelBuilder.AddTable(table);
            }

            foreach (var sequence in Database.Sequences)
            {
                modelBuilder.AddSequence(sequence);
            }

            if (modelBuilder.HasRecords)
            {
                if (!missingFirstLine)
                    builder.AppendLine();

                AppendModelBuilderComment(builder, tableIndent, ModelBuilderMethodSummaryComment, ModelBuilderMethodParamComment);

                var methodBody = modelBuilder.ToString();
                builder.Append(tableIndent)
                    .AppendLine("protected override void OnModelCreating(ModelBuilder modelBuilder)")
                    .Append(tableIndent)
                    .AppendLine("{")
                    .Append(methodBody)
                    .Append(tableIndent)
                    .AppendLine("}");
            }

            builder.Append(IndentLevel)
                .AppendLine("}")
                .AppendLine("}");

            return builder.ToString();
        }

        protected static StringBuilder AppendModelBuilderComment(StringBuilder builder, string indent, string summary, string modelBuilderParam)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (indent == null)
                throw new ArgumentNullException(nameof(indent));
            if (summary == null)
                throw new ArgumentNullException(nameof(summary));
            if (modelBuilderParam == null)
                throw new ArgumentNullException(nameof(modelBuilderParam));

            var escapedSummary = SecurityElement.Escape(summary);
            var escapedModelBuilderParam = SecurityElement.Escape(modelBuilderParam);

            return builder.Append(indent)
                .AppendLine("/// <summary>")
                .Append(indent)
                .Append("/// ")
                .AppendLine(escapedSummary)
                .Append(indent)
                .AppendLine("/// </summary>")
                .Append(indent)
                .Append("/// <param name=\"modelBuilder\">")
                .Append(escapedModelBuilderParam)
                .AppendLine("</param>");
        }

        private const string IndentLevel = "    ";
        private const string ModelBuilderMethodSummaryComment = "Configure the model that was discovered by convention from the defined entity types.";
        private const string ModelBuilderMethodParamComment = "The builder being used to construct the model for this context.";
    }
}