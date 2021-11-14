using System.Collections.Generic;
using System.Linq;

namespace Coding4fun.DataTools.Analyzers.Template.DataTable
{
    internal class InvokeMethodResolver : ResolverBase
    {
        /// <inheritdoc />
        public override object?[] Resolve(CodeTemplate template, IEnumerable<object> contextObjects)
        {
            if (template.Name == "varName")
            {
                var table = (TableDescription)contextObjects.Last();
                return new object?[] { table.VarName };
            }

            if (template.Name == "items")
            {
                var table = (TableDescription)contextObjects.Last();
                return new object?[] { table.EnumerableName ?? "items" };
            }

            if (template.Name == "preExecutionActions")
            {
                var table = (TableDescription)contextObjects.Last();
                return table.PreExecutionActions.Cast<object>().ToArray();
            }

            if (template.Name == "entityName")
            {
                var table = (TableDescription)contextObjects.Last();
                return new object?[] { table.EntityName };
            }

            if (template.Name == "subTables")
            {
                var table = (TableDescription)contextObjects.Last();
                return table.SubTables.Cast<object>().ToArray();
            }

            if (template.Name == "offset")
            {
                int offsetLength = (contextObjects.OfType<TableDescription>().Count() + 2) * 4;
                return new object?[] { new string(' ', offsetLength) };
            }
            
            

            return new object?[] { null };
        }

        /// <inheritdoc />
        public override bool TryReplaceTemplate(CodeTemplate currentTemplate,
            out ResolverBase? newResolver,
            LinkedList<object> contextObjects)
        {
            if (currentTemplate.Name == "invokeMethod")
            {
                newResolver = new InvokeMethodResolver();
                return true;
            }
            
            newResolver = null;
            return false;
        }
    }
}