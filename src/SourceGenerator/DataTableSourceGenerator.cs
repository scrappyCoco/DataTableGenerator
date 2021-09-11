using System;
using System.Collections.Generic;
using System.Linq;
using Coding4fun.DataTableGenerator.Common;
using Coding4fun.DataTableGenerator.Common.Extension;
using Coding4fun.DataTableGenerator.SourceGenerator.Extension;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Coding4fun.DataTableGenerator.SourceGenerator
{
    [Generator]
    public class DataTableSourceGenerator : ISourceGenerator
    {
        /// <inheritdoc />
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new MySyntaxReceiver());
        }

        /// <inheritdoc />
        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not MySyntaxReceiver contextSyntaxReceiver) return;
            if (contextSyntaxReceiver.DiagnosticError != null)
                context.ReportDiagnostic(contextSyntaxReceiver.DiagnosticError);

            try
            {
                if (contextSyntaxReceiver.UsingDirectives == null)
                    throw new InvalidOperationException("Unable to find using directives.");

                if (contextSyntaxReceiver.TableDescription == null)
                    throw new InvalidOperationException("Unable to find table description.");

                List<string> usingDirectives = new();
                usingDirectives.AddRange(contextSyntaxReceiver.UsingDirectives);
                usingDirectives.AddRange(new[]
                {
                    "using System.Collections.Generic;",
                    "using System.Data;"
                });

                string[] dataTableDefinitions = contextSyntaxReceiver.TableDescription.GetDataTableNames()
                    .Select(dtName => "public DataTable " + dtName + ";\n".AddSpacesAfterLn(8))
                    .ToArray();

                string[] cleanDataTables = contextSyntaxReceiver.TableDescription.GetDataTableNames()
                    .Select(dtName => dtName + ".Clear();\n".AddSpacesAfterLn(12))
                    .ToArray();

                string template =
                    $@"{string.Join(Environment.NewLine, new HashSet<string>(usingDirectives.OrderBy(it => it)))}

namespace {contextSyntaxReceiver.Namespace}
{{
    public partial class {contextSyntaxReceiver.SqlMappingClassName}
    {{
        {string.Join("", dataTableDefinitions)}

        public {contextSyntaxReceiver.SqlMappingClassName}()
        {{
            {contextSyntaxReceiver.TableDescription.GetSharpDataTableDefinitions().AddSpacesAfterLn(12)}
        }}

        public string GetSqlTableDefinition() => @""
{contextSyntaxReceiver.TableDescription.GetSqlTableDefinitions()}
"";

        public void FillDataTables(IEnumerable<{contextSyntaxReceiver.TableDescription.ClassName}> items)
        {{
            {string.Join("", cleanDataTables)}

            foreach (var item in items)
            {{
                Add{contextSyntaxReceiver.TableDescription.SqlTableName.Replace("#", "")}(item);
            }}
        }}

        {contextSyntaxReceiver.TableDescription.GetMethodDefinitions().AddSpacesAfterLn(8)}
    }}
}}
";
                context.AddSource($"{contextSyntaxReceiver.SqlMappingClassName ?? "NotFound"}.Generated.cs", template);
            }
            catch (Exception exception)
            {
                context.ReportDiagnostic(CreateError(exception.Message));
            }
        }

        private static Diagnostic CreateError(string message, Location? location = null)
        {
            var diagnosticDescriptor = new DiagnosticDescriptor("Coding4fun.DataTableGenerator", "Error", message,
                "Generator", DiagnosticSeverity.Error, true);
            return Diagnostic.Create(diagnosticDescriptor, location == null ? Location.None : location);
        }

        private class MySyntaxReceiver : ISyntaxReceiver
        {
            private static readonly string DataTableBuilderName = typeof(DataTableBuilder<int>).GetNameWithoutGeneric();
            public string? Namespace { get; private set; }
            public string? SqlMappingClassName { get; private set; }

            public TableDescription? TableDescription { get; private set; }

            public string[]? UsingDirectives { get; private set; }
            public Diagnostic? DiagnosticError { get; private set; }

            /// <inheritdoc />
            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                try
                {
                    // public partial class PersonSqlMapping
                    // {
                    //     public PersonSqlMapping() { ... }
                    //     ^                      ^

                    if (syntaxNode is not ConstructorDeclarationSyntax sqlMappingConstructor ||
                        !sqlMappingConstructor.Identifier.Text.EndsWith("SqlMapping")
                        || sqlMappingConstructor.Body == null)
                        return;

                    // new DataTableBuilder<Person>("#PERSON")...
                    // ^                                     ^
                    var dtBuilderCreationSyntax = sqlMappingConstructor
                        .DescendantNodes()
                        .IsInstanceOf<ObjectCreationExpressionSyntax>()
                        .FirstOrDefault();

                    // new DataTableBuilder<Person>("#PERSON")...
                    //     ^              ^
                    if (dtBuilderCreationSyntax == null ||
                        dtBuilderCreationSyntax.Type is not GenericNameSyntax genericNameSyntax ||
                        genericNameSyntax.Identifier.Text != DataTableBuilderName)
                        return;

                    // new DataTableBuilder<Person>("#PERSON")...
                    //                              ^       ^
                    var firstArgument = dtBuilderCreationSyntax.ArgumentList?.Arguments.FirstOrDefault();
                    var tableName = (firstArgument?.Expression as LiteralExpressionSyntax)?.Token.ValueText;

                    if (tableName == null) return;

                    // new DataTableBuilder<Person>("#PERSON")...
                    //                     ^      ^
                    var firstTypeArgument = genericNameSyntax.TypeArgumentList.Arguments.FirstOrDefault();
                    if (firstTypeArgument == null) return;

                    TableDescription = new TableDescription(tableName, firstTypeArgument.ToString());

                    // ..
                    //   .AddColumn(...)
                    //   .AddColumn(...)
                    //   .AddSubTable(...)
                    // ...
                    var invocationExpressions = dtBuilderCreationSyntax
                        .Ancestors()
                        .Where(node => node is InvocationExpressionSyntax)
                        .Cast<InvocationExpressionSyntax>();


                    ParseInvocationExpressions(invocationExpressions, TableDescription);

                    UsingDirectives = syntaxNode.Ancestors().IsInstanceOf<CompilationUnitSyntax>()
                        .First()
                        .ChildNodes()
                        .IsInstanceOf<UsingDirectiveSyntax>()
                        .Select(u => u.ToString())
                        .ToArray();

                    SqlMappingClassName = sqlMappingConstructor.Identifier.Text;

                    var namespaceDeclarationSyntax = syntaxNode.Ancestors()
                        .IsInstanceOf<NamespaceDeclarationSyntax>()
                        .FirstOrDefault();

                    if (namespaceDeclarationSyntax == null)
                        throw new InvalidOperationException("Unable to find namespace");
                    Namespace = namespaceDeclarationSyntax.Name.GetText().ToString();
                }
                catch (Exception exception)
                {
                    DiagnosticError = CreateError(exception.Message,
                        Location.Create(syntaxNode.SyntaxTree, syntaxNode.FullSpan));
                }
            }

            private static ColumnDescription ParseAddColumn(InvocationExpressionSyntax invocationExpression)
            {
                string? columnName = null;
                string? columnType = null;
                string? varName = null;
                string? expressionBody = null;

                //
                // ...AddColumn("AGE", "SMALLINT", p => p.Age)
                //
                for (var argNumber = 0; argNumber < invocationExpression.ArgumentList.Arguments.Count; argNumber++)
                {
                    ArgumentSyntax argumentListArgument = invocationExpression.ArgumentList.Arguments[argNumber];
                    switch (argNumber)
                    {
                        case 0:
                            columnName = ((LiteralExpressionSyntax)argumentListArgument.Expression).Token.ValueText;
                            break;
                        case 1:
                            columnType = ((LiteralExpressionSyntax)argumentListArgument.Expression).Token.ValueText;
                            break;
                        case 2:
                        {
                            var lambdaExpressionSyntax =
                                argumentListArgument.Expression as SimpleLambdaExpressionSyntax;
                            varName = lambdaExpressionSyntax?.Parameter.ToString();
                            expressionBody = lambdaExpressionSyntax?.ExpressionBody?.ToString();
                            break;
                        }
                    }
                }

                if (columnName == null || columnType == null || expressionBody == null || varName == null)
                    throw new InvalidOperationException($"Unable to parse: {invocationExpression}");

                ColumnDescription columnDescription = new(columnName, columnType, expressionBody, varName);

                return columnDescription;
            }

            private static TableDescription ParseAddSubTable(InvocationExpressionSyntax invocationExpression)
            {
                string? subTableName = null;
                string? expressionBody = null;
                string? varName = null;

                //
                // .AddSubTable<Job>("#JOB_HISTORY", p => p.Jobs, jobBuilder => jobBuilder
                //              .AddColumn("COMPANY_NAME", "VARCHAR(100)", j => j.CompanyName)
                //              .AddColumn("ADDRESS", "VARCHAR(200)", j => j.Address)
                //          )
                //

                var memberAccessExpression = invocationExpression.Expression as MemberAccessExpressionSyntax;
                var genericName = memberAccessExpression?.Name as GenericNameSyntax;
                var genericNameText = genericName?.TypeArgumentList.Arguments.FirstOrDefault()?.ToString();

                for (var argNumber = 0; argNumber < invocationExpression.ArgumentList.Arguments.Count; argNumber++)
                {
                    ArgumentSyntax argumentListArgument = invocationExpression.ArgumentList.Arguments[argNumber];
                    switch (argNumber)
                    {
                        case 0:
                            subTableName = ((LiteralExpressionSyntax)argumentListArgument.Expression).Token.ValueText;
                            break;
                        case 1:
                        {
                            var lambdaExpressionSyntax = argumentListArgument.Expression as SimpleLambdaExpressionSyntax;
                            expressionBody = lambdaExpressionSyntax?.ExpressionBody?.ToString();
                            break;
                        }
                        case 2:
                        {
                            var lambdaExpressionSyntax = argumentListArgument.Expression as SimpleLambdaExpressionSyntax;
                            varName = lambdaExpressionSyntax?.Parameter.ToString();
                            break;
                        }
                    }
                }

                if (genericNameText == null)
                    throw new InvalidOperationException($"Unable to find generic name in: {invocationExpression}");

                if (subTableName == null)
                    throw new InvalidOperationException($"Unable to find sub-table name in: {invocationExpression}");

                TableDescription subTableDescription = new(subTableName, genericNameText)
                {
                    EnumerableName = expressionBody,
                    VarName = varName
                };

                return subTableDescription;
            }

            private static TableDescription ParseAddBasicSubTable(InvocationExpressionSyntax invocationExpression)
            {
                string? subTableName = null;
                string? expressionBody = null;
                string? varName = null;
                string? columnName = null;
                string? columnType = null;

                //
                // .AddBasicSubTable<string>("#SKILL", "SKILL", "VARCHAR(100)", p => p.Skills)
                // 

                var memberAccessExpression = invocationExpression.Expression as MemberAccessExpressionSyntax;
                var genericName = memberAccessExpression?.Name as GenericNameSyntax;
                var genericNameText = genericName?.TypeArgumentList.Arguments.FirstOrDefault()?.ToString();

                for (var argNumber = 0; argNumber < invocationExpression.ArgumentList.Arguments.Count; argNumber++)
                {
                    ArgumentSyntax argumentListArgument = invocationExpression.ArgumentList.Arguments[argNumber];
                    if (argNumber == 0)
                    {
                        subTableName = ((LiteralExpressionSyntax)argumentListArgument.Expression).Token.ValueText;
                    }
                    else if (argNumber == 1)
                    {
                        columnName = ((LiteralExpressionSyntax)argumentListArgument.Expression).Token.ValueText;
                    }
                    else if (argNumber == 2)
                    {
                        columnType = ((LiteralExpressionSyntax)argumentListArgument.Expression).Token.ValueText;
                    }
                    else if (argNumber == 3)
                    {
                        var lambdaExpressionSyntax = argumentListArgument.Expression as SimpleLambdaExpressionSyntax;
                        varName = lambdaExpressionSyntax?.Parameter.ToString();
                        expressionBody = lambdaExpressionSyntax?.ExpressionBody?.ToString();
                    }
                }

                if (columnName == null || columnType == null || varName == null || subTableName == null)
                    throw new InvalidOperationException($"Unable to parse: {invocationExpression}");

                ColumnDescription columnDescription = new(columnName, columnType, null, varName);

                TableDescription subTableDescription = new(subTableName, genericNameText)
                {
                    EnumerableName = expressionBody,
                    VarName = varName,
                    Columns = { columnDescription }
                };

                return subTableDescription;
            }

            private static void ParseInvocationExpressions(
                IEnumerable<InvocationExpressionSyntax> invocationExpressions,
                TableDescription tableDescription)
            {
                foreach (var invocationExpression in invocationExpressions)
                {
                    if (invocationExpression.Expression is not MemberAccessExpressionSyntax
                        memberAccessExpressionSyntax) continue;

                    string methodName = memberAccessExpressionSyntax.Name.Identifier.Text;

                    if (methodName == nameof(DataTableBuilder<int>.AddColumn))
                    {
                        var columnDescription = ParseAddColumn(invocationExpression);
                        tableDescription.VarName = columnDescription.VarName;
                        tableDescription.Columns.Add(columnDescription);
                    }
                    else if (methodName == nameof(DataTableBuilder<int>.AddSubTable))
                    {
                        var subTableDescription = ParseAddSubTable(invocationExpression);
                        tableDescription.SubTables.Add(subTableDescription);

                        var simpleLambdaExpression =
                            invocationExpression.ArgumentList.Arguments[2].Expression as SimpleLambdaExpressionSyntax;

                        if (simpleLambdaExpression?.ExpressionBody == null)
                            throw new InvalidOperationException($"Unable to find lambda expression in {methodName}");

                        var subTableInvocationExpressions = simpleLambdaExpression.ExpressionBody
                            .DescendantNodes()
                            .Where(node => node is InvocationExpressionSyntax)
                            .Cast<InvocationExpressionSyntax>();

                        ParseInvocationExpressions(subTableInvocationExpressions, subTableDescription);
                    }
                    else if (methodName == nameof(DataTableBuilder<int>.AddBasicSubTable))
                    {
                        var basicSubTable = ParseAddBasicSubTable(invocationExpression);
                        tableDescription.SubTables.Add(basicSubTable);
                    }
                }
            }
        }
    }
}