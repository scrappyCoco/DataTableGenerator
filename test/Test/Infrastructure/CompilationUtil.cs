using System.Collections.Immutable;
using System.Data;
using Coding4fun.DataTools.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Coding4fun.DataTools.Test.Infrastructure
{
    internal static class CompilationUtil
    {
        internal static Compilation CreateCompilation(string source)
            => CSharpCompilation.Create("compilation",
                new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp9)) },
                new[]
                {
                    MetadataReference.CreateFromFile(typeof(DataTable).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(TableBuilder<int>).Assembly.Location)
                },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));

        internal static Compilation RunGenerators(Compilation compilation, out ImmutableArray<Diagnostic> diagnostics, params ISourceGenerator[] generators)
        {
            CSharpGeneratorDriver.Create(generators).RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out diagnostics);
            return newCompilation;
        }
    }
}