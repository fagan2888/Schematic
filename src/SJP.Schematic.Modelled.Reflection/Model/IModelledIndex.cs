﻿using System.Collections.Generic;
using System.Reflection;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public interface IModelledIndex
    {
        PropertyInfo? Property { get; set; }

        bool IsUnique { get; }

        IEnumerable<IModelledIndexColumn> Columns { get; }

        IEnumerable<IModelledColumn> IncludedColumns { get; }
    }
}
