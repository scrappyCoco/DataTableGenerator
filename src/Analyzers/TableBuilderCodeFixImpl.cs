using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Coding4fun.DataTools.Analyzers.Extension;
using Coding4fun.DataTools.Analyzers.Template.TableBuilder;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Coding4fun.DataTools.Analyzers
{
    public class TableBuilderCodeFixImpl
    {
        private readonly HashSet<string> _usedTypes = new();
        private readonly Dictionary<IPropertySymbol, LinkedList<string>> _objectPaths = new();

        public async Task<Document> AddSqlMappingAsync(Document document,
            ObjectCreationExpressionSyntax objectCreationExpression,
            CancellationToken cancellationToken)
        {
            SemanticModel? semanticModel =
                await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            var genericName = (GenericNameSyntax)objectCreationExpression.Type;
            IdentifierNameSyntax typeSyntax = (IdentifierNameSyntax)genericName.TypeArgumentList.Arguments.First();
            TypeInfo genericTypeInfo = ModelExtensions.GetTypeInfo(semanticModel!, typeSyntax, cancellationToken);
            StatementSyntax oldStatement = objectCreationExpression.Ancestors().OfType<StatementSyntax>().First();
            
            StatementSyntax newStatement;
            SyntaxTrivia? whitespaceTrivia = null;
            try
            {
                SyntaxTriviaList leadingTrivia = oldStatement.GetLeadingTrivia();
                whitespaceTrivia = leadingTrivia.LastOrDefault(trivia => trivia.Kind() == SyntaxKind.WhitespaceTrivia);
                TableDescription tableDescription = ParseTable(genericTypeInfo.Type!);
                string code = RootResolver.GenerateDataBuilder(tableDescription, whitespaceTrivia.ToString());
                newStatement = SyntaxFactory.ParseStatement(code);
            }
            catch (Exception e)
            {
                newStatement = SyntaxFactory.ParseStatement($"{whitespaceTrivia}string errorMessage = \"{e.Message}\";\n");
            }

            SyntaxNode? oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode newRoot = oldRoot!.ReplaceNode(oldStatement, newStatement);
            
            return document.WithSyntaxRoot(newRoot);
        }

        private TableDescription ParseTable(ITypeSymbol type)
        {
            void CheckUsedType(ITypeSymbol t)
            {
                string typeFullName = t.ToString();
                if (_usedTypes.Contains(typeFullName))
                {
                    throw new SourceGeneratorException(DataTableMessages.GetCyclicDependenciesAreNotSupported(typeFullName));
                }
                _usedTypes.Add(typeFullName);
            }

            CheckUsedType(type);

            TableDescription tableDescription = new(type.Name);

            List<IPropertySymbol> propertySymbols = type
                .GetMembers()
                .OfType<IPropertySymbol>()
                .ToList();

            for (int propertyNumber = 0; propertyNumber < propertySymbols.Count; propertyNumber++)
            {
                IPropertySymbol property = propertySymbols[propertyNumber];
                _objectPaths.TryGetValue(property, out LinkedList<string>? pathComponents);

                string GetPropertyName()
                {
                    if (!(pathComponents?.Any() ?? false)) return property.Name;
                    return string.Join(".", pathComponents) + "." + property.Name;
                }
                
                ObjectKind objectKind = GetObjectKind(property.Type, out ITypeSymbol? enumerableType);
                if (objectKind == ObjectKind.Scalar)
                {
                    tableDescription.Columns.Add(new ColumnDescription(GetPropertyName()));
                }
                else if (objectKind == ObjectKind.Enumerable)
                {
                    TableDescription subTable = ParseTable(enumerableType!);
                    subTable.EnumerableName = GetPropertyName();
                    tableDescription.SubTables.Add(subTable);
                    subTable.ParentTable = tableDescription;
                }
                else if (objectKind == ObjectKind.Object)
                {
                    CheckUsedType(property.Type);
                    
                    IPropertySymbol[] objectProperties = property.Type
                        .GetMembers()
                        .OfType<IPropertySymbol>()
                        .ToArray();
                    
                    foreach (IPropertySymbol objectProperty in objectProperties)
                    {
                        LinkedList<string> cPathComponents = new();
                        if (pathComponents != null)
                        {
                            foreach (string pathComponent in pathComponents)
                            {
                                cPathComponents.AddLast(pathComponent);
                            }
                        }
                        cPathComponents.AddLast(property.Name);
                        _objectPaths.Add(objectProperty, cPathComponents);
                    }
                    
                    propertySymbols.AddRange(objectProperties);
                }
            }

            return tableDescription;
        }

        private ObjectKind GetObjectKind(ITypeSymbol type, out ITypeSymbol? enumerableType)
        {
            enumerableType = null;
            if (IsScalarType(type)) return ObjectKind.Scalar;
            if (type is IArrayTypeSymbol arrayType)
            {
                if ("byte".EqualsIgnoreCase(arrayType.ElementType.Name))
                    return ObjectKind.Scalar;
                
                if (IsScalarType(arrayType.ElementType)) return ObjectKind.Ignore;
                enumerableType = arrayType.ElementType;
                return ObjectKind.Enumerable;
            }

            if (type.AllInterfaces.Any(i =>
                    i.Name == "IEnumerable" && i.ContainingNamespace.ToString() == "System.Collections")
                && type is INamedTypeSymbol namedType)
            {
                ITypeSymbol genericTypeParameter = namedType.TypeArguments.First();
                enumerableType = genericTypeParameter;
                if ("byte".EqualsIgnoreCase(genericTypeParameter.Name)) return ObjectKind.Scalar;
                if (IsScalarType(genericTypeParameter)) return ObjectKind.Ignore;
                return ObjectKind.Enumerable;
            }

            return ObjectKind.Object;
        }

        private bool IsScalarType(ITypeSymbol type)
        {
            if (type.IsValueType) return true;
            if ("string".EqualsIgnoreCase(type.Name)) return true;
            return false;
        }

        private enum ObjectKind
        {
            Scalar,
            Enumerable,
            Object,
            Ignore
        }
    }
}