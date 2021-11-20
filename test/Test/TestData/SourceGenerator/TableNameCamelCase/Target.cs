using Coding4fun.DataTools.Analyzers;
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

        public PersonSqlMapping()
        {
            PersonDataTable.Columns.Add("id", typeof(System.Guid));
        }
  
        public string GetSqlTableDefinition() => @"
CREATE TABLE #person
(
  id UNIQUEIDENTIFIER
);
";

        public void FillDataTables(IEnumerable<Person> items)
        {
            foreach (var person in items)
            {
                AddPerson(person);
            }
        }

        public async Task BulkCopyAsync(SqlConnection targetConnection)
        {
            using (SqlBulkCopy personSqlBulkCopy = new SqlBulkCopy(targetConnection))
            {
                personSqlBulkCopy.DestinationTableName = "#person";
                await personSqlBulkCopy.WriteToServerAsync(PersonDataTable);
            }
        }

        public void AddPerson(Person person)
        {
            PersonDataTable.Rows.Add(
                person.Id
            );
        }
    }
}