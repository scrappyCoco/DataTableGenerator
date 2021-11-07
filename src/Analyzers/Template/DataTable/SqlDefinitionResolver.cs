using System.Collections.Generic;
using System.Linq;
using Coding4fun.DataTools.Analyzers.Extension;

namespace Coding4fun.DataTools.Analyzers.Template.DataTable
{
    internal class SqlDefinition: ResolverBase
    {
        /// <inheritdoc />
        public override object?[] Resolve(CodeTemplate template, IEnumerable<object> contextObjects)
        {
            if (template.Name == "sqlTableName")
            {
                var table = (TableDescription)contextObjects.Last();
                return new object?[] { table.SqlTableName };
            }
            
            if (template.Name == "columns")
            {
                var table = (TableDescription)contextObjects.Last();
                return table.Columns.ToArrayOfItem().Cast<object>().ToArray();
            }
            
            if (template.Name == "subTables")
            {
                var table = (TableDescription)contextObjects.Last();
                return table.SubTables.Cast<object>().ToArray();
            }
            
            if (template.Name == "sqlColumnName")
            {
                var column = (EnumerableItem<ColumnDescription>)contextObjects.Last();
                return new object?[] { column.Value.SqlColumnName };
            }
            
            if (template.Name == "sqlType")
            {
                var column = (EnumerableItem<ColumnDescription>)contextObjects.Last();
                return new object?[] { column.Value.SqlType };
            }
            
            if (template.Name == "comma")
            {
                var column = (EnumerableItem<ColumnDescription>)contextObjects.Last();
                return new object?[] { column.IsLast ? null : "," };
            }

            return new object?[] { null };
        }

        /// <inheritdoc />
        public override bool TryReplaceTemplate(CodeTemplate currentTemplate, out ResolverBase? newResolver,
            LinkedList<object> contextObjects)
        {
            if (currentTemplate.Name == "sqlTableDefinition")
            {
                newResolver = this;
                return true;
            }
            
            newResolver = null;
            return false;
        }
    }
}