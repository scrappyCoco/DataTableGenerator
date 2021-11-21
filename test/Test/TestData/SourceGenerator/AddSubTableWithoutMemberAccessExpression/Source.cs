using Coding4fun.DataTools.Analyzers.StringUtil;
using Coding4fun.DataTools.Analyzers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System;

namespace Coding4fun.DataTools.Test.TestData.SourceGenerator
{
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
            new TableBuilder<Person>().AddSubTable<Job>((Person person) =>/*[__ERROR__*/new Job[] { }/*__ERROR__]*/, jobBuilder => jobBuilder
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