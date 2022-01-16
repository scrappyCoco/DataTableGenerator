
using Coding4fun.DataTools.Analyzers;
using Coding4fun.DataTools.Analyzers.StringUtil;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Coding4fun.DataTools.Test.TestData.SourceGenerator
{
    public partial class PersonSqlMapping
    {
        public DataTable SomePersonDataTable { get; } = new DataTable();

        public PersonSqlMapping()
        {
            SomePersonDataTable.Columns.Add("id", typeof(System.Guid));
        }
  
        public string GetSqlTableDefinition() => @"
CREATE TABLE #somePerson
(
  id UNIQUEIDENTIFIER
);
";

        public void FillDataTables(IEnumerable<SomePerson> items)
        {
            foreach (var somePerson in items)
            {
                AddSomePerson(somePerson);
            }
        }

        public async Task BulkCopyAsync(SqlConnection targetConnection)
        {
            using (SqlBulkCopy somePersonSqlBulkCopy = new SqlBulkCopy(targetConnection))
            {
                somePersonSqlBulkCopy.DestinationTableName = "#somePerson";
                await somePersonSqlBulkCopy.WriteToServerAsync(SomePersonDataTable);
            }
        }

        public void AddSomePerson(SomePerson somePerson)
        {
            SomePersonDataTable.Rows.Add(
                somePerson.Id
            );
        }
    }
}
