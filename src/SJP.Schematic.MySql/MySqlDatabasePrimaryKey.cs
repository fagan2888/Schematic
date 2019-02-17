﻿using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.MySql
{
    public class MySqlDatabasePrimaryKey : MySqlDatabaseKey
    {
        public MySqlDatabasePrimaryKey(IReadOnlyCollection<IDatabaseColumn> columns)
            : base(PrimaryKeyName, DatabaseKeyType.Primary, columns)
        {
        }

        private static readonly Identifier PrimaryKeyName = Identifier.CreateQualifiedIdentifier("PRIMARY");
    }
}
