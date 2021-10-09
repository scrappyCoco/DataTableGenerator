using System.IO;
using System.Linq;
using Coding4fun.DataTableGenerator.SourceGenerator;
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
        public Guid Id { get; set; }
        public short Age { get; set; }
        [MaxLength(50)]
        public string FirstName { get; set; }
        [MaxLength(50)]
        public string LastName { get; set; }
        [MinLength(2)]
        [MaxLength(2)]
        public string CountryCode { get; set; }
        public List<Job> Jobs { get; set; }
    }

    public class Job
    {
        public Guid PersonId { get; set; }
        public int Number { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
    }

    public partial class PersonSqlMapping
    {
        [SqlMappingDeclaration]  
        private void Initialize()
        {
            new TableBuilder<Person>(NamingConvention.SnakeCase)
                .AddPreExecutionAction(person =>
                {
                    Console.WriteLine(person.LastName + "" "" + person.FirstName);
                })
                .SetName(""#MY_PERSON"")
                .AddColumn(person => person.Id)
                .AddColumn(person => person.Age)
                .AddColumn(person => person.FirstName)
                .AddColumn(person => person.LastName)
                .AddColumn(person => person.CountryCode, columnName:""COUNTRY"")
                .AddSubTable(person => person.Jobs, jobBuilder => jobBuilder
                    .AddPreExecutionAction((job, person) =>
                    {
                        job.PersonId = person.Id;
                    })
                    .AddColumn(job => job.PersonId)
                    .AddColumn(job => job.CompanyName, ""VARCHAR(100)"")
                    .AddColumn(job => job.Address, ""VARCHAR(200)"")
                );
        }
    }
}
";
            var compilation = CompilationUtil.CreateCompilation(userSource);
            var newCompilation = CompilationUtil.RunGenerators(compilation, out _, new DataTableSourceGenerator());

            var newFile = newCompilation.SyntaxTrees
                .Single(x => Path.GetFileName(x.FilePath).EndsWith(".Generated.cs"));

            Assert.NotNull(newFile);

            var generatedText = newFile.GetText().ToString().Trim();
            
            string expectedOutput = @"
using Coding4fun.DataTableGenerator.Common;
using System.Collections.Generic;
using System.Data;

namespace MyExample
{
    public partial class PersonSqlMapping
    {
        public DataTable PersonDataTable { get; } = new DataTable();
        public DataTable JobDataTable { get; } = new DataTable();

        public string GetSqlTableDefinition() => @""
CREATE TABLE #MY_PERSON
(
    id UNIQUEIDENTIFIER,
    age SMALLINT,
    first_name NVARCHAR(50),
    last_name NVARCHAR(50),
    country NCHAR(2)
);

CREATE TABLE #job
(
    person_id UNIQUEIDENTIFIER,
    company_name VARCHAR(100),
    address VARCHAR(200)
);
"";

        public void FillDataTables(IEnumerable<Person> items)
        {
            foreach (var person in items)
            {
                Console.WriteLine(person.LastName + "" "" + person.FirstName);
                AddPerson(person);
                foreach (var job in person.Jobs)
                {
                    job.PersonId = person.Id;
                    AddJob(job);
                }
            }
        }

        public void AddPerson(Person person)
        {
            PersonDataTable.Rows.Add(
                person.Id,
                person.Age,
                person.FirstName,
                person.LastName,
                person.CountryCode
            );
        }

        public void AddJob(Job job)
        {
            JobDataTable.Rows.Add(
                job.PersonId,
                job.CompanyName,
                job.Address
            );
        }
    }
}".Trim();

            Assert.AreEqual(expectedOutput, generatedText);
        }
    }
}