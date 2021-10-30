using System.Collections.Immutable;
using Coding4fun.DataTools.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Coding4fun.DataTools.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TableBuilderAnalyzer: DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TableBuilderAnalyzer";
        
        private static readonly string Title = $"{TableBuilder<int>.Name} is empty";
        private static readonly string MessageFormat = "It should have some method calls.";
        private static readonly string Description = "Add methods.";
        private const string Category = "DataTools";

        private static DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category,
            DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        
        /// <inheritdoc />
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ObjectCreationExpression);
        }

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var objectCreationExpression = (ObjectCreationExpressionSyntax)context.Node;
            if (objectCreationExpression.Type is not GenericNameSyntax { Identifier: { Text: TableBuilder<int>.Name } }) return;

            if (objectCreationExpression.Parent?.Kind() != SyntaxKind.SimpleMemberAccessExpression)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
            }
        }
    }
}