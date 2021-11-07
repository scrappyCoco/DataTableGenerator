using System;
using System.Collections.Generic;
using Coding4fun.DataTools.Common.StringUtil;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace Coding4fun.DataTools.Analyzers
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    internal class TableDescription
    {
        public TableDescription(string className, string? sqlTableName = null)
        {
            SqlTableName = sqlTableName;
            VarName = className.ChangeCase(CaseRules.ToCamelCase);
            EntityName = className.ChangeCase(CaseRules.ToTitleCase)!;
            ClassName = className;
            DataTableName = className.ChangeCase(CaseRules.ToTitleCase, "") + "DataTable";
            Columns = new List<ColumnDescription>();
            SubTables = new List<TableDescription>();
        }
        
        public ITypeSymbol? GenericType { get; set; }
        public string EntityName { get; internal set; }
        public string? ClassName { get; }
        public string? SqlTableName { get; internal set; }
        public string DataTableName { get; }
        public string? VarName { get; set; }
        public string? EnumerableName { get; set; }
        public string[] PreExecutionActions { get; set; } = Array.Empty<string>();
        public TableDescription? ParentTable { get; set; }
        
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