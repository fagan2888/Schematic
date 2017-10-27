﻿using System;
using System.Collections.Generic;
using Superpower.Model;
using SJP.Schematic.Core;
using System.Linq;

namespace SJP.Schematic.Sqlite.Parsing
{
    public class Check
    {
        public Check(string constraintName, IEnumerable<Token<SqliteToken>> definition)
        {
            if (definition == null || definition.Empty())
                throw new ArgumentNullException(nameof(definition));

            Name = constraintName;
            Definition = definition.ToList();
        }

        public string Name { get; }

        public IEnumerable<Token<SqliteToken>> Definition { get; }
    }
}