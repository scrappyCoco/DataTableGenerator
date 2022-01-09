using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Coding4fun.DataTools.Analyzers.Extension;
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

            // @formatter:off
            return template.Name switch
            {
                nameof(TableDescription.VarName)                     => context.GetValue<TableDescription>(t => t.VarName),
                nameof(TableDescription.EntityName)                  => context.GetValue<TableDescription>(t => t.EntityName),
                nameof(TableDescription.ClassName)                   => context.Objects.Any()
                                                                        ? context.GetValue<TableDescription>(t => t.ClassName)
                                                                        : _rootTableDescriptions.First().Cast<TableDescription>().ClassName.ToArrayOfObject(),
                nameof(TableDescription.EnumerableName)              => context.GetValue<TableDescription>(t => t.EnumerableName ?? "items"),
                nameof(TableDescription.DataTableName)               => context.GetValue<TableDescription>(t => t.DataTableName),
                nameof(TableDescription.SqlTableName)                => context.GetValue<TableDescription>(t => t.SqlTableName),
                nameof(TableDescription.SubTables)                   => context.GetArray<TableDescription, TableDescription>(t => t.SubTables),
                nameof(TableDescription.Columns)                     => context.GetArray<TableDescription, ColumnDescription>(t => t.Columns),
                "ParentTableVarName"                                 => context.GetValue<TableDescription>(t => (t.ParentTable ?? throw new NullReferenceException($"{nameof(t.ParentTable)} is null.")).VarName),
                "Has" + nameof(TableDescription.PreExecutionActions) => context.GetBool<TableDescription>(t => t.PreExecutionActions.Any()),
                nameof(TableDescription.PreExecutionActions)         => context.GetArray<TableDescription, string>(t => t.PreExecutionActions),
                nameof(ColumnDescription.SharpType)                  => context.GetValue<ColumnDescription>(c => c.SharpType),
                nameof(ColumnDescription.SqlType)                    => context.GetValue<ColumnDescription>(c => c.SqlType),
                nameof(ColumnDescription.ValueBody)                  => context.GetValue<ColumnDescription>(c => c.ValueBody),
                nameof(ColumnDescription.SqlColumnName)              => context.GetValue<ColumnDescription>(c => c.SqlColumnName),
                "Offset"                                             => context.GetTableOffset(),
                "RootClass"                                          => _rootTableDescriptions,
                "Root"                                               => context.SingleNullObjects,
                "Comma"                                              => context.GetComma(),
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