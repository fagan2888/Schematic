﻿namespace SJP.Schematic.Core
{
    public interface IDatabaseTrigger : IDatabaseEntity, IDatabaseOptional
    {
        IRelationalDatabaseTable Table { get; }

        string Definition { get; }

        TriggerQueryTiming QueryTiming { get; }

        TriggerEvent TriggerEvent { get; }
    }
}