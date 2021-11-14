﻿#nullable disable

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
                .AddPreExecutionAction(person =>
                {
                    Console.WriteLine(person.LastName + " " + person.FirstName);
                })
                .SetName("#MY_PERSON")
                .AddColumn(person => person.Id)
                .AddColumn(person => person.Age)
                .AddColumn(person => person.FirstName)
                .AddColumn(person => person.LastName)
                .AddColumn(person => person.CountryCode, columnName:"COUNTRY")
                .AddColumn(person => person.Contact.Phone)
                .AddSubTable(person => person.Jobs, jobBuilder => jobBuilder
                    .AddPreExecutionAction((job, person) =>
                    {
                        job.PersonId = person.Id;
                    })
                    .AddColumn(job => job.PersonId)
                    .AddColumn(job => job.CompanyName, "VARCHAR(100)")
                    .AddColumn(job => job.Address, "VARCHAR(200)")
                ).AddSubTable(person => );
        }
    }
}