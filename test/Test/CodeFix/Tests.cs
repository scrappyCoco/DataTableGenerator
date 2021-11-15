﻿using System.Linq;
using System.Threading.Tasks;
using Coding4fun.DataTools.Analyzers;
using Coding4fun.DataTools.Test.Infrastructure;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using Message = System.Collections.Generic.KeyValuePair<string, string>;

namespace Coding4fun.DataTools.Test.CodeFix
{
    public class Tests : TestBase
    {
        [Test]
        public async Task Basic()
        {
            string source = await LoadAsync();
            string target = await LoadAsync("Target.cs");
            await CSharpCodeFixVerifier<TableBuilderAnalyzer, TableBuilderCodeFixProvider>.VerifyCodeFixAsync(source,
                target);
        }

        [Test]
        public async Task CyclicDependency()
        {
            string source = await LoadAsync();
            string target = await LoadAsync("Target.cs");
            Message analyzerMessage = new(TableBuilderAnalyzer.DiagnosticId, "It should have some method calls");
            Message fixMessage = DataTableMessages.GetCyclicDependenciesAreNotSupported("Example.Contact");

            DiagnosticResult[] expectedDiagnostics = {
                //new DiagnosticResult(fixMessage.Key, DiagnosticSeverity.Warning).WithMessage(fixMessage.Value),
                new DiagnosticResult(analyzerMessage.Key, DiagnosticSeverity.Info)
                    .WithMessage(analyzerMessage.Value)
                    .WithSpan(47, 13, 47, 39)
            };

            await CSharpCodeFixVerifier<TableBuilderAnalyzer, TableBuilderCodeFixProvider>.VerifyCodeFixAsync(source, expectedDiagnostics, target);
        }
    }
}