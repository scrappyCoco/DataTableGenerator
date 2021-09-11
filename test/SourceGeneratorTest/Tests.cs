using System.Collections.Immutable;
using System.Data;
using System.IO;
using System.Linq;
using Coding4fun.DataTableGenerator.Common;
using Coding4fun.DataTableGenerator.SourceGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace SourceGeneratorTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test()
        {
            const string userSource = @"
using Coding4fun.DataTableGenerator.Common;

namespace MyExample
{
    public class Person
    {
        public short Age { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CountryCode { get; set; }
        public List<Job> Jobs { get; set; }
        public List<string> Skills { get; set; }
    }

    public class Job
    {
        public string CompanyName { get; set; }
        public string Address { get; set; }
    }


    public partial class PersonSqlMapping
    {
        static PersonSqlMapping()
        {
            new DataTableBuilder<Person>(""#PERSON"")
                .AddColumn(""AGE"", ""SMALLINT"", p => p.Age)
                .AddColumn(""FIRST_NAME"", ""VARCHAR(50)"", p => p.FirstName)
                .AddColumn(""LAST_NAME"", ""VARCHAR(50)"", p => p.LastName)
                .AddColumn(""COUNTRY_CODE"", ""CHAR(2)"", p => p.CountryCode)
                .AddSubTable<Job>(""#JOB_HISTORY"", p => p.Jobs, jobBuilder => jobBuilder
                    .AddColumn(""COMPANY_NAME"", ""VARCHAR(100)"", j => j.CompanyName)
                    .AddColumn(""ADDRESS"", ""VARCHAR(200)"", j => j.Address)
                ).AddBasicSubTable<string>(""#SKILL"", ""VALUE"", ""VARCHAR(100)"", p => p.Skills);
        }
    }
}
";
            var comp = CreateCompilation(userSource);
            var newComp = RunGenerators(comp, out _, new DataTableSourceGenerator());

            var newFile = newComp.SyntaxTrees
                .Single(x => Path.GetFileName(x.FilePath).EndsWith(".Generated.cs"));

            Assert.NotNull(newFile);

            var generatedText = newFile.GetText().ToString().Replace("\r\n", "\n").Trim();

            var expectedOutput = @"
using Coding4fun.DataTableGenerator.Common;
using System.Collections.Generic;
using System.Data;

namespace MyExample

{
    public partial class PersonSqlMapping
    {
        public DataTable PERSON;
        public DataTable JOB_HISTORY;
        public DataTable SKILL;
        

        public PersonSqlMapping()
        {
            PERSON = new DataTable();
            PERSON.Columns.Add(""AGE"", typeof(short));
            PERSON.Columns.Add(""FIRST_NAME"", typeof(string));
            PERSON.Columns.Add(""LAST_NAME"", typeof(string));
            PERSON.Columns.Add(""COUNTRY_CODE"", typeof(string));
            
            JOB_HISTORY = new DataTable();
            JOB_HISTORY.Columns.Add(""COMPANY_NAME"", typeof(string));
            
            SKILL = new DataTable();
            SKILL.Columns.Add(""VALUE"", typeof(string));
            
        }

        public string GetSqlTableDefinition() => @""
CREATE TABLE #PERSON (
    AGE SMALLINT,
    FIRST_NAME VARCHAR(50),
    LAST_NAME VARCHAR(50),
    COUNTRY_CODE CHAR(2)
);
CREATE TABLE #JOB_HISTORY (
    COMPANY_NAME VARCHAR(100)
);
CREATE TABLE #SKILL (
    VALUE VARCHAR(100)
);
"";

        public void FillDataTables(IEnumerable<Person> items)
        {
            PERSON.Clear();
            JOB_HISTORY.Clear();
            SKILL.Clear();
            

            foreach (var item in items)
            {
                AddPERSON(item);
            }
        }

        public void AddPERSON(Person p){
            PERSON.Rows.Add(
                p.Age,
                p.FirstName,
                p.LastName,
                p.CountryCode);
            foreach (var subItem in p.Jobs)
            {
                AddJOB_HISTORY(subItem);
            }
            foreach (var subItem in p.Skills)
            {
                AddSKILL(subItem);
            }
        }
        
        public void AddJOB_HISTORY(Job j){
            JOB_HISTORY.Rows.Add(
                j.CompanyName);
        }
        
        public void AddSKILL(string p){
            SKILL.Rows.Add(
                );
        }
        
    }
}
".Replace("\r\n", "\n").Trim();

            Assert.AreEqual(expectedOutput, generatedText);
        }

        // Thanks for example: https://github.com/TessenR/NotifyPropertyChangedDemo
        private static Compilation CreateCompilation(string source)
            => CSharpCompilation.Create("compilation",
                new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp9)) },
                new[]
                {
                    MetadataReference.CreateFromFile(typeof(DataTable).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(DataTableBuilder<int>).Assembly.Location)
                },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));

        private static GeneratorDriver CreateDriver(params ISourceGenerator[] generators)
            => CSharpGeneratorDriver.Create(generators);

        private static Compilation RunGenerators(Compilation compilation, out ImmutableArray<Diagnostic> diagnostics,
            params ISourceGenerator[] generators)
        {
            CreateDriver(generators)
                .RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out diagnostics);
            return newCompilation;
        }
    }
}