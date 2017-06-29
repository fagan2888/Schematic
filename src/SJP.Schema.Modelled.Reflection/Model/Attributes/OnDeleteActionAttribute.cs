﻿using System;

namespace SJP.Schema.Modelled.Reflection.Model
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class OnDeleteActionAttribute : ModelledSchemaAttribute
    {
        public OnDeleteActionAttribute(ForeignKeyAction action)
            : base(new[] { Dialect.All })
        {
            Action = action;
        }

        public OnDeleteActionAttribute(ForeignKeyAction action, params Type[] dialects)
            : base(dialects)
        {
            Action = action;
        }

        public ForeignKeyAction Action { get; }
    }
}