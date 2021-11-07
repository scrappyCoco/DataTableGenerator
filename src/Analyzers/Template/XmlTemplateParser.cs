using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Coding4fun.DataTools.Analyzers.Template
{
    public class XmlTemplateParser
    {
        public CodeTemplate ParseXml(Stream templateStream)
        {
            using var streamReader = new StreamReader(templateStream);
            return ParseXml(streamReader);
        }

        public CodeTemplate ParseXml(TextReader streamReader)
        {
            using var xmlReader = XmlReader.Create(streamReader);

            while (xmlReader.Read())
            {
                if (xmlReader.NodeType != XmlNodeType.Element) continue;
                return ReadBlock(xmlReader);
            }

            throw new InvalidOperationException();
        }

        private CodeTemplate ReadBlock(XmlReader xmlReader)
        {
            LinkedList<CodeTemplate> children = new ();
            
            string blockName = xmlReader.Name;
            CodeTemplate template = new CodeTemplate(blockName, null, Array.Empty<CodeTemplate>());
            if (xmlReader.IsEmptyElement) return template;

            while (xmlReader.Read())
            {
                switch (xmlReader.NodeType)
                {
                    case XmlNodeType.Text:
                    case XmlNodeType.Whitespace:
                        children.AddLast(new CodeTemplate(template, xmlReader.Value));
                        break;
                    case XmlNodeType.Element:
                        children.AddLast(ReadBlock(xmlReader));
                        break;
                    case XmlNodeType.EndElement:
                        template.Children = children.ToArray();
                        foreach (CodeTemplate child in children)
                        {
                            child.Parent = template;
                        }
                        return template;
                }
            }

            throw new InvalidOperationException();
        }
    }
}