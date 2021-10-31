using System.IO;
using Antlr4.StringTemplate;
using Microsoft.CodeAnalysis;

namespace Coding4fun.DataTools.Analyzers
{
    public static class TemplateManager
    {
        private static Template GetTemplate(string resourcePath, string instance)
        {
            using Stream? templatesStream = typeof(TemplateManager).Assembly
                .GetManifestResourceStream(resourcePath);

            if (templatesStream == null)
            {
                throw new SourceGeneratorException("Unable to find resource with string template.", Location.None);
            }

            TemplateGroupString templateGroup = new(new StreamReader(templatesStream!).ReadToEnd());
            Template? classTemplate = templateGroup.GetInstanceOf(instance);
            return classTemplate;
        }

        public static Template GetDataTableTemplate() =>
            GetTemplate("Coding4fun.DataTools.Analyzers.Template.DataTable.stg", "ClassDefinition");
        
        public static Template GetTableBuilderTemplate() =>
            GetTemplate("Coding4fun.DataTools.Analyzers.Template.TableBuilder.stg", "RootTableBuilder");
    }
}