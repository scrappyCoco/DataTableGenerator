using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Coding4fun.DataTools.Analyzers;
using Coding4fun.DataTools.Test.Infrastructure;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Coding4fun.DataTools.Test
{
    public class SourceGeneratorTests : Infrastructure.SourceGeneratorTest<DataTableSourceGenerator>
    {
        [SetUp]
        public void Setup()
        {
        }

        private async Task TestSuccessAsync([CallerMemberName] string methodName = null!)
        {
            string sourceCode = await LoadAsync("Source.cs", methodName);
            string targetCode = await LoadAsync("Target.cs", methodName);

            var test = new CSharpSourceGeneratorVerifier<DataTableSourceGenerator>.Test
            {
                TestState =
                {
                    Sources = { sourceCode },
                    GeneratedSources =
                    {
                        (typeof(DataTableSourceGenerator), "PersonSqlMapping.Generated.cs", targetCode)
                    },
                    MarkupHandling = MarkupMode.Allow,
                    AdditionalFiles =
                    {
                        ("Coding4fun.Test.DataTools", "")
                    }
                }
            };
            
            foreach (var (fileName, fileContent) in await LoadXmlFiles(methodName))
            {
                test.TestState.AdditionalFiles.Add((fileName, fileContent));
            }

            await test.RunAsync();
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
        public async Task CustomTemplate() => await TestSuccessAsync();

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
            await AssertDiagnosticAsync(Messages.Get19());
        
        [Test]
        public async Task AddSubTableWithoutMemberAccessExpression() =>
            await AssertDiagnosticAsync(Messages.Get20());
        
        [Test]
        public async Task MethodCallInsteadOfProperty() =>
            await AssertDiagnosticAsync(Messages.Get19());
        
        [Test]
        public async Task MethodCallInsteadOfPropertySubTable() =>
            await AssertDiagnosticAsync(Messages.Get20());
        
        [Test]
        public async Task ArrayOfString() => await AssertDiagnosticAsync(Messages.GetInvalidType());
        
        [Test]
        public async Task InvalidSubTable() => await AssertDiagnosticAsync(Messages.Get20());
        
        [Test]
        public async Task SyntaxError() => await AssertDiagnosticAsync();
    }
}