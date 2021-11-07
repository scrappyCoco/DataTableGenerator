using System.Collections.Immutable;
using Coding4fun.DataTools.Analyzers;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace SourceGeneratorTest
{
    public class ErrorTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test()
        {
            const string userSource = @"
using Coding4fun.DataTools.Analyzers;

namespace MyExample
{
    public partial class PersonSqlMapping
    {
        [SqlMappingDeclaration]  
        private void Initialize()
        {
            
        }
    }
}
";

            var compilation = CompilationUtil.CreateCompilation(userSource);
            CompilationUtil.RunGenerators(compilation, out ImmutableArray<Diagnostic> diagnostics,
                new DataTableSourceGenerator());
            
            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual("Unable to find definition of TableBuilder<TItem>.", diagnostics[0].GetMessage());
        }
    }
}