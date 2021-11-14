﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Coding4fun.DataTools.Analyzers.Extension;
using Coding4fun.DataTools.Analyzers.StringUtil;
using Coding4fun.DataTools.Analyzers.Template.DataTable;
using Microsoft.CodeAnalysis;
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
        private SyntaxNode? _nodeContext = null;

        /// <inheritdoc />
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        /// <inheritdoc />
        public void Execute(GeneratorExecutionContext context)
        {
            foreach (SyntaxTree syntaxTree in context.Compilation.SyntaxTrees)
            {
                if (syntaxTree.Options.Errors.Any() ||
                    syntaxTree.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error))
                {
                    return;
                }

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

                        SetContext(genericName, DataTableMessages.GetUnableToGetTableDefinition);

                        // new DataTableBuilder<Person>()...
                        //                     ^      ^
                        TypeSyntax genericType = genericName.TypeArgumentList.Arguments.First();
                        SetContext(genericType);

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
                        string sqlTableName = GetSqlTableName(genericName.ToString());
                        TableDescription tableDescription =
                            new(genericType.ToString(), sqlTableName);

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
                        {
                            Throw(DataTableMessages.GetSqlMappingIsEmpty());
                        }

                        SemanticModel semanticModel = context.Compilation.GetSemanticModel(syntaxTree);
                        ITypeSymbol? genericTypeSymbol = semanticModel.GetTypeInfo(genericType).Type;
                        if (genericTypeSymbol is null or IErrorTypeSymbol)
                        {
                            Throw(DataTableMessages.GetUnableToGetTableTypeInfo(), genericType);
                        }

                        ParseInvocationExpressions(invocationExpressions, tableDescription, genericTypeSymbol);
                        
                        string[] usingDirectives = rootNode.DescendantNodes()
                            .OfType<UsingDirectiveSyntax>()
                            .Select(u => u.Name.ToString())
                            .ToArray();
                        
                        // public partial class PersonSqlMapping
                        //                      ^              ^
                        
                        ClassDeclarationSyntax classDeclaration =
                            genericType.Ancestors().OfType<ClassDeclarationSyntax>().First()!;
                        
                        string sqlMappingClassName = classDeclaration.Identifier.Text;
                        NamespaceDeclarationSyntax? namespaceDeclaration = genericName.Ancestors()
                            .OfType<NamespaceDeclarationSyntax>()
                            .FirstOrDefault();

                        SetContext(namespaceDeclaration, DataTableMessages.GetUnableToFindNamespace);
                        
                        string @namespace = namespaceDeclaration.Name.GetText().ToString().Trim();
                        
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
                NamingConvention.CamelCase => entityName.ChangeCase(CaseRules.ToCamelCase)!,
                NamingConvention.PascalCase => entityName.ChangeCase(CaseRules.ToTitleCase)!,
                NamingConvention.KebabCase => entityName.ChangeCase(CaseRules.ToLowerCase, "-")!,
                NamingConvention.SnakeCase => entityName.ChangeCase(CaseRules.ToLowerCase, "_")!,
                NamingConvention.ScreamingSnakeCase => entityName.ChangeCase(CaseRules.ToUpperCase, "_")!,
                _ => throw new ArgumentOutOfRangeException($"Unable to map {_namingConvention}.")
            };

            return sqlTableName;
        }

        private string GetSqlTableName(string entityName) => "#" + ChangeSqlCase(entityName);

        private ColumnDescription ParseAddColumn(InvocationExpressionSyntax invocationExpression,
            ITypeSymbol genericType)
        {
            string? propertyName = null;
            string? columnName = null;
            string? columnType = null;
            string? varName = null;
            string? expressionBody = null;

            //
            // ...AddColumn(person => person.CountryCode, "CHAR(2)", "COUNTRY_CODE")
            //
            for (int argNumber = 0; argNumber < invocationExpression.ArgumentList.Arguments.Count; argNumber++)
            {
                ArgumentSyntax argument = invocationExpression.ArgumentList.Arguments[argNumber];
                string? parameterName = argument.NameColon?.Name.Identifier.Text;

                if (argNumber == 0)
                {
                    // person => person.CountryCode
                    if (argument.Expression is SimpleLambdaExpressionSyntax simpleLambdaExpression)
                    {
                        varName = simpleLambdaExpression.Parameter.ToString();
                        SetContext(simpleLambdaExpression.ExpressionBody, DataTableMessages.GetUnableToGetExpressionBody);
                        expressionBody = simpleLambdaExpression.ExpressionBody!.ToString();
                        propertyName = columnName = simpleLambdaExpression.ExpressionBody.GetLastToken().ToString();
                    }
                    else
                    {
                        // (person) => person.CountryCode
                        var parenthesizedLambdaExpression =
                            argument.Expression as ParenthesizedLambdaExpressionSyntax;

                        SetContext(parenthesizedLambdaExpression?.ExpressionBody, DataTableMessages.GetUnableToGetExpressionBody);
                        
                        varName = parenthesizedLambdaExpression.ParameterList.Parameters.First().Identifier.Text;
                        expressionBody = parenthesizedLambdaExpression.ExpressionBody!.ToString();
                        propertyName = columnName =
                            parenthesizedLambdaExpression.ExpressionBody.GetLastToken().ToString();
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

            if (expressionBody == null || varName == null || columnName == null || propertyName == null)
            {
                Throw(DataTableMessages.GetUnableToParseInvocationExpression(), invocationExpression);
            }

            IPropertySymbol? propertySymbol = null;
            try
            {
                propertySymbol = (IPropertySymbol)genericType.GetMembers(propertyName)[0];
            }
            catch
            {
                Throw(DataTableMessages.GetUnableToResolveProperty(propertyName, genericType.Name));
            }
            columnType ??= MapSharpType2Sql(propertySymbol);
            string? sharpType = propertySymbol.Type.ToString();

            columnName = ChangeSqlCase(columnName);

            ColumnDescription columnDescription = new(expressionBody, columnName, columnType)
            {
                SharpType = sharpType
            };

            return columnDescription;
        }

        private string MapSharpType2Sql(IPropertySymbol propertySymbol)
        {
            if (propertySymbol.Type is IArrayTypeSymbol arrayTypeSymbol)
            {
                if (!"byte".Equals(arrayTypeSymbol.ElementType.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    Throw(DataTableMessages.GetInvalidType());
                }

                return "VARBINARY(MAX)";
            }

            if ("string".Equals(propertySymbol.Type.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                AttributeData[] attributes = propertySymbol.GetAttributes().ToArray();
                AttributeData? maxLengthAttribute =
                    attributes.FirstOrDefault(a => a.AttributeClass?.Name == "MaxLength");
                AttributeData? minLengthAttribute =
                    attributes.FirstOrDefault(a => a.AttributeClass?.Name == "MinLength");

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
                "short"          => "SMALLINT",
                "int16"          => "SMALLINT",
                "int"            => "INTEGER",
                "int32"          => "INTEGER",
                "long"           => "BIGINT",
                "int64"          => "BIGINT",
                "bool"           => "BIT",
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
            string? attributeText = attributeNode.Cast<AttributeSyntax>()
                .ArgumentList?.Arguments.First().Expression.Cast<LiteralExpressionSyntax>()
                .Token.Text;

            if (attributeText == null)
            {
                Throw(DataTableMessages.GetUnableToParseIntValueFromAttribute(), attributeNode);
            }

            return int.Parse(attributeText);
        }

        private TableDescription ParseAddSubTable(InvocationExpressionSyntax invocationExpression,
            ITypeSymbol genericType)
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
                    var lambdaExpressionSyntax = argument.Expression as SimpleLambdaExpressionSyntax;
                    ExpressionSyntax? expressionBody = lambdaExpressionSyntax?.ExpressionBody;
                    expressionBodyText = expressionBody?.ToString();

                    if (expressionBody == null)
                    {
                        Throw(DataTableMessages.GetUnableToGetBodyOfLambdaExpression(), argument);
                    }

                    // if generic type is implicit: .AddSubTable(person => person.Jobs, ...)
                    // then we must get it from the type information.
                    MemberAccessExpressionSyntax? enumerableMemberAccessExpression = expressionBody
                        .DescendantNodesAndSelf()
                        .OfType<MemberAccessExpressionSyntax>()
                        .Last();

                    string enumerableName = enumerableMemberAccessExpression.GetLastToken().Text;
                    IPropertySymbol propertySymbol = (IPropertySymbol)genericType.GetMembers(enumerableName)[0];
                    ITypeSymbol enumerableType;
                    if (propertySymbol.Type is INamedTypeSymbol namedTypeSymbol)
                    {
                        enumerableType = namedTypeSymbol.TypeArguments[0];
                    }
                    else if (propertySymbol.Type is IArrayTypeSymbol arrayTypeSymbol)
                    {
                        enumerableType = (INamedTypeSymbol)arrayTypeSymbol.ElementType;
                    }
                    else
                    {
                        Throw(DataTableMessages.GetUnableToGetTypeOfEnumerable());
                        throw new InvalidOperationException();
                    }
                    
                    subTableGenericType = enumerableType;

                    genericNameText = subTableGenericType.Name;
                }
            }

            if (genericNameText == null || subTableGenericType == null)
            {
                Throw(DataTableMessages.GetUnableToGetGenericName(), invocationExpression);
            }

            string sqlTableName = GetSqlTableName(genericNameText);

            TableDescription subTableDescription = new(genericNameText, sqlTableName)
            {
                EnumerableName = expressionBodyText,
                GenericType = subTableGenericType
            };

            return subTableDescription;
        }

        // TODO: delete node?
        [DoesNotReturn]
        private void Throw(KeyValuePair<string, string> message, SyntaxNode? node = null) =>
            throw new SourceGeneratorException(message.Key, message.Value, node?.GetLocation() ?? _nodeContext?.GetLocation() ?? Location.None);

        private void SetContext([NotNull] SyntaxNode? node, Func<KeyValuePair<string, string>> errorTextOnNull)
        {
            if (node == null)
            {
                Throw(errorTextOnNull.Invoke());
            }
            
            _nodeContext = node;
        }
        
        private void SetContext(SyntaxNode node) => _nodeContext = node;

        private void ParseInlineObject(InvocationExpressionSyntax invocationExpressionSyntax)
        {
            
        }

        private void ParseInvocationExpressions(IEnumerable<InvocationExpressionSyntax> invocationExpressions,
            TableDescription tableDescription,
            ITypeSymbol genericType)
        {
            foreach (var invocationExpression in invocationExpressions)
            {
                if (invocationExpression.Expression is not MemberAccessExpressionSyntax
                    memberAccessExpressionSyntax) continue;

                string methodName = memberAccessExpressionSyntax.Name.Identifier.Text;

                if (methodName == nameof(TableBuilder<int>.AddPreExecutionAction))
                {
                    //
                    // .SetPrimaryKey(person => Console.WriteLine(person.LastName + " " + person.FirstName))
                    //                          ^                                                         ^

                    var lambdaExpression =
                        invocationExpression.ArgumentList.Arguments.FirstOrDefault()?.Expression as
                            LambdaExpressionSyntax;
                    
                    if (lambdaExpression == null)
                    {
                        Throw(DataTableMessages.GetUnableToGetExpression(),
                            invocationExpression.ArgumentList);
                    }
                    
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
                    ColumnDescription columnDescription = ParseAddColumn(invocationExpression, genericType);
                    tableDescription.Columns.Add(columnDescription);
                }
                if (methodName == nameof(TableBuilder<int>.InlineObject))
                {
                    ParseInlineObject(invocationExpression);
                }
                else if (methodName == nameof(TableBuilder<int>.AddSubTable))
                {
                    TableDescription subTableDescription = ParseAddSubTable(invocationExpression, genericType);

                    tableDescription.SubTables.Add(subTableDescription);
                    subTableDescription.ParentTable = tableDescription;

                    var simpleLambdaExpression = (SimpleLambdaExpressionSyntax)
                        invocationExpression.ArgumentList.Arguments[1].Expression;

                    var subTableInvocationExpressions = simpleLambdaExpression.ExpressionBody!
                        .DescendantNodesAndSelf()
                        .Where(node => node is InvocationExpressionSyntax)
                        .Cast<InvocationExpressionSyntax>()
                        .Reverse();

                    if (subTableDescription.GenericType == null) Throw(DataTableMessages.GetUnableToGetGenericTypeOfSubTable());
                    
                    ParseInvocationExpressions(subTableInvocationExpressions, subTableDescription,
                        subTableDescription.GenericType!);
                }
            }
        }
    }
}