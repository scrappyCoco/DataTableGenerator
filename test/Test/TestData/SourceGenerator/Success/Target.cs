﻿using Coding4fun.DataTools.Analyzers;
using Coding4fun.DataTools.Analyzers.StringUtil;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Coding4fun.DataTools.Test.TestData.SourceGenerator
{
    public partial class PersonSqlMapping
    {
        public DataTable PersonDataTable { get; } = new DataTable();
        public DataTable JobDataTable { get; } = new DataTable();

        public PersonSqlMapping()
        {
            PersonDataTable.Columns.Add("id", typeof(Guid));
            PersonDataTable.Columns.Add("age", typeof(short));
            PersonDataTable.Columns.Add("first_name", typeof(string));
            PersonDataTable.Columns.Add("last_name", typeof(string));
            PersonDataTable.Columns.Add("country", typeof(string));

            JobDataTable.Columns.Add("person_id", typeof(Guid));
            JobDataTable.Columns.Add("company_name", typeof(string));
            JobDataTable.Columns.Add("address", typeof(string));
        }
  
        public string GetSqlTableDefinition() => @"
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
";

        public void FillDataTables(IEnumerable<Person> items)
        {
            foreach (var person in items)
            {
                Console.WriteLine(person.LastName + " " + person.FirstName);
                AddPerson(person);
                foreach (var job in person.Jobs)
                {
                    job.PersonId = person.Id;
                    AddJob(job);
                }
            }
        }

        public async Task BulkCopyAsync(SqlConnection targetConnection)
        {
            using (SqlBulkCopy personSqlBulkCopy = new SqlBulkCopy(targetConnection))
            {
                personSqlBulkCopy.DestinationTableName = "#MY_PERSON";
                await personSqlBulkCopy.WriteToServerAsync(PersonDataTable);
            }
            using (SqlBulkCopy jobSqlBulkCopy = new SqlBulkCopy(targetConnection))
            {
                jobSqlBulkCopy.DestinationTableName = "#job";
                await jobSqlBulkCopy.WriteToServerAsync(JobDataTable);
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
}