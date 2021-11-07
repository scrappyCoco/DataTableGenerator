using System.Collections.Generic;
using System.Linq;
using Coding4fun.DataTools.Analyzers.Extension;

namespace Coding4fun.DataTools.Analyzers.Template.DataTable
{
    internal class MethodDefinitionResolver: ResolverBase
    {
        /// <inheritdoc />
        public override object?[] Resolve(CodeTemplate template, IEnumerable<object> contextObjects)
        {
            if (template.Name == "className")
            {
                var table = (TableDescription)contextObjects.Last();
                return new object?[] { table.ClassName };
            }
            
            if (template.Name == "varName")
            {
                var table = (TableDescription)contextObjects.Last();
                return new object?[] { table.VarName };
            }
            
            if (template.Name == "entityName")
            {
                var table = (TableDescription)contextObjects.Last();
                return new object?[] { table.EntityName };
            }
            
            if (template.Name == "dataTableName")
            {
                var table = (TableDescription)contextObjects.Last();
                return new object?[] { table.DataTableName };
            }
            
            if (template.Name == "columns")
            {
                var table = (TableDescription)contextObjects.Last();
                return table.Columns.ToArrayOfItem().Cast<object>().ToArray();
            }
            
            if (template.Name == "valueBody")
            {
                var column = (EnumerableItem<ColumnDescription>)contextObjects.Last();
                return new object?[] { column.Value.ValueBody };
            }
            
            if (template.Name == "subTables")
            {
                var table = (TableDescription)contextObjects.Last();
                return table.SubTables.Cast<object>().ToArray();
            }
            
            if (template.Name == "comma")
            {
                var column = (EnumerableItem<ColumnDescription>)contextObjects.Last();
                return new object?[] { column.IsLast ? "" : "," };
            }

            return new object?[] { null };
        }

        /// <inheritdoc />
        public override bool TryReplaceTemplate(CodeTemplate currentTemplate, out ResolverBase? newResolver,
            LinkedList<object> contextObjects)
        {
            if (currentTemplate.Name == "methodDefinition")
            {
                newResolver = this;
                return true;
            }
            
            newResolver = null;
            return false;
        }
    }
}