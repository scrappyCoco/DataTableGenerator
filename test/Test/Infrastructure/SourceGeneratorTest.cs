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
            CompilationUtil.RunGenerators(compilation, out ImmutableArray<Diagnostic> diagnostics,
                new TSourceGenerator());
            ImmutableArray<Diagnostic> compilationDiagnostics = compilation.GetDiagnostics();
            
            if (expectedDiagnostics == null && compilationDiagnostics.Any())
            {
                // Syntax error.
                Assert.That(compilationDiagnostics.All(d => d.Descriptor.Id.StartsWith("CS")), "Syntax errors were expected.");
            }
            else if (expectedDiagnostics != null)
            {
                Assert.AreEqual(1, diagnostics.Length);
                Assert.AreEqual(expectedDiagnostics.Value.Message, diagnostics[0].GetMessage());
                Assert.AreEqual(expectedDiagnostics.Value.Severity, diagnostics[0].Severity);
                Assert.AreEqual(expectedDiagnostics.Value.Id, diagnostics[0].Descriptor.Id);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}