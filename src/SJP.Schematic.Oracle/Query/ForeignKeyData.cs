﻿namespace SJP.Schematic.Oracle.Query
{
    public class ForeignKeyData
    {
        public string ConstraintName { get; set; }

        public string EnabledStatus { get; set; }

        public string DeleteRule { get; set; }

        public string ParentTableSchema { get; set; }

        public string ParentTableName { get; set; }

        public string ParentConstraintName { get; set; }

        public string ParentKeyType { get; set; }

        public string ColumnName { get; set; }

        public int ColumnPosition { get; set; }
    }
}