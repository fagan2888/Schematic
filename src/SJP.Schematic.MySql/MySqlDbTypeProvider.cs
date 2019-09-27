﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.MySql
{
    public class MySqlDbTypeProvider : IDbTypeProvider
    {
        public IDbType CreateColumnType(ColumnTypeMetadata typeMetadata)
        {
            if (typeMetadata == null)
                throw new ArgumentNullException(nameof(typeMetadata));

            if (typeMetadata.TypeName == null)
                typeMetadata.TypeName = GetDefaultTypeName(typeMetadata);
            if (typeMetadata.DataType == DataType.Unknown)
                typeMetadata.DataType = GetDataType(typeMetadata.TypeName.LocalName);
            if (typeMetadata.ClrType == null)
                typeMetadata.ClrType = GetClrType(typeMetadata.TypeName.LocalName);
            typeMetadata.IsFixedLength = GetIsFixedLength(typeMetadata.TypeName.LocalName);

            var definition = GetFormattedTypeName(typeMetadata);
            return new ColumnDataType(
                typeMetadata.TypeName,
                typeMetadata.DataType,
                definition,
                typeMetadata.ClrType,
                typeMetadata.IsFixedLength,
                typeMetadata.MaxLength,
                typeMetadata.NumericPrecision,
                typeMetadata.Collation
            );
        }

        public IDbType GetComparableColumnType(IDbType otherType)
        {
            if (otherType == null)
                throw new ArgumentNullException(nameof(otherType));

            var typeMetadata = new ColumnTypeMetadata
            {
                ClrType = null, // ignoring so we get the default type provided
                Collation = otherType.Collation,
                DataType = otherType.DataType,
                IsFixedLength = otherType.IsFixedLength,
                MaxLength = otherType.MaxLength,
                NumericPrecision = otherType.NumericPrecision,
                TypeName = null // ignoring so we get a default name generated
            };

            return CreateColumnType(typeMetadata);
        }

        protected static bool GetIsFixedLength(string typeName)
        {
            if (typeName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(typeName));

            return FixedLengthTypes.Contains(typeName);
        }

        protected static Identifier GetDefaultTypeName(ColumnTypeMetadata typeMetadata)
        {
            if (typeMetadata == null)
                throw new ArgumentNullException(nameof(typeMetadata));

            switch (typeMetadata.DataType)
            {
                case DataType.BigInteger:
                    return "bigint";
                case DataType.Binary:
                    return typeMetadata.IsFixedLength ? "binary" : "varbinary";
                case DataType.LargeBinary:
                    return "longblob";
                case DataType.Boolean:
                    return "bit";
                case DataType.Date:
                    return "date";
                case DataType.DateTime:
                    return "datetime";
                case DataType.Float:
                    return "double";
                case DataType.Integer:
                    return "int";
                case DataType.Interval:
                    return "timestamp";
                case DataType.Numeric:
                    return "numeric";
                case DataType.SmallInteger:
                    return "smallint";
                case DataType.String:
                case DataType.Unicode:
                    return typeMetadata.IsFixedLength ? "char" : "varchar";
                case DataType.Text:
                case DataType.UnicodeText:
                    return "longtext";
                case DataType.Time:
                    return "time";
                case DataType.Unknown:
                    throw new ArgumentOutOfRangeException(nameof(typeMetadata), "Unable to determine a type name for an unknown data type.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeMetadata), "Unable to determine a type name for data type: " + typeMetadata.DataType.ToString());
            }
        }

        protected static string GetFormattedTypeName(ColumnTypeMetadata typeMetadata)
        {
            if (typeMetadata == null)
                throw new ArgumentNullException(nameof(typeMetadata));
            if (typeMetadata.TypeName == null)
                throw new ArgumentException("The type name is missing. A formatted type name cannot be generated.", nameof(typeMetadata));

            var builder = StringBuilderCache.Acquire(typeMetadata.TypeName.LocalName.Length * 2);
            var typeName = typeMetadata.TypeName.LocalName;

            builder.Append(typeName);

            if (TypeNamesWithNoLengthAnnotation.Contains(typeName))
                return builder.GetStringAndRelease();

            builder.Append('(');

            var npWithPrecisionOrScale = typeMetadata.NumericPrecision.Filter(np => np.Precision > 0 || np.Scale > 0);
            if (npWithPrecisionOrScale.IsSome)
            {
                npWithPrecisionOrScale.IfSome(precision =>
                {
                    builder.Append(precision.Precision.ToString(CultureInfo.InvariantCulture));
                    if (precision.Scale > 0)
                    {
                        builder.Append(", ");
                        builder.Append(precision.Scale.ToString(CultureInfo.InvariantCulture));
                    }
                });
            }
            else if (typeMetadata.MaxLength > 0)
            {
                var maxLength = typeMetadata.DataType == DataType.Unicode || typeMetadata.DataType == DataType.UnicodeText;
                builder.Append(maxLength.ToString(CultureInfo.InvariantCulture));
            }

            builder.Append(')');

            return builder.GetStringAndRelease();
        }

        protected static DataType GetDataType(string typeName)
        {
            if (typeName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(typeName));

            return StringToDataTypeMap.ContainsKey(typeName)
                ? StringToDataTypeMap[typeName]
                : DataType.Unknown;
        }

        protected static Type GetClrType(string typeName)
        {
            if (typeName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(typeName));

            return StringToClrTypeMap.ContainsKey(typeName)
                ? StringToClrTypeMap[typeName]
                : typeof(object);
        }

        private static readonly IEnumerable<string> FixedLengthTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "char",
            "binary"
        };

        private static readonly IEnumerable<string> TypeNamesWithNoLengthAnnotation = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "bit",
            "tinyint",
            "smallint",
            "mediumint",
            "int",
            "bigint",
            "double",
            "tinyblob",
            "blob",
            "mediumblob",
            "largeblob",
            "tinytext",
            "text",
            "mediumtext",
            "longtext"
        };

        private static readonly IReadOnlyDictionary<string, DataType> StringToDataTypeMap = new Dictionary<string, DataType>(StringComparer.OrdinalIgnoreCase)
        {
            ["bit"] = DataType.Boolean,
            ["tinyint"] = DataType.Integer,
            ["smallint"] = DataType.Integer,
            ["mediumint"] = DataType.Integer,
            ["int"] = DataType.Integer,
            ["bigint"] = DataType.Integer,
            ["numeric"] = DataType.Numeric,
            ["decimal"] = DataType.Numeric,
            ["float"] = DataType.Float,
            ["double"] = DataType.Float,
            ["date"] = DataType.Date,
            ["datetime"] = DataType.DateTime,
            ["timestamp"] = DataType.DateTime,
            ["time"] = DataType.Time,
            ["year"] = DataType.Integer,
            ["char"] = DataType.Unicode,
            ["varchar"] = DataType.Unicode,
            ["binary"] = DataType.Binary,
            ["varbinary"] = DataType.Binary,
            ["tinyblob"] = DataType.LargeBinary,
            ["blob"] = DataType.LargeBinary,
            ["mediumblob"] = DataType.LargeBinary,
            ["largeblob"] = DataType.LargeBinary,
            ["tinytext"] = DataType.UnicodeText,
            ["text"] = DataType.UnicodeText,
            ["mediumtext"] = DataType.UnicodeText,
            ["longtext"] = DataType.UnicodeText
        };

        private static readonly IReadOnlyDictionary<string, Type> StringToClrTypeMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            ["bit"] = typeof(bool),
            ["tinyint"] = typeof(byte),
            ["smallint"] = typeof(short),
            ["mediumint"] = typeof(int),
            ["int"] = typeof(int),
            ["bigint"] = typeof(long),
            ["decimal"] = typeof(decimal),
            ["numeric"] = typeof(decimal),
            ["float"] = typeof(double),
            ["double"] = typeof(double),
            ["date"] = typeof(DateTime),
            ["datetime"] = typeof(DateTime),
            ["timestamp"] = typeof(DateTime),
            ["time"] = typeof(DateTime),
            ["year"] = typeof(int),
            ["char"] = typeof(string),
            ["varchar"] = typeof(string),
            ["tinytext"] = typeof(string),
            ["text"] = typeof(string),
            ["mediumtext"] = typeof(string),
            ["longtext"] = typeof(string),
            ["binary"] = typeof(byte[]),
            ["varbinary"] = typeof(byte[]),
            ["tinyblob"] = typeof(byte[]),
            ["blob"] = typeof(byte[]),
            ["mediumblob"] = typeof(byte[]),
            ["largeblob"] = typeof(byte[])
        };
    }
}
