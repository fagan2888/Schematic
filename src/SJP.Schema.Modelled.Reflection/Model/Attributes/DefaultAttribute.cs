﻿using System;
using System.Globalization;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection.Model
{
    public sealed class DefaultAttribute : ModelledSchemaAttribute
    {
        public DefaultAttribute(string defaultValue)
            : base(new[] { Dialect.All })
        {
            if (defaultValue.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(defaultValue));

            DefaultValue = defaultValue;
        }

        public DefaultAttribute(string defaultValue, params Type[] dialects)
            : base(dialects)
        {
            if (defaultValue.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(defaultValue));

            DefaultValue = defaultValue;
        }

        public DefaultAttribute(byte defaultValue)
            : this(defaultValue.ToString(CultureInfo.InvariantCulture))
        {
        }

        public DefaultAttribute(byte defaultValue, params Type[] dialects)
            : this(defaultValue.ToString(CultureInfo.InvariantCulture), dialects)
        {
        }

        public DefaultAttribute(char defaultValue)
            : this(defaultValue.ToString())
        {
        }

        public DefaultAttribute(char defaultValue, params Type[] dialects)
            : this(defaultValue.ToString(), dialects)
        {
        }

        public DefaultAttribute(double defaultValue)
            : this(defaultValue.ToString(CultureInfo.InvariantCulture))
        {
        }

        public DefaultAttribute(double defaultValue, params Type[] dialects)
            : this(defaultValue.ToString(CultureInfo.InvariantCulture), dialects)
        {
        }

        public DefaultAttribute(short defaultValue)
            : this(defaultValue.ToString(CultureInfo.InvariantCulture))
        {
        }

        public DefaultAttribute(short defaultValue, params Type[] dialects)
            : this(defaultValue.ToString(CultureInfo.InvariantCulture), dialects)
        {
        }

        public DefaultAttribute(int defaultValue)
            : this(defaultValue.ToString(CultureInfo.InvariantCulture))
        {
        }

        public DefaultAttribute(int defaultValue, params Type[] dialects)
            : this(defaultValue.ToString(CultureInfo.InvariantCulture), dialects)
        {
        }

        public DefaultAttribute(long defaultValue)
            : this(defaultValue.ToString(CultureInfo.InvariantCulture))
        {
        }

        public DefaultAttribute(long defaultValue, params Type[] dialects)
            : this(defaultValue.ToString(CultureInfo.InvariantCulture), dialects)
        {
        }

        public DefaultAttribute(float defaultValue)
            : this(defaultValue.ToString(CultureInfo.InvariantCulture))
        {
        }

        public DefaultAttribute(float defaultValue, params Type[] dialects)
            : this(defaultValue.ToString(CultureInfo.InvariantCulture), dialects)
        {
        }

        public string DefaultValue { get; }
    }
}
