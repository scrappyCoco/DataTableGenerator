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
            try
            {
                SyntaxTriviaList leadingTrivia = oldStatement.GetLeadingTrivia();
                var whitespaceTrivia = leadingTrivia.LastOrDefault(trivia => trivia.Kind() == SyntaxKind.WhitespaceTrivia);
                var tableDescription = ParseTable(genericTypeInfo.Type!);
                string code = RootResolver.GenerateDataBuilder(tableDescription, whitespaceTrivia.ToString());
                newStatement = SyntaxFactory.ParseStatement(code);
            }
            catch (Exception e)
            {
                newStatement = SyntaxFactory.ParseStatement($"string errorMessage = \"{e.Message}\"");
            }

            SyntaxNode? oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode newRoot = oldRoot!.ReplaceNode(oldStatement, newStatement);
            
            return document.WithSyntaxRoot(newRoot);
        }

        private TableDescription ParseTable(ITypeSymbol type)
        {
            string typeFullName = type.ToString();

            if (_usedTypes.Contains(typeFullName))
            {
                // TODO: uncomment it.
                //throw new SourceGeneratorException($"{typeFullName} cyclic dependencies are not supported.", Location.None);
            }
            _usedTypes.Add(typeFullName);

            TableDescription tableDescription = new(type.Name);

            IPropertySymbol[] propertySymbols = type
                .GetMembers()
                .OfType<IPropertySymbol>()
                .ToArray();
            
            foreach (IPropertySymbol property in propertySymbols)
            {
                ObjectKind objectKind = GetObjectKind(property.Type, out ITypeSymbol? enumerableType);
                if (objectKind == ObjectKind.Scalar)
                {
                    tableDescription.Columns.Add(new ColumnDescription(property.Name));
                }
                else if (objectKind == ObjectKind.Enumerable)
                {
                    TableDescription subTable = ParseTable(enumerableType!);
                    subTable.EnumerableName = property.Name;
                    tableDescription.SubTables.Add(subTable);
                    subTable.ParentTable = tableDescription;
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