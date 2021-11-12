
using Coding4fun.DataTools.Analyzers;
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
}