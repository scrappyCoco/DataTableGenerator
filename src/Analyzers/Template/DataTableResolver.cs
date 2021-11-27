using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Coding4fun.DataTools.Analyzers.Extension;

namespace Coding4fun.DataTools.Analyzers.Template
{
    internal class DataTableResolver : ResolverBase
    {
        private readonly object?[] _rootTableDescriptions;
        private readonly object?[] _ns;
        private readonly object?[] _sqlMappingClassName;
        private readonly object[] _usingNamespaces;
        private readonly object?[] _singleNullObjects;
        private readonly object?[] _emptyObjects;
        private readonly object?[] _commaObjects;

        private DataTableResolver(TableDescription tableDescription,
            string @namespace,
            string sqlMappingClassName,
            string usingNamespaces): base("Coding4fun.DataTools.Analyzers.Template.DataTable")
        {
            _rootTableDescriptions = new object?[] { tableDescription };
            _ns = new object?[] { @namespace };
            _sqlMappingClassName = new object?[] { sqlMappingClassName };
            _singleNullObjects = new object?[] { null };
            _commaObjects = new object?[] { "," };
            _emptyObjects = new object?[] { };
            _usingNamespaces = usingNamespaces.Cast<object>().ToArray();
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public override object?[] Resolve(CodeTemplate template, IEnumerable<EnumerableItem> contextObjects)
        {
            object?[] GetValue<T>(Func<T, object?> valueGetter)
            {
                T requiredObject = contextObjects.Reverse()
                    .Select(t => t.Value)
                    .OfType<T>()
                    .First()!;
                
                return requiredObject.Let(it => (valueGetter.Invoke(it) ?? it).ToArrayOfObject());
            }

            object?[] GetBool<T>(Func<T, bool> predicate) => contextObjects.Last()
                .Value
                .Cast<T>()
                .Let(it => predicate.Invoke(it) ? _singleNullObjects : _emptyObjects);

            object?[] GetArray<TSource, TTarget>(Func<TSource, IEnumerable<TTarget>> enumerableGetter) => contextObjects
                .Last()
                .Value
                .Cast<TSource>()
                .Let(it => enumerableGetter.Invoke(it).Cast<object?>().ToArray());

            object?[] GetTableOffset()
            {
                 int offsetLength = (contextObjects
                     .Count(t => t.Value is TableDescription) + 2) * 4;
                 
                 return new object?[] { new string(' ', offsetLength) };
            }

            object?[] GetComma()
            {
                EnumerableItem lastItem = contextObjects.Last();
                return lastItem.Position + 1 == lastItem.Length ? _singleNullObjects : _commaObjects;
            }

            // @formatter:off
            return template.Name switch
            {
                nameof(TableDescription.VarName)                     => GetValue<TableDescription>(t => t.VarName),
                nameof(TableDescription.EntityName)                  => GetValue<TableDescription>(t => t.EntityName),
                nameof(TableDescription.ClassName)                   => contextObjects.Any()
                                                                        ? GetValue<TableDescription>(t => t.ClassName)
                                                                        : _rootTableDescriptions.First().Cast<TableDescription>().ClassName.ToArrayOfObject(),
                nameof(TableDescription.EnumerableName)              => GetValue<TableDescription>(t => t.EnumerableName ?? "items"),
                nameof(TableDescription.DataTableName)               => GetValue<TableDescription>(t => t.DataTableName),
                nameof(TableDescription.SqlTableName)                => GetValue<TableDescription>(t => t.SqlTableName),
                nameof(TableDescription.SubTables)                   => GetArray<TableDescription, TableDescription>(t => t.SubTables),
                nameof(TableDescription.Columns)                     => GetArray<TableDescription, ColumnDescription>(t => t.Columns),
                "ParentTableVarName"                                 => GetValue<TableDescription>(t => t.ParentTable.SqlTableName),
                "Has" + nameof(TableDescription.PreExecutionActions) => GetBool<TableDescription>(t => t.PreExecutionActions.Any()),
                nameof(TableDescription.PreExecutionActions)         => GetArray<TableDescription, string>(t => t.PreExecutionActions),
                nameof(ColumnDescription.SharpType)                  => GetValue<ColumnDescription>(c => c.SharpType),
                nameof(ColumnDescription.SqlType)                    => GetValue<ColumnDescription>(c => c.SqlType),
                nameof(ColumnDescription.ValueBody)                  => GetValue<ColumnDescription>(c => c.ValueBody),
                nameof(ColumnDescription.SqlColumnName)              => GetValue<ColumnDescription>(c => c.SqlColumnName),
                "Offset"                                             => GetTableOffset(),
                "RootClass"                                          => _rootTableDescriptions,
                "UsingNamespaces"                                    => _usingNamespaces,
                "UsedNamespace"                                      => GetValue<string>(t => t),
                "Namespace"                                          => _ns,
                "SqlMappingClassName"                                => _sqlMappingClassName,
                "Root"                                               => _singleNullObjects,
                "Comma"                                              => GetComma(),
                _                                                    => throw new InvalidOperationException(template.Name)
            };
            // @formatter:on
        }

        /// <inheritdoc />
        public override bool TryReplaceTemplate(CodeTemplate currentTemplate,
            [NotNullWhen(true)] out CodeTemplate? newCodeTemplate, LinkedList<EnumerableItem> contextObjects)
        {
            if (AvailableTemplateResources.Contains(currentTemplate.Name!))
            {
                newCodeTemplate = ReadTemplate(currentTemplate.Name!);
                return true;
            }

            newCodeTemplate = null;
            return false;
        }

        public static string BuildCode(TableDescription tableDescription)
        {
            DataTableResolver dataTableResolver = new (tableDescription, TODO, TODO, TODO);
            CodeTemplate rootTemplate = dataTableResolver.ReadTemplate("ClassDefinition");
            StringBuilder codeBuilder = new();
            rootTemplate.BuildCode(codeBuilder, dataTableResolver);
            return codeBuilder.ToString();
        }
    }
}