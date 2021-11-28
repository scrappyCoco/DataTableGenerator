using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

namespace Coding4fun.DataTools.Analyzers.Template
{
    public abstract class ResolverBase
    {
        private readonly string _pathToResource;
        protected readonly ImmutableHashSet<string> AvailableTemplateResources;
        private static readonly Dictionary<string, CodeTemplate> Cache = new();

        protected ResolverBase(string pathToResource)
        {
            _pathToResource = pathToResource;
            
            AvailableTemplateResources = typeof(ResolverBase)
                .Assembly.GetManifestResourceNames()
                .Where(path => path.StartsWith(pathToResource))
                .Select(path => path
                    .Replace(pathToResource + ".", "")
                    .Replace(".xml", ""))
                .ToImmutableHashSet();
        }

        protected CodeTemplate ReadTemplate(string templateName)
        {
            if (!AvailableTemplateResources.Contains(templateName))
            {
                throw new InvalidOperationException($"Unable to get resource {templateName}.");
            }
            string templatePath = _pathToResource + "." + templateName + ".xml";
            if (Cache.TryGetValue(templateName, out CodeTemplate codeTemplate)) return codeTemplate;

            using Stream templatesStream = typeof(ResolverBase)
                .Assembly
                .GetManifestResourceStream(templatePath)!;

            CodeTemplate template = new XmlTemplateParser().ParseXml(templatesStream);
            Cache.Add(templateName, template);
            return template;
        }

        public abstract object?[]? Resolve(CodeTemplate template, ResolverContext context);

        public abstract bool TryReplaceTemplate(
            CodeTemplate currentTemplate,
            [NotNullWhen(true)] out CodeTemplate? newCodeTemplate);

        public string GenerateCode()
        {
            StringBuilder sharpCodeBuilder = new();
            CodeTemplate rootTemplate = ReadTemplate("Main");
            rootTemplate.BuildCode(sharpCodeBuilder, this);
            return sharpCodeBuilder.ToString();
        }
    }
}