using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Coding4fun.DataTools.Analyzers.Extension;
using Coding4fun.DataTools.Analyzers.StringUtil;
using Coding4fun.DataTools.Analyzers.Template.DataTable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CaseRules = Coding4fun.DataTools.Analyzers.StringUtil.CaseRules;
using NamingConvention = Coding4fun.DataTools.Analyzers.StringUtil.NamingConvention;

namespace Coding4fun.DataTools.Analyzers
{
    [Generator]
    public class DataTableSourceGenerator : ISourceGenerator
    {
        private const string UnexpectedDiagnosticId = "C4FDT0001";
        private const string ErrorTitle = "Unable to generate source for DataTable";
        private const string ErrorCategory = "Code";
        
        private readonly string _tableBuilderName = typeof(TableBuilder<int>).GetNameWithoutGeneric();
        private NamingConvention _namingConvention = NamingConvention.ScreamingSnakeCase;
        private SyntaxNode? _nodeContext;

        /// <inheritdoc />
        public void Initialize(GeneratorInitializationContext context)
        {
        
        }

        /// <inheritdoc />
        public void Execute(GeneratorExecutionContext context)
        {
            if (context.Compilation.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                return;
            }
            
            foreach (SyntaxTree syntaxTree in context.Compilation.SyntaxTrees)
            {
                try
                {
                    SyntaxNode rootNode = syntaxTree.GetRoot();
                    MethodDeclarationSyntax[] methodDeclarations = rootNode
                        .DescendantNodes()
                        .OfType<MethodDeclarationSyntax>()
                        .ToArray();

                    foreach (MethodDeclarationSyntax methodDeclaration in methodDeclarations)
                    {
                        // [SqlMappingDeclaration]
                        //  ^                   ^
                        // private void Initialize()
                        // {
                        //    ...
                        // }

                        SetContext(methodDeclaration);

                        bool isSqlMappingDefinition = methodDeclaration.AttributeLists
                            .SelectMany(attributeList => attributeList.Attributes)
                            .Any(attribute => attribute.Name.ToString() == SqlMappingDeclarationAttribute.Name);

                        if (!isSqlMappingDefinition)
                        {
                            continue;
                        }

                        // new TableBuilder<Person>(NamingConvention.ScreamingSnakeCase)...
                        //     ^                  ^
                        GenericNameSyntax? genericName = methodDeclaration
                            .DescendantNodes()
                            .OfType<ObjectCreationExpressionSyntax>()
                            .Select(objectCreationExpression => objectCreationExpression.Type)
                            .OfType<GenericNameSyntax>()
                            .FirstOrDefault(genericName => genericName.Identifier.Text == _tableBuilderName);

                        SetContext(genericName, Messages.GetUnableToGetTableDefinition);

                        // new DataTableBuilder<Person>()...
                        //                     ^      ^
                        TypeSyntax rootType = genericName.TypeArgumentList.Arguments.First();
                        SetContext(rootType);

                        // new DataTableBuilder<Person>(NamingConvention.ScreamingSnakeCase)...
                        //                              ^                                 ^
                        string? namingConventionValue = genericName.Ancestors()
                                                            .OfType<ObjectCreationExpressionSyntax>()
                                                            .First()
                                                            .ArgumentList?.Arguments.FirstOrDefault()?.GetLastToken()
                                                            .Text
                                                        ?? NamingConvention.ScreamingSnakeCase.ToString();

                        _namingConvention = namingConventionValue.ParseEnum<NamingConvention>();

                        //_currentNode = genericName;
                        string sqlTableName = GetSqlTableName(rootType.ToString());
                        TableDescription tableDescription = new(rootType.ToString(), sqlTableName);

                        // ..
                        //   .AddColumn(...)
                        //   .AddColumn(...)
                        //   .AddSubTable(...)
                        // ...
                        InvocationExpressionSyntax[] invocationExpressions = genericName
                            .Ancestors()
                            .OfType<InvocationExpressionSyntax>()
                            .ToArray();

                        if (invocationExpressions.Length == 0)
                            Throw(Messages.GetSqlMappingIsEmpty());

                        SemanticModel semanticModel = context.Compilation.GetSemanticModel(syntaxTree);
                        
                        ParseInvocationExpressions(invocationExpressions, tableDescription, semanticModel);
                        
                        string[] usingDirectives = rootNode.DescendantNodes()
                            .OfType<UsingDirectiveSyntax>()
                            .Select(u => u.Name.ToString())
                            .ToArray();
                        
                        // public partial class PersonSqlMapping
                        //                      ^              ^
                        
                        ClassDeclarationSyntax classDeclaration =
                            rootType.Ancestors().OfType<ClassDeclarationSyntax>().First()!;
                        
                        // TODO: C# 10
                        string sqlMappingClassName = classDeclaration.Identifier.Text;
                        NamespaceDeclarationSyntax? namespaceDeclaration = genericName.Ancestors()
                            .OfType<NamespaceDeclarationSyntax>()
                            .FirstOrDefault();

                        string @namespace = namespaceDeclaration!.Name.GetText().ToString().Trim();
                        
                        List<string> usingNamespaces = new();
                        usingNamespaces.AddRange(usingDirectives);
                        usingNamespaces.AddRange(new[]
                        {
                            "System.Collections.Generic",
                            "System.Data",
                            "System.Data.SqlClient",
                            "System.Threading.Tasks"
                        });
                        
                        string sharpCode = ClassDefinitionResolver.GenerateDataTable(tableDescription, usingNamespaces, @namespace, sqlMappingClassName);
            
                        context.AddSource($"{sqlMappingClassName}.Generated.cs", sharpCode);
                    }
                }
                catch (Exception exception)
                {
                    Location codeLocation = Location.None;
                    string diagnosticId = UnexpectedDiagnosticId;
                   
                    if (exception is SourceGeneratorException sourceGeneratorException)
                    {
                        codeLocation = sourceGeneratorException.Location;
                        diagnosticId = sourceGeneratorException.DiagnosticId;
                    }
                    
                    var diagnosticDescriptor = new DiagnosticDescriptor(diagnosticId, ErrorTitle, exception.Message,
                        ErrorCategory, DiagnosticSeverity.Error, true);
                    
                    var diagnostic = Diagnostic.Create(diagnosticDescriptor, codeLocation);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private string ChangeSqlCase(string entityName)
        {
            string sqlTableName = _namingConvention switch
            {
                NamingConvention.CamelCase               => entityName.ChangeCase(CaseRules.ToCamelCase)!,
                NamingConvention.PascalCase              => entityName.ChangeCase(CaseRules.ToTitleCase)!,
                NamingConvention.SnakeCase               => entityName.ChangeCase(CaseRules.ToLowerCase, "_")!,
                NamingConvention.ScreamingSnakeCase or _ => entityName.ChangeCase(CaseRules.ToUpperCase, "_")!
            };

            return sqlTableName;
        }

        private string GetSqlTableName(string entityName) => "#" + ChangeSqlCase(entityName);

        private ColumnDescription ParseAddColumn(InvocationExpressionSyntax invocationExpression, SemanticModel semanticModel)
        {
            string? propertyName = null;
            string? columnName = null;
            string? columnType = null;
            string? expressionBody = null;
            IPropertySymbol? propertySymbol = null;

            //
            // ...AddColumn(person => person.CountryCode, "CHAR(2)", "COUNTRY_CODE")
            //
            for (int argNumber = 0; argNumber < invocationExpression.ArgumentList.Arguments.Count; argNumber++)
            {
                ArgumentSyntax argument = invocationExpression.ArgumentList.Arguments[argNumber];
                string? parameterName = argument.NameColon?.Name.Identifier.Text;

                // Unfortunately type name must be presented explicitly before parameter name in lambda expression.
                // Without it SemanticModel model unable to resolve type.
                if (argNumber == 0)
                {
                    // Person person => person.CountryCode
                    if (argument.Expression is SimpleLambdaExpressionSyntax)
                        Throw(Messages.GetLambdaWithoutType());
                    // (Person person) => person.CountryCode
                    else
                    {
                        // TODO: expression could be MethodInvocationExpression.
                        var parenthesizedLambdaExpression = (ParenthesizedLambdaExpressionSyntax)argument.Expression;
                        
                        ParameterSyntax parameter = parenthesizedLambdaExpression.ParameterList.Parameters.First();
                        if (parameter.Type == null)
                            Throw(Messages.GetLambdaWithoutType());
                        expressionBody = parenthesizedLambdaExpression.ExpressionBody!.ToString();

                        MemberAccessExpressionSyntax? memberAccessExpressionSyntax = parenthesizedLambdaExpression.ExpressionBody.DescendantNodesAndSelf()
                            .OfType<MemberAccessExpressionSyntax>()
                            .FirstOrDefault();
                        
                        SetContext(memberAccessExpressionSyntax, Messages.GetMemberAccessExpression);
                        
                        propertyName = columnName = parenthesizedLambdaExpression.ExpressionBody
                            .DescendantTokens()
                            .LastOrDefault(t => t.Kind() == SyntaxKind.IdentifierToken)
                            .ToString();
                        
                        propertySymbol = ModelExtensions.GetSymbolInfo(semanticModel, memberAccessExpressionSyntax).Symbol as IPropertySymbol;
                    }
                }
                else if (parameterName == "sqlType" || parameterName == null && argNumber == 1)
                {
                    columnType = ((LiteralExpressionSyntax)argument.Expression).Token.ValueText;
                }
                else if (parameterName == "columnName" || parameterName == null && argNumber == 2)
                {
                    columnName = ((LiteralExpressionSyntax)argument.Expression).Token.ValueText;
                }
            }

            if (propertySymbol == null)
                Throw(Messages.GetUnableToResolveProperty(propertyName!));
            columnType ??= MapSharpType2Sql(propertySymbol);
            string sharpType = propertySymbol.Type.ToString();

            columnName = ChangeSqlCase(columnName!);

            ColumnDescription columnDescription = new(expressionBody!, columnName, columnType)
            {
                SharpType = sharpType
            };

            return columnDescription;
        }

        private string MapSharpType2Sql(IPropertySymbol propertySymbol)
        {
            if (propertySymbol.Type is IArrayTypeSymbol arrayTypeSymbol)
            {
                if (!"byte".EqualsIgnoreCase(arrayTypeSymbol.ElementType.ToString()))
                    Throw(Messages.GetInvalidType());

                return "VARBINARY(MAX)";
            }

            if ("string".Equals(propertySymbol.Type.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                AttributeData[] attributes = propertySymbol.GetAttributes().ToArray();
                
                AttributeData? maxLengthAttribute = 
                    attributes.FirstOrDefault(a => a.AttributeClass?.ToString() == "System.ComponentModel.DataAnnotations.MaxLengthAttribute");
                
                AttributeData? minLengthAttribute =
                    attributes.FirstOrDefault(a => a.AttributeClass?.ToString() == "System.ComponentModel.DataAnnotations.MinLengthAttribute");

                if (maxLengthAttribute != null)
                {
                    int? min = null;

                    int? max = ParseIntAttribute(maxLengthAttribute);
                    if (minLengthAttribute != null)
                    {
                        min = ParseIntAttribute(minLengthAttribute);
                    }

                    return min == max ? $"NCHAR({max})" : $"NVARCHAR({max})";
                }
            }
            
            return propertySymbol.Type.Name.ToLowerInvariant() switch
            {
                "guid"           => "UNIQUEIDENTIFIER",
                "byte"           => "BINARY",
                "int16"          => "SMALLINT",
                "int32"          => "INTEGER",
                "int64"          => "BIGINT",
                "boolean"        => "BIT",
                "datetime"       => "DATETIME",
                "datetimeoffset" => "DATETIMEOFFSET",
                "timespan"       => "TIME",
                "decimal"        => "DECIMAL(15,2)",
                "double"         => "FLOAT",
                "single"         => "REAL",
                _                => "NVARCHAR(MAX)"
            };
        }

        private int ParseIntAttribute(AttributeData attributeData)
        {
            SyntaxNode attributeNode = attributeData.ApplicationSyntaxReference!.GetSyntax();
            string attributeText = attributeNode.Cast<AttributeSyntax>()
                .ArgumentList?.Arguments.First().Expression.Cast<LiteralExpressionSyntax>()
                .Token.Text!;

            return int.Parse(attributeText);
        }

        private TableDescription ParseAddSubTable(InvocationExpressionSyntax invocationExpression, SemanticModel semanticModel)
        {
            string? expressionBodyText = null;

            //
            // .AddSubTable<Job>(p => p.Jobs, jobBuilder => jobBuilder
            //              .AddColumn(j => j.CompanyName, "VARCHAR(100)", "COMPANY_NAME")
            //              .AddColumn(j => j.Address, "VARCHAR(200)", "ADDRESS"),
            //              new Assignment<Person, Job>(p => p.Id, j => j.PersonId)
            //          )
            //
            
            string? genericNameText = null;
            ITypeSymbol? subTableGenericType = null;

            for (int argNumber = 0; argNumber < invocationExpression.ArgumentList.Arguments.Count; argNumber++)
            {
                ArgumentSyntax argument = invocationExpression.ArgumentList.Arguments[argNumber];
                if (argNumber == 0)
                {
                    ParenthesizedLambdaExpressionSyntax? lambdaExpressionSyntax = argument.Expression as ParenthesizedLambdaExpressionSyntax;
                    SetContext(lambdaExpressionSyntax, Messages.GetLambdaWithoutType);
                    ExpressionSyntax expressionBody = lambdaExpressionSyntax.ExpressionBody!;
                    expressionBodyText = expressionBody.ToString();

                    // if generic type is implicit: .AddSubTable(person => person.Jobs, ...)
                    // then we must get it from the type information.
                    MemberAccessExpressionSyntax? enumerableMemberAccessExpression = expressionBody
                        .DescendantNodesAndSelf()
                        .OfType<MemberAccessExpressionSyntax>()
                        .LastOrDefault();

                    SetContext(enumerableMemberAccessExpression, Messages.GetMemberAccessExpression);

                    IPropertySymbol? enumerableTypeInfo = semanticModel
                        .GetSymbolInfo(enumerableMemberAccessExpression).Symbol as IPropertySymbol;

                    if (enumerableTypeInfo == null) Throw(Messages.GetInvalidType(), lambdaExpressionSyntax);

                    ITypeSymbol? enumerableType = null;
                    if (enumerableTypeInfo.Type is INamedTypeSymbol namedTypeSymbol)
                    {
                        enumerableType = namedTypeSymbol.TypeArguments[0];
                    }
                    else if (enumerableTypeInfo.Type is IArrayTypeSymbol arrayTypeSymbol)
                    {
                        enumerableType = (INamedTypeSymbol)arrayTypeSymbol.ElementType;
                    }
                    
                    subTableGenericType = enumerableType;

                    genericNameText = subTableGenericType!.Name;
                }
            }

            string sqlTableName = GetSqlTableName(genericNameText!);

            TableDescription subTableDescription = new(genericNameText!, sqlTableName)
            {
                EnumerableName = expressionBodyText
            };

            return subTableDescription;
        }

        // TODO: delete node?
        [DoesNotReturn]
        private void Throw(KeyValuePair<string, string> message, SyntaxNode? node = null) =>
            throw new SourceGeneratorException(message.Key, message.Value, node?.GetLocation() ?? _nodeContext?.GetLocation() ?? Location.None);

        private void SetContext([NotNull] SyntaxNode? node, Func<KeyValuePair<string, string>> errorTextOnNull)
        {
            if (node == null) Throw(errorTextOnNull.Invoke());
            _nodeContext = node;
        }
        
        private void SetContext(SyntaxNode node) => _nodeContext = node;

        private void ParseInvocationExpressions(IEnumerable<InvocationExpressionSyntax> invocationExpressions,
            TableDescription tableDescription, SemanticModel semanticModel)
        {
            foreach (var invocationExpression in invocationExpressions)
            {
                var memberAccessExpressionSyntax = (MemberAccessExpressionSyntax)invocationExpression.Expression;
                
                string methodName = memberAccessExpressionSyntax.Name.Identifier.Text;

                if (methodName == nameof(TableBuilder<int>.AddPreExecutionAction))
                {
                    //
                    // .SetPrimaryKey(person => Console.WriteLine(person.LastName + " " + person.FirstName))
                    //                          ^                                                         ^

                    var lambdaExpression =
                        (LambdaExpressionSyntax)invocationExpression.ArgumentList.Arguments.First().Expression;

                    tableDescription.PreExecutionActions = lambdaExpression.Block?.Statements
                                                               .Select(statement => statement.ToString())
                                                               .ToArray()
                                                           ?? (lambdaExpression.ExpressionBody == null
                                                               ? null
                                                               : new[]
                                                               {
                                                                   lambdaExpression.ExpressionBody + ";"
                                                               })
                                                           ?? Array.Empty<string>();
                }
                else if (methodName == nameof(TableBuilder<int>.SetName))
                {
                    tableDescription.SqlTableName =
                        ((LiteralExpressionSyntax)invocationExpression.ArgumentList.Arguments[0].Expression)
                        .Token
                        .ValueText;
                }

                if (methodName == nameof(TableBuilder<int>.AddColumn))
                {
                    ColumnDescription columnDescription = ParseAddColumn(invocationExpression, semanticModel);
                    tableDescription.Columns.Add(columnDescription);
                }
                else if (methodName == nameof(TableBuilder<int>.AddSubTable))
                {
                    TableDescription subTableDescription = ParseAddSubTable(invocationExpression, semanticModel);

                    tableDescription.SubTables.Add(subTableDescription);
                    subTableDescription.ParentTable = tableDescription;

                    var simpleLambdaExpression = (SimpleLambdaExpressionSyntax)
                        invocationExpression.ArgumentList.Arguments[1].Expression;

                    var subTableInvocationExpressions = simpleLambdaExpression.ExpressionBody!
                        .DescendantNodesAndSelf()
                        .Where(node => node is InvocationExpressionSyntax)
                        .Cast<InvocationExpressionSyntax>()
                        .Reverse();

                    ParseInvocationExpressions(subTableInvocationExpressions, subTableDescription, semanticModel);
                }
            }
        }
    }
}