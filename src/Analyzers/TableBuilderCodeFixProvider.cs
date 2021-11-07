using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Coding4fun.DataTools.Analyzers.Extension;
using Coding4fun.DataTools.Analyzers.Template.TableBuilder;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Coding4fun.DataTools.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TableBuilderCodeFixProvider)), Shared]
    public class TableBuilderCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc />
        public override FixAllProvider? GetFixAllProvider() => null;

        /// <inheritdoc />
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode? root = await context.Document
                .GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);
            
            Diagnostic diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            ObjectCreationExpressionSyntax declaration = root!.FindToken(diagnosticSpan.Start).Parent!
                .AncestorsAndSelf()
                .OfType<ObjectCreationExpressionSyntax>()
                .First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Create SQL mapping",
                    createChangedDocument: c => AddSqlMappingAsync(context.Document, declaration, c),
                    equivalenceKey: "Create SQL mapping"),
                diagnostic);
        }

        /// <inheritdoc />
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(TableBuilderAnalyzer.DiagnosticId);

        private static async Task<Document> AddSqlMappingAsync(Document document,
            ObjectCreationExpressionSyntax objectCreationExpression,
            CancellationToken cancellationToken)
        {
            SemanticModel? semanticModel =
                await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            var genericName = (GenericNameSyntax)objectCreationExpression.Type;
            IdentifierNameSyntax typeSyntax = (IdentifierNameSyntax)genericName.TypeArgumentList.Arguments.First();
            TypeInfo genericTypeInfo = semanticModel.GetTypeInfo(typeSyntax, cancellationToken);

            var tableDescription = ParseTable(genericTypeInfo.Type!);

            StatementSyntax oldStatement = objectCreationExpression.Ancestors().OfType<StatementSyntax>().First();
            SyntaxTriviaList leadingTrivia = oldStatement.GetLeadingTrivia();
            var whitespaceTrivia = leadingTrivia.LastOrDefault(trivia => trivia.Kind() == SyntaxKind.WhitespaceTrivia);

            string code = RootResolver.GenerateDataBuilder(tableDescription, whitespaceTrivia.ToString());

            StatementSyntax newStatement = SyntaxFactory.ParseStatement(code);
            
            SyntaxNode? oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode newRoot = oldRoot!.ReplaceNode(oldStatement, newStatement);

            // Return document with transformed tree.
            return document.WithSyntaxRoot(newRoot);
        }

        private static TableDescription ParseTable(ITypeSymbol type)
        {
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
                // TODO: ObjectKind.Object
            }

            return tableDescription;
        }

        private static ObjectKind GetObjectKind(ITypeSymbol type, out ITypeSymbol? enumerableType)
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

        private static bool IsScalarType(ITypeSymbol type)
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