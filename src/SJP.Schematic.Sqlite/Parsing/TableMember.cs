﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SJP.Schematic.Sqlite.Parsing
{
    internal class TableMember
    {
        public TableMember(ColumnDefinition column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            Columns = column.ToEnumerable();
            Constraints = Enumerable.Empty<TableConstraint>();
        }

        public TableMember(TableConstraint constraint)
        {
            if (constraint == null)
                throw new ArgumentNullException(nameof(constraint));

            Columns = Enumerable.Empty<ColumnDefinition>();
            Constraints = constraint.ToEnumerable();
        }

        public IEnumerable<ColumnDefinition> Columns { get; }

        public IEnumerable<TableConstraint> Constraints { get; }
    }
}