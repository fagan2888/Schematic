﻿using System;
using System.Collections.Generic;
using EnumsNET;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint
{
    public abstract class Rule : IRule
    {
        protected Rule(string title, RuleLevel level)
        {
            if (title.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(title));
            if (!level.IsValid())
                throw new ArgumentException($"The { nameof(RuleLevel) } provided must be a valid enum.", nameof(level));

            Level = level;
            Title = title;
        }

        public RuleLevel Level { get; }

        public string Title { get; }

        public abstract IEnumerable<IRuleMessage> AnalyseDatabase(IRelationalDatabase database);
    }
}