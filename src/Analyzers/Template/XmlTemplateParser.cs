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
                if (xmlReader.NodeType == XmlNodeType.Element) break;
            }

            return ReadBlock(xmlReader);
        }

        private CodeTemplate ReadBlock(XmlReader xmlReader)
        {
            LinkedList<CodeTemplate> children = new ();
            Dictionary<string, string> attributes = new(StringComparer.InvariantCultureIgnoreCase);

            string blockName = xmlReader.Name;
            CodeTemplate template = new CodeTemplate(blockName, attributes);
            if (xmlReader.HasAttributes)
            {
                for (int attributeNumber = 0; attributeNumber < xmlReader.AttributeCount; ++attributeNumber)
                {
                    xmlReader.MoveToAttribute(attributeNumber);
                    attributes.Add(xmlReader.Name, xmlReader.Value);
                }

                xmlReader.MoveToElement(); 
            }
            if (xmlReader.IsEmptyElement) return template;

            for (bool goForward = true; goForward && (goForward = xmlReader.Read());)
            {
                switch (xmlReader.NodeType)
                {
                    case XmlNodeType.Attribute:
                        attributes.Add(xmlReader.Name, xmlReader.Value);
                        break;
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

                        goForward = false;
                        break;
                }
            }

            return template;
        }
    }
}