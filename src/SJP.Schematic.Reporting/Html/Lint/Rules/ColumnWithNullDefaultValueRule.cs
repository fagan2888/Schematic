﻿using System;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules
{
    internal class ColumnWithNullDefaultValueRule : Schematic.Lint.Rules.ColumnWithNullDefaultValueRule
    {
        public ColumnWithNullDefaultValueRule(RuleLevel level)
            : base(level)
        {
        }

        protected override IRuleMessage BuildMessage(Identifier tableName, string columnName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));

            var tableLink = $"<a href=\"tables/{ tableName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
            var messageText = $"The table { tableLink } has a column <code>{ HttpUtility.HtmlEncode(columnName) }</code> whose default value is <code>NULL</code>. Consider removing the default value on the column.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }
    }
}