using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

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
            
            Diagnostic diagnostic = context.Diagnostics.First(d => d.Id == TableBuilderAnalyzer.DiagnosticId);
            TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;
            
            ObjectCreationExpressionSyntax declaration = root!.FindToken(diagnosticSpan.Start).Parent!
                .AncestorsAndSelf()
                .OfType<ObjectCreationExpressionSyntax>()
                .First();

            const string title = "Create SQL mapping for DataBuilder";
            
            context.RegisterCodeFix(
                CodeAction.Create(
                    title,
                    createChangedDocument: cancellationToken =>
                        new TableBuilderCodeFixImpl().AddSqlMappingAsync(context.Document, declaration, cancellationToken),
                    equivalenceKey: title),
                diagnostic);
        }

        /// <inheritdoc />
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(TableBuilderAnalyzer.DiagnosticId);
    }
}