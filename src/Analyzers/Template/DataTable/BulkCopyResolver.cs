using System.Collections.Generic;
using System.Linq;

namespace Coding4fun.DataTools.Analyzers.Template.DataTable
{
    internal class BulkCopyResolver : ResolverBase
    {
        /// <inheritdoc />
        public override object?[] Resolve(CodeTemplate template, IEnumerable<object> contextObjects)
        {
            if (template.Name == "varName")
            {
                var table = (TableDescription)contextObjects.Last();
                return new object?[] { table.VarName };
            }

            if (template.Name == "sqlTableName")
            {
                var table = (TableDescription)contextObjects.Last();
                return new object?[] { table.SqlTableName };
            }

            if (template.Name == "dataTableName")
            {
                var table = (TableDescription)contextObjects.Last();
                return new object?[] { table.DataTableName };
            }

            if (template.Name == "subTables")
            {
                var table = (TableDescription)contextObjects.Last();
                return table.SubTables.Cast<object>().ToArray();
            }

            return new object?[] { null };
        }

        /// <inheritdoc />
        public override bool TryReplaceTemplate(CodeTemplate currentTemplate, out ResolverBase? newResolver,
            LinkedList<object> contextObjects)
        {
            if (currentTemplate.Name == "bulkCopy")
            {
                newResolver = this;
                return true;
            }
            
            newResolver = null;
            return false;
        }
    }
}