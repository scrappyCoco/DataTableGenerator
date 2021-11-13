using System.Threading.Tasks;
using Coding4fun.DataTools.Analyzers;
using Coding4fun.DataTools.Test.Infrastructure;
using NUnit.Framework;

namespace Coding4fun.DataTools.Test
{
    public class AnalyzerTests: TestBase
    { [Test]
        public async Task TestPositive()
        {
            // new TableBuilder<Person>(); // must be triggered.
            string source = await LoadAsync();
            await CSharpAnalyzerVerifier<TableBuilderAnalyzer>.VerifyAnalyzerAsync(source);
        }
        
        [Test]
        public async Task TestNegative()
        {
            // List<Person>(); // must not be triggered.
            string source = await LoadAsync();
            await CSharpAnalyzerVerifier<TableBuilderAnalyzer>.VerifyAnalyzerAsync(source);
        }

        [Test]
        public async Task TestCodeFix()
        {
            string source = await LoadAsync();
            string target = await LoadAsync();
            await CSharpCodeFixVerifier<TableBuilderAnalyzer, TableBuilderCodeFixProvider>.VerifyCodeFixAsync(source, target);
        }
    }
}   