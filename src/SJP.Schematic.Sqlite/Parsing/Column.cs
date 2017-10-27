﻿using System;
using System.Collections.Generic;
using Superpower.Model;
using SJP.Schematic.Core;
using EnumsNET;
using System.Linq;

namespace SJP.Schematic.Sqlite.Parsing
{
    public class Column
    {
        public Column(string columnName, IEnumerable<Token<SqliteToken>> typeDefinition, bool nullable, bool autoIncrement, SqliteCollation collation, IEnumerable<Token<SqliteToken>> defaultValue)
        {
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));
            if (!collation.IsValid())
                throw new ArgumentException($"The { nameof(SqliteCollation) } provided must be a valid enum.", nameof(collation));

            Name = columnName;
            TypeDefinition = typeDefinition?.ToList() ?? Enumerable.Empty<Token<SqliteToken>>();
            Nullable = nullable;
            IsAutoIncrement = autoIncrement;
            Collation = collation;
            DefaultValue = defaultValue?.ToList() ?? Enumerable.Empty<Token<SqliteToken>>();
        }

        public string Name { get; }

        public IEnumerable<Token<SqliteToken>> TypeDefinition { get; }

        public bool Nullable { get; }

        public bool IsAutoIncrement { get; }

        public SqliteCollation Collation { get; }

        public IEnumerable<Token<SqliteToken>> DefaultValue { get; }
    }
}