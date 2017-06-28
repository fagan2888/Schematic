﻿using System;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection.Model
{
    public static partial class ColumnType
    {
        public sealed class BinaryAttribute : DeclaredTypeAttribute
        {
            public BinaryAttribute(int length, bool isFixedLength = false)
            : base(DataType.Binary, length, isFixedLength, new[] { Dialect.All })
            {
            }

            public BinaryAttribute(int length, bool isFixedLength = false, params Type[] dialects)
                : base(DataType.Binary, length, isFixedLength, dialects)
            {
            }
        }
    }
}
