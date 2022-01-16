
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
    public partial class PersonSqlMapping : MsSqlBatchHandler
    {
        private readonly DataTable _PersonDataTable;
        private readonly DataTable _JobDataTable;
        private readonly DataTable _SkillDataTable;

        public PersonSqlMapping(MsSqlConfig config) : base(config, CreateDataTables())
        {
            _PersonDataTable = DataTables.First(t => t.TableName == "#MY_PERSON");
            _JobDataTable = DataTables.First(t => t.TableName == "#job");
            _SkillDataTable = DataTables.First(t => t.TableName == "#skill");
        }

        /// <inheritdoc />
        protected /*override*/ string TempTableDeclaration => @"
CREATE TABLE #MY_PERSON
(
  id UNIQUEIDENTIFIER,
  age SMALLINT,
  first_name NVARCHAR(50),
  last_name NVARCHAR(50),
  country NCHAR(2),
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

        /// <inheritdoc />
        protected /*override*/ string ProcedureName => "dbo.InsertPerson";

        private static DataTable[] CreateDataTables()
        {
            DataTable PersonDataTable = new DataTable("#MY_PERSON");
            PersonDataTable.Columns.Add("id", typeof(System.Guid));
            PersonDataTable.Columns.Add("age", typeof(short));
            PersonDataTable.Columns.Add("first_name", typeof(string));
            PersonDataTable.Columns.Add("last_name", typeof(string));
            PersonDataTable.Columns.Add("country", typeof(string));
            PersonDataTable.Columns.Add("phone", typeof(string));
            PersonDataTable.Columns.Add("photo", typeof(byte[]));

            DataTable JobDataTable = new DataTable("#job");
            JobDataTable.Columns.Add("person_id", typeof(System.Guid));
            JobDataTable.Columns.Add("company_name", typeof(string));
            JobDataTable.Columns.Add("address", typeof(string));

            DataTable SkillDataTable = new DataTable("#skill");
            SkillDataTable.Columns.Add("person_id", typeof(System.Guid));
            SkillDataTable.Columns.Add("tag", typeof(string));


            return new[] { PersonDataTable, JobDataTable, SkillDataTable };
        }

        /// <inheritdoc />
        protected /*override*/ void AddEntityInternal(object entity)
        {
            AddPerson((Person)entity);
        }

        private void AddPerson(Person person)
        {
            _PersonDataTable.Rows.Add(
                person.Id,
                person.Age,
                person.FirstName,
                person.LastName,
                person.CountryCode,
                person.Contact.Phone,
                person.Photo
            );
        }

        private void AddJob(Job job)
        {
            _JobDataTable.Rows.Add(
                job.PersonId,
                job.CompanyName,
                job.Address
            );
        }

        private void AddSkill(Skill skill)
        {
            _SkillDataTable.Rows.Add(
                skill.PersonId,
                skill.Tag
            );
        }
    }
}
