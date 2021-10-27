using System.Threading.Tasks;
using Coding4fun.DataTools.Analyzers;
using NUnit.Framework;

namespace SourceGeneratorTest
{
    public class AnalyzerTests
    {
        [Test]
        public async Task Test()
        {
            const string source = @"
using System;

class Program
{
    static void Main()
    {
        [|int i = 0;|]
        Console.WriteLine(i);
    }
}";
            await CSharpAnalyzerVerifier<MakeConstAnalyzer>.VerifyAnalyzerAsync(source);
        }
    }
}