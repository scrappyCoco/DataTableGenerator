using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Coding4fun.DataTools.Analyzers.Template.DataTable
{
    internal class ClassDefinitionResolver : ResolverBase
    {
        private readonly TableDescription _tableDescription;
        private readonly string _ns;
        private readonly string _sqlMappingClassName;
        private readonly string[] _usingNamespaces;

        public ClassDefinitionResolver(
            TableDescription tableDescription,
            IEnumerable<string> usingNamespaces,
            string @namespace,
            string sqlMappingClassName)
        {
            _tableDescription = tableDescription;
            _ns = @namespace;
            _sqlMappingClassName = sqlMappingClassName;
            _usingNamespaces = usingNamespaces.ToArray();
        }

        /// <inheritdoc />
        public override object?[] Resolve(CodeTemplate template, IEnumerable<object> contextObjects)
        {
            if (template.Name == "class") return new object?[] { _tableDescription };
            if (template.Name == "usingNamespaces") return _usingNamespaces.Cast<object>().ToArray();
            if (template.Name == "usingNamespace") return new[] { contextObjects.Last() };
            if (template.Name == "namespace") return new object?[] { _ns };
            if (template.Name == "sqlMappingClassName") return new object?[] { _sqlMappingClassName };
            if (template.Name == "className") return new object?[] { _tableDescription.ClassName };
            if (template.Name == "methodDefinition") return new object?[] { "TODO:methodDefinition" };
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
            
            if (currentTemplate.Name == "sqlTableDefinition")
            {
                newResolver = new SqlDefinition();
                return true;
            }
            
            if (currentTemplate.Name == "invokeMethod")
            {
                newResolver = new InvokeMethodResolver();
                return true;
            }
            
            if (currentTemplate.Name == "methodDefinition")
            {
                newResolver = new MethodDefinitionResolver();
                return true;
            }

            if (currentTemplate.Name == "bulkCopy")
            {
                newResolver = new BulkCopyResolver();
                return true;
            }
            
            if (currentTemplate.Name == "columnMapping")
            {
                newResolver = new ColumnMappingResolver(_sqlMappingClassName);
                return true;
            }

            newResolver = null;
            return false;
        }

        public static string GenerateDataTable(TableDescription tableDescription, IEnumerable<string> usingNamespaces,
            string @namespace,
            string sqlMappingClassName)
        {
            ClassDefinitionResolver resolver = new(tableDescription, usingNamespaces, @namespace,
                sqlMappingClassName);
            CodeTemplate codeTemplate = resolver.GetTemplate();
            StringBuilder stringBuilder = new();
            codeTemplate.BuildCode(stringBuilder, resolver);
            return stringBuilder.ToString();
        }
    }
}