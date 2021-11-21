using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using ErrorMessage = System.Collections.Generic.KeyValuePair<string, string>;

namespace Coding4fun.DataTools.Test.Infrastructure
{
    public class SourceGeneratorTest<TSourceGenerator> : TestBase where TSourceGenerator : ISourceGenerator, new()
    {
        private readonly Regex _errorTextRangeRegex = new Regex("(?<start>/[*][[]__ERROR__[*]/)(.|\\s)+?(?<end>/[*]__ERROR__[]][*]/)");
        
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
                Assert.AreEqual(expectedDiagnostics.Id, generatorDiagnostics[0].Descriptor.Id);
                Assert.AreEqual(expectedDiagnostics.Message, generatorDiagnostics[0].GetMessage());
                Assert.AreEqual(expectedDiagnostics.Severity, generatorDiagnostics[0].Severity);
                AssertValidRange(source, generatorDiagnostics[0]);
            }
            else
            {
                Diagnostic[] generatorErrors =
                    generatorDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
                Assert.Zero(generatorErrors.Length,
                    $"Some errors contains in generated code: {generatorErrors.FirstOrDefault()}");
            }
        }

        private void AssertValidRange(string sourceText, Diagnostic actualDiagnostic)
        {
            Match rangeMatch = _errorTextRangeRegex.Match(sourceText);
            if (!rangeMatch.Success) Assert.Fail("Expected diagnostics was not found");
            Group startGroup = rangeMatch.Groups["start"];
            Group endGroup = rangeMatch.Groups["end"];

            TextSpan actualLocationSpan = actualDiagnostic.Location.SourceSpan;
            bool isValidRange = (actualLocationSpan.Start == startGroup.Index ||
                                 actualLocationSpan.Start == startGroup.Index + startGroup.Length) &&
                                (actualLocationSpan.End == endGroup.Index ||
                                 actualLocationSpan.End == endGroup.Index + endGroup.Length);

            if (!isValidRange)
            {
                ReadOnlySpan<char> actualLocationText =
                    sourceText.AsSpan().Slice(actualLocationSpan.Start, actualLocationSpan.Length);
                
                Assert.Fail($"Expected diagnostics location: {actualLocationText.ToString()}\nBut was: {rangeMatch.Value}");
            }
        }
    }
}