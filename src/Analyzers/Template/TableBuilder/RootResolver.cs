using System.Collections.Generic;
using System.Text;

namespace Coding4fun.DataTools.Analyzers.Template.TableBuilder
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class RootResolver: ResolverBase
    {
        private readonly TableDescription _tableDescription;
        private readonly string _linePrefix;

        private RootResolver(TableDescription tableDescription, string linePrefix)
        {
            _tableDescription = tableDescription;
            _linePrefix = linePrefix;
        }

        public override object?[]? Resolve(CodeTemplate template, IEnumerable<object> context)
        {
            return template.Name switch
            {
                "table"     => new object[] { _tableDescription },
                "className" => new object[] { _tableDescription.ClassName! },
                "space"     => new object[] { _linePrefix },
                _           => null
            };
        }

        /// <inheritdoc />
        public override bool TryReplaceTemplate(CodeTemplate currentTemplate, out ResolverBase? newResolver,
            LinkedList<object> contextObjects)
        {
            newResolver = null;
            
            if (currentTemplate.Name == "body")
            {
                newResolver = new BodyResolver(_linePrefix);

                return true;
            }

            return false;
        }

        public static string GenerateDataBuilder(TableDescription tableDescription, string linePrefix)
        {
            RootResolver resolver = new (tableDescription, linePrefix);
            CodeTemplate codeTemplate = resolver.GetTemplate();
            StringBuilder stringBuilder = new ();
            codeTemplate.BuildCode(stringBuilder, resolver);
            return stringBuilder.ToString();
        }
    }
}