﻿using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer
{
    public class SqlServerDatabaseSynonym : IDatabaseSynonym
    {
        public SqlServerDatabaseSynonym(IRelationalDatabase database, Identifier synonymName, Identifier targetName)
        {
            if (synonymName == null || synonymName.LocalName == null)
                throw new ArgumentNullException(nameof(synonymName));
            if (targetName == null || targetName.LocalName == null)
                throw new ArgumentNullException(nameof(targetName));

            Database = database ?? throw new ArgumentNullException(nameof(database));

            if (synonymName.Schema == null && database.DefaultSchema != null)
                synonymName = new Identifier(database.DefaultSchema, synonymName.LocalName);
            Name = synonymName;

            if (targetName.Schema == null && database.DefaultSchema != null)
                targetName = new Identifier(database.DefaultSchema, targetName.LocalName);
            Target = targetName; // don't check for validity of target, could be a broken synonym
        }

        public IRelationalDatabase Database { get; }

        public Identifier Name { get; }

        public Identifier Target { get; }
    }
}
