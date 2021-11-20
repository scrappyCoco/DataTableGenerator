#nullable disable

using Coding4fun.DataTools.Analyzers;
using Coding4fun.DataTools.Analyzers.StringUtil;
using System;
using System.ComponentModel.DataAnnotations;

namespace Coding4fun.DataTools.Test.TestData.SourceGenerator
{
    public class Person
    {
        public Guid Id { get; set; }
    }

    public partial class PersonSqlMapping
    {
        [SqlMappingDeclaration]  
        private void Initialize()
        {
            new TableBuilder<Person>(NamingConvention.CamelCase)
                .AddColumn((Person person) => person.Id);
        }
    }

    static class Program
    {
        static void Main()
        {
            new PersonSqlMapping();
        }
    }
}