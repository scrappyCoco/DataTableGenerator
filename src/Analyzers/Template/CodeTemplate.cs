using System;
using System.Collections.Generic;
using System.Text;

namespace Coding4fun.DataTools.Analyzers.Template
{
    public class CodeTemplate
    {
        internal CodeTemplate(string? name, CodeTemplate? parent, CodeTemplate[]? children = null)
        {
            Name = name;
            Parent = parent;
            Children = children ?? Array.Empty<CodeTemplate>();
            foreach (CodeTemplate child in Children) child.Parent = this;
        }

        internal CodeTemplate(CodeTemplate? parent, string text)
        {
            Text = text;
            Parent = parent;
            Children = Array.Empty<CodeTemplate>();
        }

        public string? Name { get; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public CodeTemplate? Parent { get; set; }
        public CodeTemplate[] Children { get; internal set; }
        private string? Text { get; }

        public void BuildCode(StringBuilder sharpSb, ResolverBase resolver)
        {
            LinkedList<object> contextObjects = new ();
            BuildCode(sharpSb, resolver, contextObjects);
        }

        private void BuildCode(StringBuilder sharpSb, ResolverBase resolver, LinkedList<object> contextObjects)
        {
            if (Text != null)
            {
                sharpSb.Append(Text);
                return;
            }

            if (resolver.TryReplaceTemplate(this, out ResolverBase? newResolver, contextObjects))
            {
                CodeTemplate newTemplate = newResolver!.GetTemplate();
                newTemplate.BuildCode(sharpSb, newResolver, contextObjects);
                return;
            }

            object?[]? resolvedObjects = resolver.Resolve(this, contextObjects);
            if ((resolvedObjects?.Length ?? 0) == 0) return;

            if (resolvedObjects!.Length == 1 && Children.Length == 0) sharpSb.Append(resolvedObjects[0]);
            else
            {
                foreach (object? rObject in resolvedObjects)
                {
                    if (rObject != null) contextObjects.AddLast(rObject);
                    foreach (CodeTemplate? childTemplate in Children)
                    {
                        childTemplate.BuildCode(sharpSb, resolver, contextObjects);
                    }
                    if (rObject != null) contextObjects.RemoveLast();
                }
            }
        }

        /// <inheritdoc />
        public override string ToString() => $"{Name ?? Text}";
    }

}