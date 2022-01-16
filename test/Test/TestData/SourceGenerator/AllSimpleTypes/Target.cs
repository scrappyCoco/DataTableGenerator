
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
        public DataTable PersonDataTable { get; } = new DataTable();

        public PersonSqlMapping()
        {
            PersonDataTable.Columns.Add("IT_IS_GUID", typeof(System.Guid));
            PersonDataTable.Columns.Add("IT_IS_DATE_TIME", typeof(System.DateTime));
            PersonDataTable.Columns.Add("IT_IS_DATE_TIME_OFFSET", typeof(System.DateTimeOffset));
            PersonDataTable.Columns.Add("IT_IS_DATE_TIME_SPAN", typeof(System.TimeSpan));
            PersonDataTable.Columns.Add("IT_IS_BOOL", typeof(bool));
            PersonDataTable.Columns.Add("IT_IS_BYTE", typeof(byte));
            PersonDataTable.Columns.Add("IT_IS_SHORT", typeof(short));
            PersonDataTable.Columns.Add("IT_IS_INT", typeof(int));
            PersonDataTable.Columns.Add("IT_IS_LONG", typeof(long));
            PersonDataTable.Columns.Add("IT_IS_DECIMAL", typeof(decimal));
            PersonDataTable.Columns.Add("IT_IS_DOUBLE", typeof(double));
            PersonDataTable.Columns.Add("IT_IS_SINGLE", typeof(float));
            PersonDataTable.Columns.Add("IT_IS_INT_16", typeof(short));
            PersonDataTable.Columns.Add("IT_IS_INT_32", typeof(int));
            PersonDataTable.Columns.Add("IT_IS_INT_64", typeof(long));
        }
  
        public string GetSqlTableDefinition() => @"
CREATE TABLE #PERSON
(
  IT_IS_GUID UNIQUEIDENTIFIER,
  IT_IS_DATE_TIME DATETIME,
  IT_IS_DATE_TIME_OFFSET DATETIMEOFFSET,
  IT_IS_DATE_TIME_SPAN TIME,
  IT_IS_BOOL BIT,
  IT_IS_BYTE BINARY,
  IT_IS_SHORT SMALLINT,
  IT_IS_INT INTEGER,
  IT_IS_LONG BIGINT,
  IT_IS_DECIMAL DECIMAL(15,2),
  IT_IS_DOUBLE FLOAT,
  IT_IS_SINGLE REAL,
  IT_IS_INT_16 SMALLINT,
  IT_IS_INT_32 INTEGER,
  IT_IS_INT_64 BIGINT
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
                personSqlBulkCopy.DestinationTableName = "#PERSON";
                await personSqlBulkCopy.WriteToServerAsync(PersonDataTable);
            }
        }

        public void AddPerson(Person person)
        {
            PersonDataTable.Rows.Add(
                person.ItIsGuid,
                person.ItIsDateTime,
                person.ItIsDateTimeOffset,
                person.ItIsDateTimeSpan,
                person.ItIsBool,
                person.ItIsByte,
                person.ItIsShort,
                person.ItIsInt,
                person.ItIsLong,
                person.ItIsDecimal,
                person.ItIsDouble,
                person.ItIsSingle,
                person.ItIsInt16,
                person.ItIsInt32,
                person.ItIsInt64
            );
        }
    }
}
