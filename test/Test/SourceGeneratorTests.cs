using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Coding4fun.DataTools.Analyzers;
using Coding4fun.DataTools.Test.Infrastructure;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace Coding4fun.DataTools.Test
{
    public class SourceGeneratorTests : SourceGeneratorTest<DataTableSourceGenerator>
    {
        [SetUp]
        public void Setup()
        {
        }

        private async Task TestSuccessAsync([CallerMemberName] string? methodName = null)
        {
            string source = await LoadAsync("Source.cs", methodName);
        
            Compilation compilation = CompilationUtil.CreateCompilation(source);
            Diagnostic[] compilationErrors = compilation.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToArray();
            
            if (compilationErrors.Any())
            {
                Assert.Fail($"Errors count: {compilationErrors.Length}, first error: {compilationErrors.First()}");
            }
            
            var newCompilation = CompilationUtil.RunGenerators(compilation, out var diagnostics, new DataTableSourceGenerator());

            var newFile = newCompilation.SyntaxTrees
                .Single(x => Path.GetFileName(x.FilePath).EndsWith(".Generated.cs"));
        
            Assert.NotNull(newFile);
        
            var generatedText = (await newFile.GetTextAsync()).ToString().Trim();
        
            string expectedOutput = await LoadAsync("Target.cs", methodName);
        
            AssertSourceCode(expectedOutput, generatedText);
        }
        
        [Test]
        public async Task Success() => await TestSuccessAsync();
        
        [Test]
        public async Task TableNameCamelCase() => await TestSuccessAsync();
        
        [Test]
        public async Task TableNamePascalCase() => await TestSuccessAsync();
        
        [Test]
        public async Task TableNameScreamingSnakeCase() => await TestSuccessAsync();
        
        [Test]
        public async Task TableNameSnakeCase() => await TestSuccessAsync();
        
        [Test]
        public async Task AllSimpleTypes() => await TestSuccessAsync();

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
        public async Task SimpleLambdaExpressionSubTable() =>
            await AssertDiagnosticAsync(Messages.GetLambdaWithoutType());

        [Test]
        public async Task LambdaExpressionWithoutType() =>
            await AssertDiagnosticAsync(Messages.GetLambdaWithoutType());
        
        [Test]
        public async Task AddColumnWithoutMemberAccessExpression() =>
            await AssertDiagnosticAsync(Messages.GetMemberAccessExpression());
        
        [Test]
        public async Task AddSubTableWithoutMemberAccessExpression() =>
            await AssertDiagnosticAsync(Messages.GetMemberAccessExpression());
        
        [Test]
        public async Task MethodCallInsteadOfProperty() =>
            await AssertDiagnosticAsync(Messages.GetUnableToResolveProperty("GetFirstName", "Person"));
        
        [Test]
        public async Task MethodCallInsteadOfPropertySubTable() =>
            await AssertDiagnosticAsync(Messages.GetUnableToResolveProperty("GetJobs", "Person"));
        
        [Test]
        public async Task ArrayOfString() => await AssertDiagnosticAsync(Messages.GetInvalidType());
        
        [Test]
        public async Task InvalidSubTable() => await AssertDiagnosticAsync(Messages.GetMemberAccessExpression());
        
        [Test]
        public async Task SyntaxError() => await AssertDiagnosticAsync();
    }
}