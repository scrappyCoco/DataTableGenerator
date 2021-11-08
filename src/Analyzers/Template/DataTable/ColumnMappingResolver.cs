using System.Collections.Generic;
using System.Linq;

namespace Coding4fun.DataTools.Analyzers.Template.DataTable
{
    internal class ColumnMappingResolver: ResolverBase
    {
        private readonly string _sqlMappingName;

        internal ColumnMappingResolver(string sqlMappingName) => _sqlMappingName = sqlMappingName;

        /// <inheritdoc />
        public override object?[] Resolve(CodeTemplate template, IEnumerable<object> contextObjects)
        {
            if (template.Name == "columns")
            {
                var table = (TableDescription)contextObjects.Last();
                return table.Columns.Cast<object>().ToArray();
            }
            
            if (template.Name == "dataTableName")
            {
                var table = (TableDescription)contextObjects.Reverse().Skip(1).First();
                return new object?[] { table.DataTableName };
            }
            
            if (template.Name == "sqlColumnName")
            {
                var table = (ColumnDescription)contextObjects.Last();
                return new object?[] { table.SqlColumnName };
            }
            
            if (template.Name == "subTables")
            {
                var table = (TableDescription)contextObjects.Last();
                return table.SubTables.Cast<object>().ToArray();
            }
            
            if (template.Name == "sqlColumnName")
            {
                var table = (ColumnDescription)contextObjects.Last();
                return new object?[] { table.SqlColumnName };
            }
            
            if (template.Name == "sharpType")
            {
                var table = (ColumnDescription)contextObjects.Last();
                return new object?[] { table.SharpType };
            }
            
            if (template.Name == "sqlMappingName")
            {
                return new object?[] { _sqlMappingName };
            }

            return new object?[] { null };
        }

        /// <inheritdoc />
        public override bool TryReplaceTemplate(CodeTemplate currentTemplate, out ResolverBase? newResolver, LinkedList<object> contextObjects)
        {
            if (currentTemplate.Name == "columnMapping")
            {
                newResolver = this;
                return true;
            }
            newResolver = null;
            return false;
        }
    }
}