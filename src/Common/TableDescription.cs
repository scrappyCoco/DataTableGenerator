using System.Collections.Generic;

namespace Coding4fun.DataTableGenerator.Common
{
    internal class TableDescription
    {
        public TableDescription(string sqlTableName, string? className = null)
        {
            SqlTableName = sqlTableName;
            ClassName = className;
            Columns = new List<ColumnDescription>();
            SubTables = new List<TableDescription>();
        }

        public string? ClassName { get; }
        public string SqlTableName { get; }
        public string? VarName { get; set; }
        public string? EnumerableName { get; set; }
        
        public List<ColumnDescription> Columns { get; }
        public List<TableDescription> SubTables { get; }

        /// <inheritdoc />
        public override string ToString() => $"{nameof(ClassName)}={ClassName}," +
                                             $"{nameof(SqlTableName)}={SqlTableName}," +
                                             $"{nameof(VarName)}={VarName}," +
                                             $"{nameof(EnumerableName)}={EnumerableName},";
    }
}