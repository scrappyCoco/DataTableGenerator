using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Coding4fun.DataTools.Analyzers;
using Coding4fun.DataTools.Test.Infrastructure;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace Coding4fun.DataTools.Test.SourceGenerator
{
    public class Tests : SourceGeneratorTest<DataTableSourceGenerator>
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task Success()
        {
            string source = await LoadAsync();
        
            var compilation = CompilationUtil.CreateCompilation(source);
            var newCompilation = CompilationUtil.RunGenerators(compilation, out ImmutableArray<Diagnostic> diagnostics,
                new DataTableSourceGenerator());
        
            var newFile = newCompilation.SyntaxTrees
                .Single(x => Path.GetFileName(x.FilePath).EndsWith(".Generated.cs"));
        
            Assert.NotNull(newFile);
        
            var generatedText = newFile.GetText().ToString().Trim();
        
            string expectedOutput = await LoadAsync("Target.cs");
        
            Assert.AreEqual(expectedOutput, generatedText);
        }

        [Test]
        public async Task EmptySqlMappingDeclaration() =>
            await AssertDiagnosticAsync(DataTableMessages.GetUnableToGetTableDefinition());

        [Test]
        public async Task EmptyTableBuilder() =>
            await AssertDiagnosticAsync(DataTableMessages.GetSqlMappingIsEmpty());
        
        [Test]
        public async Task NotResolvedType() =>
            await AssertDiagnosticAsync(DataTableMessages.GetUnableToGetTableTypeInfo());
        
        [Test]
        public async Task WithoutNamespace() =>
            await AssertDiagnosticAsync(DataTableMessages.GetUnableToFindNamespace());
        
        [Test]
        public async Task NotResolvedProperty() =>
            await AssertDiagnosticAsync(DataTableMessages.GetUnableToResolveProperty("Name", "Person"));
        
        [Test]
        public async Task ColumnWithoutLambdaBody() =>
            await AssertDiagnosticAsync(DataTableMessages.GetUnableToGetExpressionBody());
        
        [Test]
        public async Task SyntaxError() => await AssertDiagnosticAsync();

    }
}