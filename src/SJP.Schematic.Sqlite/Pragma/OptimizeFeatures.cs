﻿using System;

namespace SJP.Schematic.Sqlite.Pragma
{
    [Flags]
    public enum OptimizeFeatures
    {
        None = 0,
        Debug = 1,
        Analyze = 2
        //RecordUsage = 4,
        //CreateIndexes = 8
    }
}