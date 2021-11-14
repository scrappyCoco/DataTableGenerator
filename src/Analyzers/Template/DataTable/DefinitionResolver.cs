using System.Collections.Generic;
using System.Linq;

namespace Coding4fun.DataTools.Analyzers.Template.DataTable
{
    internal class DefinitionResolver: ResolverBase
    {
        /// <inheritdoc />
        public override object?[] Resolve(CodeTemplate template, IEnumerable<object> contextObjects)
        {
            if (template.Name == "dataTableName")
            {
                var tableDescription = (TableDescription)contextObjects.Last();
                return new object?[] { tableDescription.DataTableName };
            }

            if (template.Name == "subTables")
            {
                var tableDescription = (TableDescription)contextObjects.Last();
                return tableDescription.SubTables.Cast<object>().ToArray();
            }

            return new object?[] { null };
        }

        /// <inheritdoc />
        public override bool TryReplaceTemplate(CodeTemplate currentTemplate, out ResolverBase? newResolver,
            LinkedList<object> contextObjects)
        {
            if (currentTemplate.Name == "dataTableDefinition")
            {
                newResolver = new DefinitionResolver();
                return true;
            }

            newResolver = null;
            return false;
        }
    }
}