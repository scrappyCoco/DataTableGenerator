using System.Threading.Tasks;
using Coding4fun.DataTools.Analyzers;
using Coding4fun.DataTools.SourceGeneratorTest.Infrastructure;
using NUnit.Framework;

namespace Coding4fun.DataTools.SourceGeneratorTest
{
    public class AnalyzerTests
    { [Test]
        public async Task TestPositive()
        {
            // new TableBuilder<Person>(); // must be triggered.
            string source = await TestSourceUtils.Loadsync("TestSource", "BasicTableBuilder.cs");
            await CSharpAnalyzerVerifier<TableBuilderAnalyzer>.VerifyAnalyzerAsync(source);
        }
        
        [Test]
        public async Task TestNegative()
        {
            // List<Person>(); // must not be triggered.
            string source = await TestSourceUtils.Loadsync("TestSource", "EmptyList.cs");
            await CSharpAnalyzerVerifier<TableBuilderAnalyzer>.VerifyAnalyzerAsync(source);
        }

        [Test]
        public async Task TestCodeFix()
        {
            string source = await TestSourceUtils.Loadsync("TestSource", "CodeFix", "Source.cs");
            string target = await TestSourceUtils.Loadsync("TestSource", "CodeFix", "Target.cs");
            await CSharpCodeFixVerifier<TableBuilderAnalyzer, TableBuilderCodeFixProvider>.VerifyCodeFixAsync(source, target);
        }
    }
}   