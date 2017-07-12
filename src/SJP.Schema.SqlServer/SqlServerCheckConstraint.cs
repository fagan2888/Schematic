﻿using System;
using System.Collections.Generic;
using SJP.Schema.Core;

namespace SJP.Schema.SqlServer
{
    public class SqlServerCheckConstraint : IDatabaseCheckConstraint
    {
        public SqlServerCheckConstraint(IRelationalDatabaseTable table, Identifier checkName, string definition, bool isEnabled)
        {
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));

            Table = table ?? throw new ArgumentNullException(nameof(table));
            Name = checkName ?? throw new ArgumentNullException(nameof(checkName));
            Definition = definition;
            IsEnabled = isEnabled;
        }

        public IRelationalDatabaseTable Table { get; }

        public Identifier Name { get; }

        public string Definition { get; }

        public bool IsEnabled { get; }
    }
}
