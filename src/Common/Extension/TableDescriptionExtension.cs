using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Coding4fun.DataTableGenerator.Common.Extension
{
    public static class TableDescriptionExtension
    {
        internal static string[] GetDataTableNames(this TableDescription tableDescription)
        {
            List<string> tableNames = new() { tableDescription.SqlTableName.Replace("#", "") };
            foreach (var subTable in tableDescription.SubTables) tableNames.AddRange(GetDataTableNames(subTable));

            return tableNames.ToArray();
        }

        internal static string GetSqlTableDefinitions(this TableDescription tableDescription)
        {
            StringBuilder sqlStringBuilder = new();
            AppendSqlTableDefinition(sqlStringBuilder, tableDescription);
            return sqlStringBuilder.ToString();
        }

        private static void AppendSqlTableDefinition(StringBuilder sqlStringBuilder, TableDescription tableDescription)
        {
            if (!tableDescription.Columns.Any())
                throw new InvalidOperationException(
                    $"Table {tableDescription.SqlTableName} must contain at least one column. Let's try to add with method {nameof(DataTableBuilder<int>.AddColumn)}");

            sqlStringBuilder.AppendFormat("CREATE TABLE {0} (", tableDescription.SqlTableName).AppendLine();
            for (var columnNumber = 0; columnNumber < tableDescription.Columns.Count; columnNumber++)
            {
                var column = tableDescription.Columns[columnNumber];
                sqlStringBuilder.AppendFormat("    {0} {1}", column.SqlColumnName, column.SqlType);
                if (columnNumber + 1 < tableDescription.Columns.Count) sqlStringBuilder.Append(',');
                sqlStringBuilder.AppendLine();
            }

            sqlStringBuilder.Append(");");

            foreach (var subTableDescription in tableDescription.SubTables)
            {
                sqlStringBuilder.AppendLine();
                AppendSqlTableDefinition(sqlStringBuilder, subTableDescription);
            }
        }

        internal static string GetSharpDataTableDefinitions(this TableDescription tableDescription)
        {
            StringBuilder tableBuilder = new();
            AppendSharpDataTableDefinition(tableBuilder, tableDescription);
            return tableBuilder.ToString();
        }

        private static void AppendSharpDataTableDefinition(
            StringBuilder tableBuilder,
            TableDescription tableDescription)
        {
            if (!tableDescription.Columns.Any())
                throw new InvalidOperationException(
                    $"Table must contain at least one column. Let's try to add with method {nameof(DataTableBuilder<int>.AddColumn)}");

            var tableName = tableDescription.SqlTableName.Replace("#", "");
            tableBuilder.AppendFormat("{0} = new DataTable();", tableName).AppendLine();
            foreach (var column in tableDescription.Columns)
                tableBuilder.AppendFormat("{0}.Columns.Add(\"{1}\", typeof({2}));",
                    tableName,
                    column.SqlColumnName,
                    MapSqlTypeToSharp(column.SqlType)
                ).AppendLine();

            foreach (var subTable in tableDescription.SubTables)
            {
                tableBuilder.AppendLine();
                AppendSharpDataTableDefinition(tableBuilder, subTable);
            }
        }

        internal static string GetMethodDefinitions(this TableDescription tableDescription)
        {
            StringBuilder stringBuilder = new();
            AppendMethodDefinition(stringBuilder, tableDescription);
            return stringBuilder.ToString();
        }

        private static void AppendMethodDefinition(StringBuilder stringBuilder, TableDescription tableDescription)
        {
            string tableName = tableDescription.SqlTableName.Replace("#", "");

            stringBuilder
                .AppendFormat( /**/"public void Add{0}({1} {2})", tableName, tableDescription.ClassName,
                    tableDescription.VarName)
                .AppendLine( /*  */"{")
                .AppendFormat( /**/"    {0}.Rows.Add(", tableDescription.SqlTableName.Replace("#", ""))
                .AppendLine();

            for (var columnNumber = 0; columnNumber < tableDescription.Columns.Count; columnNumber++)
            {
                var column = tableDescription.Columns[columnNumber];
                stringBuilder.Append("        ").Append(column.ValueBody);
                if (columnNumber + 1 < tableDescription.Columns.Count) stringBuilder.AppendLine(",");
            }

            stringBuilder.AppendLine(");");

            foreach (var subTable in tableDescription.SubTables)
            {
                if (subTable.EnumerableName == null) throw new InvalidOperationException();
                tableName = subTable.SqlTableName.Replace("#", "");
                stringBuilder
                    .AppendFormat( /**/"    foreach (var subItem in {0})", subTable.EnumerableName).AppendLine()
                    .AppendLine( /*  */"    {")
                    .AppendFormat( /**/"        Add{0}(subItem);", tableName).AppendLine()
                    .AppendLine( /*  */"    }");
            }

            stringBuilder.AppendLine("}");

            foreach (var subTable in tableDescription.SubTables)
            {
                stringBuilder.AppendLine();
                AppendMethodDefinition(stringBuilder, subTable);
            }
        }

        private static readonly Regex SqlTypeRegex =
            new("(?<sqlType>^[a-z-A-Z_]+)", RegexOptions.Compiled | RegexOptions.Singleline);
        private static string MapSqlTypeToSharp(string sqlFullType)
        {
            // NUMERIC(15,2) => NUMERIC.
            string? sqlTypeName = SqlTypeRegex.Match(sqlFullType).Groups["sqlType"].Value?.ToUpperInvariant();
            if (sqlTypeName == null) throw new InvalidOperationException($"Unable to get sql type from {sqlFullType}");

            return sqlTypeName switch
            {
                // @formatter:off
                "VARBINARY"        => "byte[]",
                "IMAGE"            => "byte[]",
                "FILESTREAM"       => "byte[]",
                "ROWVERSION"       => "byte[]",
                "VARCHAR"          => "string",
                "CHAR"             => "string",
                "NVARCHAR"         => "string",
                "NCHAR"            => "string",
                "TEXT"             => "string",
                "NTEXT"            => "string",
                "XML"              => "string",
                "UNIQUEIDENTIFIER" => "System.Guid",
                "SMALLMONEY"       => "decimal",
                "MONEY"            => "decimal",
                "NUMERIC"          => "decimal",
                "DECIMAL"          => "decimal",
                "BIT"              => "bool",
                "TINYINT"          => "byte",
                "SMALLINT"         => "short",
                "INT"              => "int",
                "BIGINT"           => "bigint",
                "FLOAT"            => "double",
                "REAL"             => "System.Single",
                "DATE"             => "System.DateTime",
                "SMALLDATETIME"    => "System.DateTime",
                "DATETIMEOFFSET"   => "System.DateTimeOffset",
                "DATETIME"         => "System.DateTime",
                "DATETIME2"        => "System.DateTime",
                "TIME"             => "System.TimeSpan",
                "SQL_VARIANT"      => "object",
                _                  => throw new InvalidOperationException($"Unable to map {sqlFullType} to C# type.")
            };
        }
    }
}