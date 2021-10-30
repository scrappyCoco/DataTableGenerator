using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Coding4fun.PainlessUtils;
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
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ObjectCreationExpressionSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Make constant",
                    createChangedDocument: c => MakeConstAsync(context.Document, declaration, c),
                    equivalenceKey: "Make constant"),
                diagnostic);
        }

        /// <inheritdoc />
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(TableBuilderAnalyzer.DiagnosticId);
        
        private static async Task<Document> MakeConstAsync(Document document,
            ObjectCreationExpressionSyntax objectCreationExpression,
            CancellationToken cancellationToken)
        {
            SemanticModel? semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            
            var genericName = (GenericNameSyntax)objectCreationExpression.Type;
            IdentifierNameSyntax typeSyntax = (IdentifierNameSyntax)genericName.TypeArgumentList.Arguments.First();

            string typeName = typeSyntax.Identifier.Text;
            TypeInfo genericTypeInfo = semanticModel.GetTypeInfo(typeSyntax, cancellationToken);
            string lambdaParamName = typeName.ChangeCase(CaseRules.ToCamelCase, "")!;

            StatementSyntax oldStatement = objectCreationExpression.Ancestors().OfType<StatementSyntax>().First();
            SyntaxTriviaList leadingTrivia = oldStatement.GetLeadingTrivia();
            SyntaxTriviaList trailingTrivia = oldStatement.GetTrailingTrivia();
            
            string code = $"{leadingTrivia.ToString()}new TableBuilder<{typeName}>()" + string.Join("", genericTypeInfo.Type
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Select(p => $".AddColumn({lambdaParamName} => {lambdaParamName}.{p.Type.IsValueType})")
            ) + ";" + trailingTrivia;

            StatementSyntax newStatement = SyntaxFactory.ParseStatement(code);


            SyntaxNode? oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            SyntaxNode newRoot = oldRoot.ReplaceNode(oldStatement, newStatement);
            
            // Return document with transformed tree.
            return document.WithSyntaxRoot(newRoot!);
        }
    }
}