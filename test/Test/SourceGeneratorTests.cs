using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Coding4fun.DataTools.Analyzers;
using Coding4fun.DataTools.Test.Infrastructure;
using NUnit.Framework;

namespace Coding4fun.DataTools.Test
{
    public class SourceGeneratorTests : SourceGeneratorTest<DataTableSourceGenerator>
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
            var newCompilation = CompilationUtil.RunGenerators(compilation, out var diagnostics, new DataTableSourceGenerator());
        
            var newFile = newCompilation.SyntaxTrees
                .Single(x => Path.GetFileName(x.FilePath).EndsWith(".Generated.cs"));
        
            Assert.NotNull(newFile);
        
            var generatedText = (await newFile.GetTextAsync()).ToString().Trim();
        
            string expectedOutput = await LoadAsync("Target.cs");
        
            AssertSourceCode(expectedOutput, generatedText);
        }

        [Test]
        public async Task EmptySqlMappingDeclaration() =>
            await AssertDiagnosticAsync(Messages.GetUnableToGetTableDefinition());

        [Test]
        public async Task EmptyTableBuilder() =>
            await AssertDiagnosticAsync(Messages.GetSqlMappingIsEmpty());
        
        [Test]
        public async Task SimpleLambdaExpression() =>
            await AssertDiagnosticAsync(Messages.GetLambdaWithoutType());
        
        [Test]
        public async Task LambdaExpressionWithoutType() =>
            await AssertDiagnosticAsync(Messages.GetLambdaWithoutType());
        
        [Test]
        public async Task WithoutMemberAccessExpression() =>
            await AssertDiagnosticAsync(Messages.GetMemberAccessExpression());
        
        [Test]
        public async Task SyntaxError() => await AssertDiagnosticAsync();
    }                                                                                                   
}