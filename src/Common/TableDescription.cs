using System;
using System.Collections.Generic;
using Coding4fun.PainlessUtils;

namespace Coding4fun.DataTableGenerator.Common
{
    internal class TableDescription
    {
        private readonly Func<string, string> _table2dtConverter = name =>
            name.ChangeCase(CaseRules.ToTitleCase, "") + "DataTable"
            ?? throw new NullReferenceException("Unable to change case for data table name");
        
        private readonly Func<string, string> _table2EntityNameConverter = name =>
            name.ChangeCase(CaseRules.ToTitleCase, "")
            ?? throw new NullReferenceException("Unable to change case for entity name");
        
        public TableDescription(string sqlTableName, string? className = null)
        {
            SqlTableName = sqlTableName;
            ClassName = className;
            DataTableName = _table2dtConverter.Invoke(sqlTableName);
            EntityName = _table2EntityNameConverter.Invoke(sqlTableName);
            Columns = new List<ColumnDescription>();
            SubTables = new List<TableDescription>();
        }

        public string EntityName { get; }
        public string? ClassName { get; }
        public string SqlTableName { get; }
        public string DataTableName { get; }
        public string? VarName { get; set; }
        public string? EnumerableName { get; set; }
        
        public List<ColumnDescription> Columns { get; }
        public List<TableDescription> SubTables { get; }

        /// <inheritdoc />
        public override string ToString() => $"{nameof(ClassName)}={ClassName}," +
                                             $"{nameof(SqlTableName)}={SqlTableName}," +
                                             $"{nameof(VarName)}={VarName}," +
                                             $"{nameof(DataTableName)}={DataTableName}," +
                                             $"{nameof(EnumerableName)}={EnumerableName},";
    }
}