using System.Threading.Tasks;
using Coding4fun.DataTools.Analyzers;
using Coding4fun.DataTools.Test.Infrastructure;
using NUnit.Framework;

namespace Coding4fun.DataTools.Test.Analyzer
{
    public class Tests: TestBase
    {
        [Test]
        public async Task BasicTableBuilder()
        {
            // new TableBuilder<Person>(); // must be triggered.
            string source = await LoadAsync();
            await CSharpAnalyzerVerifier<TableBuilderAnalyzer>.VerifyAnalyzerAsync(source);
        }
        
        [Test]
        public async Task EmptyList()
        {
            // List<Person>(); // must not be triggered.
            string source = await LoadAsync();
            await CSharpAnalyzerVerifier<TableBuilderAnalyzer>.VerifyAnalyzerAsync(source);
        }
    }
}   