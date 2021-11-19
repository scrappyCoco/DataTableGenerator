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
        public short Age { get; set; }
        [MaxLength(50)]
        public string FirstName { get; set; }
        [MaxLength(50)]
        public string LastName { get; set; }
        [MinLength(2)]
        [MaxLength(2)]
        public string CountryCode { get; set; }
        public Job[] Jobs { get; set; }
        public Contact Contact { get; set; }
    }

    public class Job
    {
        public Guid PersonId { get; set; }
        public int Number { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
    }
    
    public class Contact
    {
        public string Phone { get; set; }
        public EmailContact[] Emails { get; set; }
    }

    public class EmailContact
    {
        public Guid PersonId { get; set; }
        public string Email { get; set; }
    }

    public partial class PersonSqlMapping
    {
        [SqlMappingDeclaration]  
        private void Initialize()
        {
            new TableBuilder<Person>(NamingConvention.SnakeCase)
                .AddPreExecutionAction((Person person) =>
                {
                    Console.WriteLine(person.LastName + " " + person.FirstName);
                })
                .SetName("#MY_PERSON")
                .AddColumn((Person person) => person.Id)
                .AddColumn((Person person) => person.Age)
                .AddColumn((Person person) => person.FirstName)
                .AddColumn((Person person) => person.LastName)
                .AddColumn((Person person) => person.CountryCode, columnName:"COUNTRY")
                .AddColumn((Person person) => person.Contact.Phone)
                .AddSubTable((Person person) => person.Jobs, jobBuilder => jobBuilder
                    .AddPreExecutionAction((Job job, Person person) =>
                    {
                        job.PersonId = person.Id;
                    })
                    .AddColumn((Job job) => job.PersonId)
                    .AddColumn((Job job) => job.CompanyName, "VARCHAR(100)")
                    .AddColumn((Job job) => job.Address, "VARCHAR(200)")
                );
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