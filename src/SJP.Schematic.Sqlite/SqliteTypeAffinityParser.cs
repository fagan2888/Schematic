﻿using System;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite
{
    public class SqliteTypeAffinityParser
    {
        // https://sqlite.org/datatype3.html#determination_of_column_affinity
        public SqliteTypeAffinity ParseTypeName(string? typeName)
        {
            if (typeName.IsNullOrWhiteSpace())
                return SqliteTypeAffinity.Numeric;

            if (typeName.Contains("INT", StringComparison.OrdinalIgnoreCase))
                return SqliteTypeAffinity.Integer;

            var isText = typeName.Contains("CHAR", StringComparison.OrdinalIgnoreCase)
                || typeName.Contains("CLOB", StringComparison.OrdinalIgnoreCase)
                || typeName.Contains("TEXT", StringComparison.OrdinalIgnoreCase);
            if (isText)
                return SqliteTypeAffinity.Text;

            if (typeName.Contains("BLOB", StringComparison.OrdinalIgnoreCase))
                return SqliteTypeAffinity.Blob;

            var isReal = typeName.Contains("REAL", StringComparison.OrdinalIgnoreCase)
                || typeName.Contains("FLOA", StringComparison.OrdinalIgnoreCase)
                || typeName.Contains("DOUB", StringComparison.OrdinalIgnoreCase);
            if (isReal)
                return SqliteTypeAffinity.Real;

            return SqliteTypeAffinity.Numeric;
        }
    }
}
