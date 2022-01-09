using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace Coding4fun.DataTools.Analyzers.Template
{
    public abstract class ResolverBase
    {
        protected readonly Dictionary<string, CodeTemplate> CodeTemplates;

        protected ResolverBase(
            string pathToResource,
            CancellationToken cancellationToken,
            IEnumerable<AdditionalText>? additionalTexts)
        {
            CodeTemplates = Templates.Read(pathToResource, cancellationToken, additionalTexts);
        }

        public abstract object?[]? Resolve(CodeTemplate template, ResolverContext context);

        public abstract bool TryReplaceTemplate(
            CodeTemplate currentTemplate,
            [NotNullWhen(true)] out CodeTemplate? newCodeTemplate);

        public string GenerateCode()
        {
            StringBuilder sharpCodeBuilder = new();
            CodeTemplate mainTemplate = CodeTemplates[Templates.MainTemplate];
            mainTemplate.BuildCode(sharpCodeBuilder, this);
            return sharpCodeBuilder.ToString();
        }
    }
}