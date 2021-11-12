using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Coding4fun.DataTools.Analyzers;
using Coding4fun.DataTools.SourceGeneratorTest.Infrastructure;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace Coding4fun.DataTools.SourceGeneratorTest
{
    public class DataTableSourceGeneratorTests
    {
        [SetUp]
        public void Setup()
        {
            
        }

        [Test]
        public async Task Test()
        {
            string source = await TestSourceUtils.Loadsync("TestSource", "SourceGenerator", "Source.cs");
            
            var compilation = CompilationUtil.CreateCompilation(source);
            var newCompilation = CompilationUtil.RunGenerators(compilation, out ImmutableArray<Diagnostic> diagnostics, new DataTableSourceGenerator());

            var newFile = newCompilation.SyntaxTrees
                .Single(x => Path.GetFileName(x.FilePath).EndsWith(".Generated.cs"));

            Assert.NotNull(newFile);

            var generatedText = newFile.GetText().ToString().Trim();
            
            string expectedOutput = await TestSourceUtils.Loadsync("TestSource", "SourceGenerator", "Target.cs");

            Assert.AreEqual(expectedOutput, generatedText);
        }
    }
}