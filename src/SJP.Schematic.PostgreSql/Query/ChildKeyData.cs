﻿namespace SJP.Schematic.PostgreSql.Query
{
    public class ChildKeyData
    {
        public string ChildTableSchema { get; set; }

        public string ChildTableName { get; set; }

        public string ChildKeyName { get; set; }

        public string ParentKeyName { get; set; }

        public string ParentKeyType { get; set; }

        public string DeleteRule { get; set; }

        public string UpdateRule { get; set; }
    }
}