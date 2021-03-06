﻿using System;
using System.IO.Abstractions;
using System.Linq;
using LanguageExt;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.DataAccess.CodeGeneration;
using SJP.Schematic.DataAccess.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SJP.Schematic.DataAccess.Poco
{
    /// <summary>
    /// Generate data access classes for views for use with POCO data access projects.
    /// </summary>
    /// <seealso cref="DatabaseTableGenerator" />
    public class PocoViewGenerator : DatabaseViewGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PocoViewGenerator"/> class.
        /// </summary>
        /// <param name="fileSystem">A file system.</param>
        /// <param name="nameTranslator">The name translator.</param>
        /// <param name="baseNamespace">The base namespace.</param>
        /// <exception cref="ArgumentNullException"><paramref name="baseNamespace"/> is <c>null</c>, empty, or whitespace.</exception>
        public PocoViewGenerator(IFileSystem fileSystem, INameTranslator nameTranslator, string baseNamespace)
            : base(fileSystem, nameTranslator)
        {
            if (baseNamespace.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(baseNamespace));

            Namespace = baseNamespace;
        }

        /// <summary>
        /// The namespace to use for the generated classes.
        /// </summary>
        /// <value>A string representing a namespace.</value>
        protected string Namespace { get; }

        /// <summary>
        /// Generates source code that enables interoperability with a given database view for POCO projects.
        /// </summary>
        /// <param name="view">A database view.</param>
        /// <param name="comment">Comment information for the given view.</param>
        /// <returns>A string containing source code to interact with the view.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="view"/> is <c>null</c>.</exception>
        public override string Generate(IDatabaseView view, Option<IDatabaseViewComments> comment)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var schemaNamespace = NameTranslator.SchemaToNamespace(view.Name);
            var viewNamespace = !schemaNamespace.IsNullOrWhiteSpace()
                ? Namespace + "." + schemaNamespace
                : Namespace;

            var namespaces = view.Columns
                .Select(c => c.Type.ClrType.Namespace)
                .Where(ns => ns != viewNamespace)
                .Distinct()
                .OrderNamespaces()
                .ToList();

            var usingStatements = namespaces
                .Select(ns => ParseName(ns))
                .Select(UsingDirective)
                .ToList();
            var namespaceDeclaration = NamespaceDeclaration(ParseName(viewNamespace));
            var classDeclaration = BuildClass(view, comment);

            var document = CompilationUnit()
                .WithUsings(List(usingStatements))
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(
                        namespaceDeclaration
                            .WithMembers(
                                SingletonList<MemberDeclarationSyntax>(classDeclaration))));

            using var workspace = new AdhocWorkspace();
            return Formatter.Format(document, workspace).ToFullString();
        }

        private ClassDeclarationSyntax BuildClass(IDatabaseView view, Option<IDatabaseViewComments> comment)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var className = NameTranslator.ViewToClassName(view.Name);
            var properties = view.Columns
                .Select(vc => BuildColumn(vc, comment, className))
                .ToList();

            return ClassDeclaration(className)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .WithLeadingTrivia(BuildViewComment(view.Name, comment))
                .WithMembers(List<MemberDeclarationSyntax>(properties));
        }

        private PropertyDeclarationSyntax BuildColumn(IDatabaseColumn column, Option<IDatabaseViewComments> comment, string className)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            if (className.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(className));

            var clrType = column.Type.ClrType;
            var propertyName = NameTranslator.ColumnToPropertyName(className, column.Name.LocalName);

            var columnTypeSyntax = column.IsNullable
                ? NullableType(ParseTypeName(clrType.FullName))
                : ParseTypeName(clrType.FullName);
            if (clrType.Namespace == nameof(System) && SyntaxUtilities.TypeSyntaxMap.ContainsKey(clrType.Name))
            {
                columnTypeSyntax = column.IsNullable
                    ? NullableType(SyntaxUtilities.TypeSyntaxMap[clrType.Name])
                    : SyntaxUtilities.TypeSyntaxMap[clrType.Name];
            }

            var baseProperty = PropertyDeclaration(
                columnTypeSyntax,
                Identifier(propertyName)
            );

            var columnSyntax = baseProperty
                .WithModifiers(SyntaxTokenList.Create(Token(SyntaxKind.PublicKeyword)))
                .WithAccessorList(SyntaxUtilities.PropertyGetSetDeclaration)
                .WithLeadingTrivia(BuildColumnComment(column.Name, comment));

            var isNotNullRefType = !column.IsNullable && !column.Type.ClrType.IsValueType;
            if (!isNotNullRefType)
                return columnSyntax;

            return columnSyntax
                .WithInitializer(SyntaxUtilities.NotNullDefault)
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        private static SyntaxTriviaList BuildViewComment(Identifier viewName, Option<IDatabaseViewComments> comment)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return comment
                .Bind(c => c.Comment)
                .Match(
                    SyntaxUtilities.BuildCommentTrivia,
                    () => SyntaxUtilities.BuildCommentTrivia(new XmlNodeSyntax[]
                    {
                        XmlText("A mapping class to query the "),
                        XmlElement("c", SingletonList<XmlNodeSyntax>(XmlText(viewName.LocalName))),
                        XmlText(" view.")
                    })
                );
        }

        private static SyntaxTriviaList BuildColumnComment(Identifier columnName, Option<IDatabaseViewComments> comment)
        {
            if (columnName == null)
                throw new ArgumentNullException(nameof(columnName));

            return comment
                .Bind(c => c.ColumnComments.TryGetValue(columnName, out var cc) ? cc : Option<string>.None)
                .Match(
                    SyntaxUtilities.BuildCommentTrivia,
                    () => SyntaxUtilities.BuildCommentTrivia(new XmlNodeSyntax[]
                    {
                        XmlText("The "),
                        XmlElement("c", SingletonList<XmlNodeSyntax>(XmlText(columnName.LocalName))),
                        XmlText(" column.")
                    })
                );
        }
    }
}
