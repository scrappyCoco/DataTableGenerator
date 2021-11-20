using Coding4fun.DataTools.Analyzers.StringUtil;
using Coding4fun.DataTools.Analyzers;
using System;
using System.Linq.Expressions;

namespace Coding4fun.DataTools.Test.TestData.SourceGenerator
{
    public class SqlMappingDeclarationAttribute : System.Attribute
    {
    }

    public class TableBuilder<TItem>
    {
        public TableBuilder<TItem> AddColumn(Expression<Func<TItem, object>> valueGetter) => this;
    }

    public class Person
    {
        public string FirstName { get; set; }
    }

    public class Job
    {
        public Guid PersonId { get; set; }
    }


    public partial class PersonSqlMapping
    {
        [SqlMappingDeclaration]
        private void Initialize()
        {
            new TableBuilder<Person>().AddSubTable((Person person) => new Job[] { }, jobBuilder => jobBuilder
                .AddColumn((Job job) => job.PersonId)
            );
        }
    }

    class Program
    {
        static void Main()
        {
        }
    }
}