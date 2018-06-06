﻿using SJP.Schematic.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SJP.Schematic.Reporting.Dot
{
    internal sealed class TableNode : DotNode
    {
        public TableNode(
            DotIdentifier identifier,
            string tableName,
            IEnumerable<string> columnNames,
            IEnumerable<string> columnTypes,
            IEnumerable<string> keyColumnNames,
            uint children,
            uint parents,
            ulong rows,
            IEnumerable<NodeAttribute> nodeAttrs,
            TableNodeOptions options
        )
            : base(identifier)
        {
            if (tableName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(tableName));
            _tableName = tableName;
            _columnNames = columnNames?.ToList() ?? throw new ArgumentNullException(nameof(columnNames));
            _columnTypes = columnTypes?.ToList() ?? throw new ArgumentNullException(nameof(columnTypes));
            _keyColumnNames = keyColumnNames?.ToList() ?? throw new ArgumentNullException(nameof(keyColumnNames));
            _children = children;
            _parents = parents;
            _rows = rows;
            _nodeAttrs = nodeAttrs ?? throw new ArgumentNullException(nameof(nodeAttrs));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        protected override string BuildDot()
        {
            var builder = new StringBuilder();

            builder.Append(Identifier);
            builder.AppendLine(" [");

            const string indent = "  ";
            builder.Append(indent).AppendLine("label=<");

            var borderSize = _options.ShowColumnDataType ? 2 : 0;
            var borderSizeText = borderSize.ToString();

            var table = new XElement(HtmlElement.Table,
                new XAttribute(HtmlAttribute.Border, borderSizeText),
                new XAttribute(HtmlAttribute.CellBorder, 1),
                new XAttribute(HtmlAttribute.CellSpacing, 0),
                new XAttribute(HtmlAttribute.BackgroundColor, _options.TableBackgroundColor)
            );

            var headerBackgroundColor = _options.IsHighlighted
                ? _options.HighlightedHeaderBackgroundColor
                : _options.HeaderBackgroundColor;
            var tableHeaderRow = new XElement(HtmlElement.TableRow,
                new XElement(HtmlElement.TableCell,
                    new XAttribute(HtmlAttribute.BackgroundColor, headerBackgroundColor),
                    new XAttribute(HtmlAttribute.ColumnSpan, 3),
                    new XElement(HtmlElement.Font,
                        new XAttribute(HtmlAttribute.FontFace, nameof(FontFace.Helvetica)),
                        new XElement(HtmlElement.Bold, "Table"))));
            var keyNameHeaderRow = new XElement(HtmlElement.TableRow,
                new XElement(HtmlElement.TableCell,
                    new XAttribute(HtmlAttribute.BackgroundColor, headerBackgroundColor),
                    new XAttribute(HtmlAttribute.ColumnSpan, 3),
                    new XElement(HtmlElement.Bold, _tableName)));

            table.Add(tableHeaderRow);
            table.Add(keyNameHeaderRow);

            var hasSkippedRows = false;
            var columnRows = _columnNames.Select((c, i) =>
            {
                if (_options.IsReducedColumnSet && !_keyColumnNames.Contains(c))
                {
                    hasSkippedRows = true;
                    return null;
                }

                var columnRow = new XElement(HtmlElement.TableRow);
                var columnCell = new XElement(HtmlElement.TableCell,
                        new XAttribute(HtmlAttribute.Port, c),
                        new XAttribute(HtmlAttribute.ColumnSpan, 3),
                        new XAttribute(HtmlAttribute.Align, "LEFT"));

                var columnCellTable = new XElement(HtmlElement.Table,
                    new XAttribute(HtmlAttribute.Border, 0),
                    new XAttribute(HtmlAttribute.CellSpacing, 0),
                    new XAttribute(HtmlAttribute.Align, "LEFT"));

                var mainColumnRow = new XElement(HtmlElement.TableRow,
                    new XAttribute(HtmlAttribute.Align, "LEFT"));
                var columnNameCell = new XElement(HtmlElement.TableCell,
                    new XAttribute(HtmlAttribute.Align, "LEFT"),
                    c);

                mainColumnRow.Add(columnNameCell);
                columnCellTable.Add(mainColumnRow);
                columnCell.Add(columnCellTable);
                columnRow.Add(columnCell);

                if (_options.ShowColumnDataType)
                {
                    var columnType = _columnTypes[i];
                    var columnTypeCell = new XElement(HtmlElement.TableCell,
                        new XAttribute(HtmlAttribute.Port, columnType),
                        new XAttribute(HtmlAttribute.Align, "LEFT"));

                    columnRow.Add(columnTypeCell);
                }

                return columnRow;
            })
            .Where(c => c != null)
            .ToList();

            foreach (var columnRow in columnRows)
                table.Add(columnRow);

            if (hasSkippedRows)
            {
                var endRow = new XElement(HtmlElement.TableRow,
                    new XElement(HtmlElement.TableCell,
                        new XAttribute(HtmlAttribute.Port, "ellipsis"),
                        new XAttribute(HtmlAttribute.ColumnSpan, 3),
                        new XAttribute(HtmlAttribute.Align, "LEFT"),
                        "…"));
                table.Add(endRow);
            }

            var footerRow = new XElement(HtmlElement.TableRow);

            // can't use string.Empty as graphviz needs at least some whitespace in the <FONT> tag
            const string emptyText = " ";

            var footerBackgroundColor = _options.IsHighlighted
                ? _options.HighlightedFooterBackgroundColor
                : _options.FooterBackgroundColor;

            var foreignKeyCellText = _parents > 0 ? _parents.ToString() + " P" : emptyText;
            var foreignKeyCell = new XElement(HtmlElement.TableCell,
                        new XAttribute(HtmlAttribute.Align, "LEFT"),
                        new XAttribute(HtmlAttribute.BackgroundColor, footerBackgroundColor),
                        new XElement(HtmlElement.Font,
                            new XAttribute(HtmlAttribute.FontFace, nameof(FontFace.Helvetica)),
                            foreignKeyCellText));

            var rowsCellText = _rows.ToString() + " row" + (_rows != 1 ? "s" : string.Empty);

            var rowsCell = new XElement(HtmlElement.TableCell,
                new XAttribute(HtmlAttribute.Align, "RIGHT"),
                new XAttribute(HtmlAttribute.BackgroundColor, footerBackgroundColor),
                new XElement(HtmlElement.Font,
                    new XAttribute(HtmlAttribute.FontFace, nameof(FontFace.Helvetica)),
                    rowsCellText));

            var childKeyCellText = _children > 0 ? _children.ToString() + " C" : emptyText;
            var childKeyCell = new XElement(HtmlElement.TableCell,
                new XAttribute(HtmlAttribute.Align, "RIGHT"),
                new XAttribute(HtmlAttribute.BackgroundColor, footerBackgroundColor),
                new XElement(HtmlElement.Font,
                    new XAttribute(HtmlAttribute.FontFace, nameof(FontFace.Helvetica)),
                    childKeyCellText));

            footerRow.Add(foreignKeyCell);
            footerRow.Add(rowsCell);
            footerRow.Add(childKeyCell);

            table.Add(footerRow);

            // do not use SaveOptions.None as indented XML causes incorrect formatting in Graphviz
            var labelContent = table.ToString(SaveOptions.DisableFormatting);

            const string labelIndent = "    ";
            builder.Append(labelIndent).AppendLine(labelContent);

            builder.Append(indent).AppendLine(">");

            foreach (var nodeAttr in _nodeAttrs)
                builder.Append(indent).AppendLine(nodeAttr.ToString());

            builder.Append("]");

            return builder.ToString();
        }

        private readonly string _tableName;
        private readonly IReadOnlyList<string> _columnNames;
        private readonly IReadOnlyList<string> _columnTypes;
        private readonly IEnumerable<string> _keyColumnNames;
        private readonly uint _children;
        private readonly uint _parents;
        private readonly ulong _rows;
        private readonly IEnumerable<NodeAttribute> _nodeAttrs;
        private readonly TableNodeOptions _options;
    }
}