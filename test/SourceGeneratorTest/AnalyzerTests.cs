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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

class TableBuilder<TItem>
{
    public TableBuilder<TItem> AddColumn(
        Expression<Func<TItem, object>> valueGetter,
        string sqlType = null,
        string columnName = null) => this;

    public TableBuilder<TItem> AddSubTable<TSubItem>(
        Expression<Func<TItem, IEnumerable<TSubItem>>> enumerableGetter,
        Action<SubTableBuilder<TSubItem, TItem>> subTableConsumer) => this;
}

class SubTableBuilder<TItem, TParentItem>
{
    public SubTableBuilder<TItem, TParentItem> AddSubTable<TSubItem>(
        Expression<Func<TItem, IEnumerable<TSubItem>>> enumerableGetter,
        Action<SubTableBuilder<TSubItem, TItem>> subTableConsumer) => this;

    public SubTableBuilder<TItem, TParentItem> AddColumn(
        Expression<Func<TItem, object>> valueGetter,
        string sqlType = null,
        string customSqlColumnName = null) => this;
}

public class Person
{
    public Guid Id { get; set; }
    public short Age { get; set; }
    [MaxLength(50)]
    public string FirstName { get; set; }
    [MaxLength(50)]
    public string LastName { get; set; }
    [MinLength(2)]
    [MaxLength(2)]
    public string CountryCode { get; set; }
    public Job[] Jobs { get; set; }
    public byte[] Logo { get; set; }
    
    public string[] Skills { get; set; }

    // Enumerable of basic types should be mapped to complex types with defined relations.
    public IEnumerable<Skill> SkillValues => Skills.Select(skill => new Skill(Id, skill));
}

public class Job
{
    public Guid PersonId { get; set; }
    public string CompanyName { get; set; }
    public string Address { get; set; }
}

public class Skill
{
    public Skill(Guid personId, string tag)
    {
        PersonId = personId;
        Tag = tag;
    }

    public Guid PersonId { get; set; }
    public string Tag { get; set; }
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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

class TableBuilder<TItem>
{
    public TableBuilder<TItem> AddColumn(
        Expression<Func<TItem, object>> valueGetter,
        string sqlType = null,
        string columnName = null) => this;

    public TableBuilder<TItem> AddSubTable<TSubItem>(
        Expression<Func<TItem, IEnumerable<TSubItem>>> enumerableGetter,
        Action<SubTableBuilder<TSubItem, TItem>> subTableConsumer) => this;
}

class SubTableBuilder<TItem, TParentItem>
{
    public SubTableBuilder<TItem, TParentItem> AddSubTable<TSubItem>(
        Expression<Func<TItem, IEnumerable<TSubItem>>> enumerableGetter,
        Action<SubTableBuilder<TSubItem, TItem>> subTableConsumer) => this;

    public SubTableBuilder<TItem, TParentItem> AddColumn(
        Expression<Func<TItem, object>> valueGetter,
        string sqlType = null,
        string customSqlColumnName = null) => this;
}

public class Person
{
    public Guid Id { get; set; }
    public short Age { get; set; }
    [MaxLength(50)]
    public string FirstName { get; set; }
    [MaxLength(50)]
    public string LastName { get; set; }
    [MinLength(2)]
    [MaxLength(2)]
    public string CountryCode { get; set; }
    public Job[] Jobs { get; set; }
    public byte[] Logo { get; set; }
    
    public string[] Skills { get; set; }

    // Enumerable of basic types should be mapped to complex types with defined relations.
    public IEnumerable<Skill> SkillValues => Skills.Select(skill => new Skill(Id, skill));
}

public class Job
{
    public Guid PersonId { get; set; }
    public string CompanyName { get; set; }
    public string Address { get; set; }
}

public class Skill
{
    public Skill(Guid personId, string tag)
    {
        PersonId = personId;
        Tag = tag;
    }

    public Guid PersonId { get; set; }
    public string Tag { get; set; }
}

class Program
{
    static void Main()
    {
        new TableBuilder<Person>()
            .AddColumn(person => person.Id)
            .AddColumn(person => person.Age)
            .AddColumn(person => person.FirstName)
            .AddColumn(person => person.LastName)
            .AddColumn(person => person.CountryCode)
            .AddColumn(person => person.Logo)
            .AddSubTable(person => person.Jobs, jobBuilder => jobBuilder
                .AddColumn(job => job.PersonId)
                .AddColumn(job => job.CompanyName)
                .AddColumn(job => job.Address)
            )
            .AddSubTable(person => person.SkillValues, skillBuilder => skillBuilder
                .AddColumn(skill => skill.PersonId)
                .AddColumn(skill => skill.Tag)
            );
    }
}
");
        }
    }
}