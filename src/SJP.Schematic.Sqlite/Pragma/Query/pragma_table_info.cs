﻿#pragma warning disable IDE1006, S101 // Naming Styles
namespace SJP.Schematic.Sqlite.Pragma.Query
{
    /// <summary>
    /// Stores information on columns contained within a table.
    /// </summary>
    public class pragma_table_info
    {
        /// <summary>
        /// The rank of the column within the table.
        /// </summary>
        public int cid { get; set; }

        /// <summary>
        /// The name of the column.
        /// </summary>
        public string name { get; set; } = default!;

        /// <summary>
        /// The data type associated with the column.
        /// </summary>
        public string type { get; set; } = default!;

        /// <summary>
        /// Whether the column has a not-null constraint present.
        /// </summary>
        public bool notnull { get; set; }

        /// <summary>
        /// The default value for the column.
        /// </summary>
        public string? dflt_value { get; set; }

        /// <summary>
        /// Returns <c>0</c> for columns not part of the primary key. Otherwise, stores the 1-based index of the column within the primary key.
        /// </summary>
        public int pk { get; set; }
    }
}
#pragma warning restore IDE1006, S101 // Naming Styles