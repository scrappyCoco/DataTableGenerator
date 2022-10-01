
using Coding4fun.DataTools.Analyzers;
using Coding4fun.DataTools.Analyzers.StringUtil;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Coding4fun.DataTools.Test.TestData.SourceGenerator
{
    public partial class PersonSqlMapping
    {
        public DataTable PersonDataTable { get; } = new DataTable();
        public DataTable JobDataTable { get; } = new DataTable();
        public DataTable SkillDataTable { get; } = new DataTable();

        public PersonSqlMapping()
        {
            PersonDataTable.Columns.Add("id", typeof(System.Guid));
            PersonDataTable.Columns.Add("age", typeof(short));
            PersonDataTable.Columns.Add("first_name", typeof(string));
            PersonDataTable.Columns.Add("last_name", typeof(string));
            PersonDataTable.Columns.Add("COUNTRY", typeof(string));
            PersonDataTable.Columns.Add("phone", typeof(string));
            PersonDataTable.Columns.Add("photo", typeof(byte[]));

            JobDataTable.Columns.Add("person_id", typeof(System.Guid));
            JobDataTable.Columns.Add("company_name", typeof(string));
            JobDataTable.Columns.Add("address", typeof(string));

            SkillDataTable.Columns.Add("person_id", typeof(System.Guid));
            SkillDataTable.Columns.Add("tag", typeof(string));
        }
  
        public string GetSqlTableDefinition() => @"
CREATE TABLE #MY_PERSON
(
  id UNIQUEIDENTIFIER,
  age SMALLINT,
  first_name NVARCHAR(50),
  last_name NVARCHAR(50),
  COUNTRY NCHAR(2),
  phone NVARCHAR(MAX),
  photo VARBINARY(MAX)
);
CREATE TABLE #job
(
  person_id UNIQUEIDENTIFIER,
  company_name VARCHAR(100),
  address VARCHAR(200)
);
CREATE TABLE #skill
(
  person_id UNIQUEIDENTIFIER,
  tag VARCHAR(100)
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
                foreach (var skill in person.SkillValues)
                {
                    AddSkill(skill);
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
            using (SqlBulkCopy skillSqlBulkCopy = new SqlBulkCopy(targetConnection))
            {
                skillSqlBulkCopy.DestinationTableName = "#skill";
                await skillSqlBulkCopy.WriteToServerAsync(SkillDataTable);
            }
        }

        public void AddPerson(Person person)
        {
            PersonDataTable.Rows.Add(
                person.Id,
                person.Age,
                person.FirstName,
                person.LastName,
                person.CountryCode,
                person.Contact.Phone,
                person.Photo
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

        public void AddSkill(Skill skill)
        {
            SkillDataTable.Rows.Add(
                skill.PersonId,
                skill.Tag
            );
        }
    }
}
