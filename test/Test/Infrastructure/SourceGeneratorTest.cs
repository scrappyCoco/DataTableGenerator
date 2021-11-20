using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using ErrorMessage = System.Collections.Generic.KeyValuePair<string, string>;

namespace Coding4fun.DataTools.Test.Infrastructure
{
    public class SourceGeneratorTest<TSourceGenerator> : TestBase where TSourceGenerator : ISourceGenerator, new()
    {
        protected async Task AssertDiagnosticAsync(
            ErrorMessage? expectedMessage = null,
            [CallerMemberName] string? methodName = null
        )
        {
            string source = await LoadAsync("Source.cs", methodName!);
            Compilation compilation = CompilationUtil.CreateCompilation(source);
            Diagnostic[] compilationErrors = compilation.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error && d.Descriptor.Id.StartsWith("CS"))
                .ToArray();

            if (expectedMessage != null)
            {
                // Syntax error.
                if (compilationErrors.Any())
                {
                    Assert.Fail($"Syntax error: {compilationErrors[0]}");
                }
            }
            else
            {
                if (!compilationErrors.Any())
                {
                    Assert.Fail($"Syntax error: {compilationErrors[0]}");
                }
            }
            
            CompilationUtil.RunGenerators(compilation, out ImmutableArray<Diagnostic> generatorDiagnostics,
                new TSourceGenerator());
            
            if (expectedMessage != null)
            {
                DiagnosticResult expectedDiagnostics =
                    new DiagnosticResult(expectedMessage.Value.Key, DiagnosticSeverity.Error)
                        .WithMessage(expectedMessage.Value.Value);
                
                Assert.AreEqual(1, generatorDiagnostics.Length, "Invalid count of diagnostics.");
                Assert.AreEqual(expectedDiagnostics.Message, generatorDiagnostics[0].GetMessage());
                Assert.AreEqual(expectedDiagnostics.Severity, generatorDiagnostics[0].Severity);
                Assert.AreEqual(expectedDiagnostics.Id, generatorDiagnostics[0].Descriptor.Id);
            }
            else
            {
                Diagnostic[] generatorErrors = generatorDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
                Assert.Zero(generatorErrors.Length, $"Some errors contains in generated code: {generatorErrors.FirstOrDefault()}");                
            }
        }
    }
}