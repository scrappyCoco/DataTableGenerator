using System;
using System.Collections;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using Coding4fun.DataTools.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Coding4fun.DataTools.Test.Infrastructure
{
    internal static class CompilationUtil
    {
        internal static Compilation CreateCompilation(string source)
        {
            Assembly netStandardAssembly = AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "netstandard");
            Assembly runtimeAssembly = AppDomain.CurrentDomain.GetAssemblies().First(a => "System.Runtime".Equals(a.GetName().Name));
            Assembly linqAssembly = AppDomain.CurrentDomain.GetAssemblies().First(a => "System.Linq".Equals(a.GetName().Name));

            return CSharpCompilation.Create("compilation",
                new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp9)) },
                new[]
                {
                    MetadataReference.CreateFromFile(netStandardAssembly.Location),
                    MetadataReference.CreateFromFile(runtimeAssembly.Location),
                    MetadataReference.CreateFromFile(linqAssembly.Location),
                    MetadataReference.CreateFromFile(typeof(DataTable).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(GCLatencyMode).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Expression).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Func<string,string>).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(MinLengthAttribute).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(TableBuilder<int>).Assembly.Location)
                },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));
        }

        internal static Compilation RunGenerators(Compilation compilation, out ImmutableArray<Diagnostic> diagnostics, params ISourceGenerator[] generators)
        {
            CSharpGeneratorDriver.Create(generators).RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out diagnostics);
            return newCompilation;
        }
    }
}