using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Coding4fun.DataTools.Analyzers.Template
{
    public class CodeTemplate
    {
        public enum Attribute
        {
            Case,
            WordSeparator
        }
        
        internal CodeTemplate(string? name, Dictionary<string, string> attributes)
        {
            Name = name;
            Children = Array.Empty<CodeTemplate>();
            Attributes = attributes;
        }

        internal CodeTemplate(CodeTemplate? parent, string text)
        {
            Text = text;
            Parent = parent;
            Children = Array.Empty<CodeTemplate>();
            Attributes = new Dictionary<string, string>();
        }

        public string? Name { get; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        [ExcludeFromCodeCoverage]
        public CodeTemplate? Parent { get; set; }
        public CodeTemplate[] Children { get; internal set; }
        private string? Text { get; }
        public IReadOnlyDictionary<string, string> Attributes { get; }

        public void BuildCode(StringBuilder sharpSb, ResolverBase resolver)
        {
            ResolverContext context = new ();
            BuildCode(sharpSb, resolver, context);
        }

        private void BuildCode(StringBuilder sharpSb, ResolverBase resolver, ResolverContext context)
        {
            if (Text != null)
            {
                sharpSb.Append(Text);
                return;
            }

            if (resolver.TryReplaceTemplate(this, out CodeTemplate? newCodeTemplate))
            {
                newCodeTemplate.BuildCode(sharpSb, resolver, context);
                return;
            }

            object?[]? resolvedObjects = resolver.Resolve(this, context);
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
                    if (rObject != null) context.Add(new EnumerableItem(rObject, position, resolvedObjects.Length));
                    foreach (CodeTemplate? childTemplate in Children)
                    {
                        childTemplate.BuildCode(sharpSb, resolver, context);
                    }

                    if (rObject != null) context.RemoveLast();
                }
            }
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override string ToString() => Text ?? $"<{Name} />";
    }
}