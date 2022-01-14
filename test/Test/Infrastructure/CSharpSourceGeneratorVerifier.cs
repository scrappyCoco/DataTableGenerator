using System;
using System.Collections.Immutable;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using Coding4fun.DataTools.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace Coding4fun.DataTools.Test.Infrastructure
{
    public static class CSharpSourceGeneratorVerifier<TSourceGenerator>
        where TSourceGenerator : ISourceGenerator, new()
    {
        public class Test : CSharpSourceGeneratorTest<TSourceGenerator, NUnitVerifier>
        {
            public Test()
            {
                Assembly GetAssembly(string name) => AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == name);
            
                Assembly netStandardAssembly = GetAssembly("netstandard");
                Assembly runtimeAssembly = GetAssembly("System.Runtime");
                Assembly linqAssembly = GetAssembly("System.Linq");

                TestState.AdditionalReferences.AddRange(new[]
                {
                    netStandardAssembly,
                    runtimeAssembly,
                    linqAssembly,
                    typeof(DataTable).Assembly,
                    typeof(SqlConnection).Assembly,
                    typeof(Console).Assembly,
                    typeof(Attribute).Assembly,
                    typeof(GCLatencyMode).Assembly,
                    typeof(Expression).Assembly,
                    typeof(Func<string, string>).Assembly,
                    typeof(TableBuilder<int>).Assembly
                }.Select(assembly => MetadataReference.CreateFromFile(assembly.Location)).ToArray());
            }

            protected override CompilationOptions CreateCompilationOptions()
            {
                var compilationOptions = base.CreateCompilationOptions();
                return compilationOptions.WithSpecificDiagnosticOptions(
                    compilationOptions.SpecificDiagnosticOptions.SetItems(GetNullableWarningsFromCompiler()));
            }

            public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.Default;

            private static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarningsFromCompiler()
            {
                string[] args = { "/warnaserror:nullable" };
                var commandLineArguments = CSharpCommandLineParser.Default.Parse(args, baseDirectory: Environment.CurrentDirectory, sdkDirectory: Environment.CurrentDirectory);
                var nullableWarnings = commandLineArguments.CompilationOptions.SpecificDiagnosticOptions;

                return nullableWarnings;
            }

            protected override ParseOptions CreateParseOptions()
            {
                return ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(LanguageVersion);
            }
        }
    }
}