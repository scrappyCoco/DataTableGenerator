using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Coding4fun.DataTools.Analyzers.Extension;
using Coding4fun.DataTools.Analyzers.StringUtil;
using Microsoft.CodeAnalysis;

namespace Coding4fun.DataTools.Analyzers.Template
{
    public class DataTableResolver : ResolverBase
    {
        private readonly object?[] _rootTableDescriptions;

        public DataTableResolver(
            TableDescription tableDescription,
            string resourcePath,
            CancellationToken cancellationToken,
            IEnumerable<AdditionalText>? additionalTexts = null) : base(resourcePath, cancellationToken, additionalTexts)
        {
            _rootTableDescriptions = new object?[] { tableDescription };
        }

        public Dictionary<string, Func<ResolverContext, object?[]>> CustomResolvers { get; } = new();

        /// <inheritdoc />
        public override object?[] Resolve(CodeTemplate template, ResolverContext context)
        {
            if (CustomResolvers.TryGetValue(template.Name!, out Func<ResolverContext, object?[]>? customerResolver))
            {
                return customerResolver.Invoke(context);
            }
            
            string caseAttributeName = CodeTemplate.Attribute.Case.ToString();
            string wordSeparatorAttributeName = CodeTemplate.Attribute.WordSeparator.ToString();
            object? ApplyAttributes(object? obj)
            {
                if (obj is not string str) return null;
                if (template.Attributes.TryGetValue(caseAttributeName, out string stringCaseRuleName))
                {
                    string wordSeparator = template.Attributes.TryGetValue(wordSeparatorAttributeName, out wordSeparator)
                        ? wordSeparator
                        : "";

                    return stringCaseRuleName switch
                    {
                        nameof(CaseRules.ToCamelCase) => str.ChangeCase(CaseRules.ToCamelCase, wordSeparator),
                        nameof(CaseRules.ToTitleCase) => str.ChangeCase(CaseRules.ToTitleCase, wordSeparator),
                        nameof(CaseRules.ToLowerCase) => str.ChangeCase(CaseRules.ToLowerCase, wordSeparator),
                        nameof(CaseRules.ToUpperCase) => str.ChangeCase(CaseRules.ToUpperCase, wordSeparator),
                        _ => throw new InvalidOperationException(
                            $"Unable to find handler for the attribute {caseAttributeName}: {stringCaseRuleName}.")
                    };
                }

                return str;
            }

            // @formatter:off
            return template.Name switch
            {
                nameof(TableDescription.ClassName)                   => context.Objects.Any()
                                                                        ? context.GetValue<TableDescription>(t => ApplyAttributes(t.ClassName))
                                                                        : _rootTableDescriptions.First().Cast<TableDescription>().ClassName.Let(ApplyAttributes).ToArrayOfObject(),
                nameof(TableDescription.EnumerableName)              => context.GetValue<TableDescription>(t => (t.EnumerableName ?? "items").Let(ApplyAttributes)),
                nameof(TableDescription.SubTables)                   => context.GetArray<TableDescription, TableDescription>(t => t.SubTables),
                nameof(TableDescription.Columns)                     => context.GetArray<TableDescription, ColumnDescription>(t => t.Columns),
                "ParentTable" + nameof(TableDescription.ClassName)   => context.GetValue<TableDescription>(t => ApplyAttributes((t.ParentTable ?? throw new NullReferenceException($"{nameof(t.ParentTable)} is null.")).ClassName)),
                "Has" + nameof(TableDescription.PreExecutionActions) => context.GetBool<TableDescription>(t => t.PreExecutionActions.Any()),
                nameof(TableDescription.PreExecutionActions)         => context.GetArray<TableDescription, string>(t => t.PreExecutionActions),
                nameof(ColumnDescription.SharpType)                  => context.GetValue<ColumnDescription>(c => c.SharpType.Let(ApplyAttributes)),
                nameof(ColumnDescription.SqlType)                    => context.GetValue<ColumnDescription>(c => c.SqlType.Let(ApplyAttributes)),
                nameof(ColumnDescription.ValueBody)                  => context.GetValue<ColumnDescription>(c => c.ValueBody.Let(ApplyAttributes)),
                nameof(ColumnDescription.PropertyName)               => context.GetValue<ColumnDescription>(c => c.PropertyName.Let(ApplyAttributes)),
                "IsLastColumn"                                       => context.GetEnumerableBool(c => c.IsLast),
                "IsNotLastColumn"                                    => context.GetEnumerableBool(c => !c.IsLast),
                "HasAttribute"                                       => context.GetBool<IAttributeHolder>(t =>
                                                                        {
                                                                            string templateAttributeName = template.Attributes["name"];
                                                                            return t.Attributes.ContainsKey(templateAttributeName);
                                                                        }),
                "HasNotAttribute"                                    => context.GetBool<IAttributeHolder>(t =>
                                                                        {
                                                                            string templateAttributeName = template.Attributes["name"];
                                                                            return !t.Attributes.ContainsKey(templateAttributeName);
                                                                        }),
                "Attribute"                                          => context.GetValue<IAttributeHolder>(t =>
                                                                        {
                                                                            string templateAttributeName = template.Attributes["name"];
                                                                            return t.Attributes[templateAttributeName].Let(ApplyAttributes);
                                                                        }),
                "Offset"                                             => context.GetTableOffset(),
                "RootClass"                                          => _rootTableDescriptions,
                "Root"                                               => context.SingleNullObjects,
                _                                                    => throw new InvalidOperationException($"Unable to resolve <{template.Name} />")
            };
            // @formatter:on
        }

        /// <inheritdoc />
        public override bool TryReplaceTemplate(CodeTemplate currentTemplate,
            [NotNullWhen(true)] out CodeTemplate? newCodeTemplate)
        {
            if (CodeTemplates.TryGetValue(currentTemplate.Name!, out newCodeTemplate)) return true;
            newCodeTemplate = null;
            return false;
        }
    }
}