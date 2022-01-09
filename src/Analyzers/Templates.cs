using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Coding4fun.DataTools.Analyzers.Template;
using Microsoft.CodeAnalysis;

namespace Coding4fun.DataTools.Analyzers
{
    public static class Templates
    {
        // @formatter:off
        private  const string Coding4FunNamespace = "Coding4fun.DataTools";
        internal const string MainTemplate        = "Main";
        public   const string DataTable           = Coding4FunNamespace + ".Analyzers.Template.DataTable";
        public   const string TableBuilder        = Coding4FunNamespace + ".Analyzers.Template.TableBuilder";
        public   const string Unresolved          = Coding4FunNamespace + ".Analyzers.Template.Unresolved";
        // @formatter:on

        private static string GetTemplateName(string filePath, string prefix) => filePath
            .Replace(prefix + ".", "")
            .Replace(".xml", "");
        
        internal static Dictionary<string, CodeTemplate> Read(
            string templateNamespace,
            CancellationToken cancellationToken,
            IEnumerable<AdditionalText>? additionalDocuments = null)
        {
            Dictionary<string, CodeTemplate> templates = new();

            if (templateNamespace.StartsWith(Coding4FunNamespace))
            {
                Assembly assembly = typeof(Templates).Assembly;

                templates = assembly.GetManifestResourceNames()
                    .Where(path => path.StartsWith(templateNamespace))
                    .ToDictionary(
                        path => GetTemplateName(path, templateNamespace),
                        path => new XmlTemplateParser().ParseXml(assembly.GetManifestResourceStream(path)!)
                    );
            }
            else
            {
                AdditionalText[] templateFiles = additionalDocuments!.Where(d => d.Path.StartsWith(templateNamespace)).ToArray();
                
                using StringWriter stringWriter = new();
                StringBuilder stringBuilder = stringWriter.GetStringBuilder();
                
                foreach (AdditionalText additionalDocument in templateFiles)
                {
                    additionalDocument.GetText(cancellationToken)!.Write(stringWriter, cancellationToken);
                    string groupName = GetTemplateName(additionalDocument.Path, templateNamespace);
                    using StringReader stringReader = new(stringBuilder.ToString());
                    templates.Add(groupName, new XmlTemplateParser().ParseXml(stringReader));
                    stringBuilder.Clear();
                }

                if (!templates.ContainsKey(MainTemplate))
                    throw new InvalidOperationException($"Unable to find {MainTemplate} template in {templateNamespace}");
            }

            return templates;
        }
    }
}