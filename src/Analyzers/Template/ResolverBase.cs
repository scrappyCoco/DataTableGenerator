using System.Collections.Generic;
using System.IO;

namespace Coding4fun.DataTools.Analyzers.Template
{
    public abstract class ResolverBase
    {
        private static readonly Dictionary<string, CodeTemplate> Cache = new();
        private static CodeTemplate ReadTemplate(string resourcePath)
        {
            if (Cache.TryGetValue(resourcePath, out CodeTemplate codeTemplate)) return codeTemplate;
            
            using Stream templatesStream = typeof(ResolverBase)
                .Assembly
                .GetManifestResourceStream(resourcePath)!;

            CodeTemplate template = new XmlTemplateParser().ParseXml(templatesStream);
            Cache.Add(resourcePath, template);
            return template;
        }

        public CodeTemplate GetTemplate()
        {
            string xmlResourcePath = GetType().FullName.Replace("Resolver", "") + ".xml";
            return ReadTemplate(xmlResourcePath);
        }
        
        public abstract object?[]? Resolve(CodeTemplate template, IEnumerable<object> contextObjects);

        public abstract bool TryReplaceTemplate(CodeTemplate currentTemplate, 
            out ResolverBase? newResolver, LinkedList<object> contextObjects);
    }
}