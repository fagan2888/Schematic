﻿using System;
using SJP.Schematic.Core;
using Superpower.Model;

namespace SJP.Schematic.Sqlite.Parsing
{
    internal class SqlIdentifier
    {
        public SqlIdentifier(Token<SqliteToken> token)
        {
            if (token.Kind != SqliteToken.Identifier || token.ToStringValue().IsNullOrWhiteSpace())
                throw new ArgumentException("The provided token must be an identifier token. Instead given: " + token.Kind.ToString(), nameof(token));

            Value = UnwrapIdentifier(token.ToStringValue());
        }

        public string Value { get; }

        private static string UnwrapIdentifier(string identifier)
        {
            if (identifier.StartsWith("\""))
            {
                var result = TrimWrappingChars(identifier);
                return result.Replace("\"\"", "\"");
            }
            else if (identifier.StartsWith("["))
            {
                var result = TrimWrappingChars(identifier);
                return result.Replace("]]", "]");
            }
            else if (identifier.StartsWith("`"))
            {
                var result = TrimWrappingChars(identifier);
                return result.Replace("``", "`");
            }
            else
            {
                return identifier;
            }
        }

        private static string TrimWrappingChars(string input) => input.Substring(1, input.Length - 2);
    }
}