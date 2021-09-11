using System;
using System.Collections.Generic;
using System.Linq;
using Coding4fun.DataTableGenerator.Common;
using Coding4fun.DataTableGenerator.Common.Extension;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SourceGenerator.Extension;

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
 
            try
            {
                List<string> usingDirectives = new();
                usingDirectives.AddRange(contextSyntaxReceiver.UsingDirectives);
                usingDirectives.AddRange(new[]
                {
                    "using System.Collections.Generic;",
                    "using System.Data;"
                });

                string[] dataTableNames = contextSyntaxReceiver.TableDescription.GetDataTableNames()
                    .Select(dtName => "public DataTable " + dtName + ";\n".AddSpacesAfterLn(8))
                    .ToArray();

                string template = $@"{string.Join(Environment.NewLine, new HashSet<string>(usingDirectives.OrderBy(it => it)))}

namespace {contextSyntaxReceiver.Namespace}
{{
    public partial class {contextSyntaxReceiver.SqlMappingClassName}
    {{
        {string.Join("", dataTableNames)}

        public string GetSqlTableDefinition() => @""
{contextSyntaxReceiver.TableDescription.GetSqlTableDefinitions()}
"";

        public void FillDataTables(IEnumerable<{contextSyntaxReceiver.TableDescription.ClassName}> items)
        {{
            {contextSyntaxReceiver.TableDescription.GetSharpDataTableDefinitions().AddSpacesAfterLn(12)}

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
                var diagnosticDescriptor = new DiagnosticDescriptor("c4f.id", "Error", "Message", "Generator", DiagnosticSeverity.Error, true);
                var diagnostic = Diagnostic.Create(diagnosticDescriptor, Location.None);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private class MySyntaxReceiver : ISyntaxReceiver
        {
            private static readonly string DataTableBuilderName = typeof(DataTableBuilder<int>).GetNameWithoutGeneric();
            public string? Namespace { get; private set; }
            public string? SqlMappingClassName { get; private set; }
            public SourceText? SourceText { get; private set; }

            public TableDescription TableDescription { get; private set; }

            public string[] UsingDirectives { get; private set; }

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
                    {
                        return;
                    }

                    // new DataTableBuilder<Person>("#PERSON")...
                    // ^                                     ^
                    ObjectCreationExpressionSyntax? dtBuilderCreationSyntax = sqlMappingConstructor
                        .DescendantNodes()
                        .IsInstanceOf<ObjectCreationExpressionSyntax>()
                        .FirstOrDefault();

                    // new DataTableBuilder<Person>("#PERSON")...
                    //     ^              ^
                    if (dtBuilderCreationSyntax == null ||
                        dtBuilderCreationSyntax.Type is not GenericNameSyntax genericNameSyntax ||
                        genericNameSyntax.Identifier.Text != DataTableBuilderName)
                    {
                        return;
                    }

                    // new DataTableBuilder<Person>("#PERSON")...
                    //                              ^       ^
                    ArgumentSyntax? firstArgument = dtBuilderCreationSyntax.ArgumentList?.Arguments.FirstOrDefault();
                    string? tableName = (firstArgument?.Expression as LiteralExpressionSyntax)?.Token.ValueText;

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
                    SourceText = sqlMappingConstructor.Body.GetText();
                    NamespaceDeclarationSyntax? namespaceDeclarationSyntax = syntaxNode.Ancestors()
                        .IsInstanceOf<NamespaceDeclarationSyntax>()
                        .FirstOrDefault();

                    if (namespaceDeclarationSyntax == null)
                        throw new InvalidOperationException("Unable to find namespace");
                    Namespace = namespaceDeclarationSyntax.Name.GetText().ToString();
                }
                catch (Exception exception)
                {
                    
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
                {
                    throw new InvalidOperationException($"Unable to parse: {invocationExpression}");
                }

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
                string? genericNameText = genericName?.TypeArgumentList.Arguments.FirstOrDefault()?.ToString();

                for (var argNumber = 0; argNumber < invocationExpression.ArgumentList.Arguments.Count; argNumber++)
                {
                    ArgumentSyntax argumentListArgument = invocationExpression.ArgumentList.Arguments[argNumber];
                    if (argNumber == 0)
                    {
                        subTableName = ((LiteralExpressionSyntax)argumentListArgument.Expression).Token.ValueText;
                    }
                    else if (argNumber == 1)
                    {
                        var lambdaExpressionSyntax = argumentListArgument.Expression as SimpleLambdaExpressionSyntax;
                        expressionBody = lambdaExpressionSyntax?.ExpressionBody?.ToString();
                    }
                    else if (argNumber == 2)
                    {
                        var lambdaExpressionSyntax = argumentListArgument.Expression as SimpleLambdaExpressionSyntax;
                        varName = lambdaExpressionSyntax?.Parameter.ToString();
                        var expressionBody1 = lambdaExpressionSyntax?.ExpressionBody;
                    }
                }

                if (genericNameText == null)
                {
                    throw new InvalidOperationException($"Unable to find generic name in: {invocationExpression}");
                }

                if (subTableName == null)
                {
                    throw new InvalidOperationException($"Unable to find sub-table name in: {invocationExpression}");
                }

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
                string? genericNameText = genericName?.TypeArgumentList.Arguments.FirstOrDefault()?.ToString();

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