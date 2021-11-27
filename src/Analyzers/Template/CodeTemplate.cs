using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Coding4fun.DataTools.Analyzers.Template
{
    public class CodeTemplate
    {
        internal CodeTemplate(string? name)
        {
            Name = name;
            Children = Array.Empty<CodeTemplate>();
        }

        internal CodeTemplate(CodeTemplate? parent, string text)
        {
            Text = text;
            Parent = parent;
            Children = Array.Empty<CodeTemplate>();
        }

        public string? Name { get; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        [ExcludeFromCodeCoverage]
        public CodeTemplate? Parent { get; set; }
        public CodeTemplate[] Children { get; internal set; }
        private string? Text { get; }

        public void BuildCode(StringBuilder sharpSb, ResolverBase resolver)
        {
            LinkedList<EnumerableItem> contextObjects = new ();
            BuildCode(sharpSb, resolver, contextObjects);
        }

        private void BuildCode(StringBuilder sharpSb, ResolverBase resolver, LinkedList<EnumerableItem> contextObjects)
        {
            if (Text != null)
            {
                sharpSb.Append(Text);
                return;
            }

            if (resolver.TryReplaceTemplate(this, out CodeTemplate? newCodeTemplate, contextObjects))
            {
                newCodeTemplate.BuildCode(sharpSb, resolver, contextObjects);
                return;
            }

            object?[]? resolvedObjects = resolver.Resolve(this, contextObjects);
            if ((resolvedObjects?.Length ?? 0) == 0) return;

            if (resolvedObjects!.Length == 1 && Children.Length == 0 && resolvedObjects[0] != null)
            {
                sharpSb.Append(resolvedObjects[0]);
            }
            else
            {
                for (int position = 0; position < resolvedObjects.Length; position++)
                {
                    object? rObject = resolvedObjects[position];
                    if (rObject != null) contextObjects.AddLast(new EnumerableItem(rObject, position, resolvedObjects.Length));
                    foreach (CodeTemplate? childTemplate in Children)
                    {
                        childTemplate.BuildCode(sharpSb, resolver, contextObjects);
                    }

                    if (rObject != null) contextObjects.RemoveLast();
                }
            }
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override string ToString() => Text ?? $"<{Name} />";
    }
}