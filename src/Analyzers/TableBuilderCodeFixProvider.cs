using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Antlr4.StringTemplate;
using Coding4fun.DataTools.Analyzers.Extension;
using Coding4fun.PainlessUtils;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;

namespace Coding4fun.DataTools.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TableBuilderCodeFixProvider)), Shared]
    public class TableBuilderCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc />
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf()
                .OfType<ObjectCreationExpressionSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Make constant",
                    createChangedDocument: c => MakeConstAsync(context.Document, declaration, c),
                    equivalenceKey: "Make constant"),
                diagnostic);
        }

        /// <inheritdoc />
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(TableBuilderAnalyzer.DiagnosticId);

        private static async Task<Document> MakeConstAsync(Document document,
            ObjectCreationExpressionSyntax objectCreationExpression,
            CancellationToken cancellationToken)
        {
            SemanticModel? semanticModel =
                await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            var genericName = (GenericNameSyntax)objectCreationExpression.Type;
            IdentifierNameSyntax typeSyntax = (IdentifierNameSyntax)genericName.TypeArgumentList.Arguments.First();
            string typeName = typeSyntax.Identifier.Text;
            TypeInfo genericTypeInfo = semanticModel.GetTypeInfo(typeSyntax, cancellationToken);

            var tableDescription = ParseTable(genericTypeInfo.Type);

            StatementSyntax oldStatement = objectCreationExpression.Ancestors().OfType<StatementSyntax>().First();
            SyntaxTriviaList leadingTrivia = oldStatement.GetLeadingTrivia();
            SyntaxTriviaList trailingTrivia = oldStatement.GetTrailingTrivia();

            Template template = TemplateManager.GetTableBuilderTemplate();
            template.Add("table", tableDescription);
            template.Add("leadingTrivia", leadingTrivia);
            template.Add("trailingTrivia", trailingTrivia);


            string code = template.Render();

            StatementSyntax newStatement = SyntaxFactory.ParseStatement(code);
            SyntaxNode? oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode newRoot = oldRoot.ReplaceNode(oldStatement, newStatement);

            // Return document with transformed tree.
            return document.WithSyntaxRoot(newRoot!);
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
                    tableDescription.SubTables.Add(ParseTable(enumerableType));
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
                ITypeSymbol? genericTypeParameter = namedType.TypeArguments.FirstOrDefault();
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