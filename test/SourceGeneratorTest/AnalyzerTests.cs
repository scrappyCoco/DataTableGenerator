using System.Threading.Tasks;
using Coding4fun.DataTools.Analyzers;
using NUnit.Framework;

namespace SourceGeneratorTest
{
    public class AnalyzerTests
    {
        [Test]
        public async Task TestPositive()
        {
            const string source = @"
class TableBuilder<TItem> { }
class Person { }

class Program
{
    static void Main()
    {
        var tableBuilder = [|new TableBuilder<Person>()|];
    }
}";
            await CSharpAnalyzerVerifier<TableBuilderAnalyzer>.VerifyAnalyzerAsync(source);
        }
        
        [Test]
        public async Task TestNegative()
        {
            const string source = @"
using System.Collections.Generic;

class TableBuilder<TItem> { }
class Person { }

class Program
{
    static void Main()
    {
        var list = new List<Person>();
    }
}";
            await CSharpAnalyzerVerifier<TableBuilderAnalyzer>.VerifyAnalyzerAsync(source);
        }

        [Test]
        public async Task Test()
        {
            await CSharpCodeFixVerifier<TableBuilderAnalyzer, TableBuilderCodeFixProvider>.VerifyCodeFixAsync(@"
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

class TableBuilder<TItem>
{
    public TableBuilder<TItem> AddColumn(
      Expression<Func<TItem, object>> valueGetter,
      string sqlType = null,
      string columnName = null)
    {
      return this;
    }
}
class Person
{
    public int Age { get; set; }
}

class Program
{
    static void Main()
    {
        var tableBuilder = [|new TableBuilder<Person>()|];
    }
}
", @"
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

class TableBuilder<TItem>
{
    public TableBuilder<TItem> AddColumn(
      Expression<Func<TItem, object>> valueGetter,
      string sqlType = null,
      string columnName = null)
    {
      return this;
    }
}
class Person
{
    public int Age { get; set; }
}

class Program
{
    static void Main()
    {
        new TableBuilder<Person>().AddColumn(person => person.Age);
    }
}
");
        }
    }
}