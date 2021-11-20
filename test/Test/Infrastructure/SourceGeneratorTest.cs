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
            DiagnosticResult? expectedDiagnostics = null;
            if (expectedMessage != null)
            {
                expectedDiagnostics =
                    new DiagnosticResult(expectedMessage.Value.Key, DiagnosticSeverity.Error)
                        .WithMessage(expectedMessage.Value.Value);
            }
            

            string source = await LoadAsync("Source.cs", methodName!);
            Compilation compilation = CompilationUtil.CreateCompilation(source);
            Diagnostic[] errors = compilation.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error && d.Descriptor.Id.StartsWith("CS"))
                .ToArray();
            
            if (errors.Any())
            {
                // Syntax error.
                Assert.Fail($"Syntax error: {errors[0]}");
            }
            
            CompilationUtil.RunGenerators(compilation, out ImmutableArray<Diagnostic> diagnostics,
                new TSourceGenerator());
            
            if (expectedDiagnostics != null)
            {
                Assert.AreEqual(1, diagnostics.Length, "Invalid count of diagnostics.");
                Assert.AreEqual(expectedDiagnostics.Value.Message, diagnostics[0].GetMessage());
                Assert.AreEqual(expectedDiagnostics.Value.Severity, diagnostics[0].Severity);
                Assert.AreEqual(expectedDiagnostics.Value.Id, diagnostics[0].Descriptor.Id);
            }

            errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            Assert.Zero(errors.Length, $"Some errors contains in generated code: {errors.First()}");
        }
    }
}