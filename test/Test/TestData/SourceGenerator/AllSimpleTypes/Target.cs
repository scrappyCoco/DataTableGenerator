
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
            PersonDataTable.Columns.Add("it_is_guid", typeof(System.Guid));
            PersonDataTable.Columns.Add("it_is_date_time", typeof(System.DateTime));
            PersonDataTable.Columns.Add("it_is_date_time_offset", typeof(System.DateTimeOffset));
            PersonDataTable.Columns.Add("it_is_date_time_span", typeof(System.TimeSpan));
            PersonDataTable.Columns.Add("it_is_bool", typeof(bool));
            PersonDataTable.Columns.Add("it_is_byte", typeof(byte));
            PersonDataTable.Columns.Add("it_is_short", typeof(short));
            PersonDataTable.Columns.Add("it_is_int", typeof(int));
            PersonDataTable.Columns.Add("it_is_long", typeof(long));
            PersonDataTable.Columns.Add("it_is_decimal", typeof(decimal));
            PersonDataTable.Columns.Add("it_is_double", typeof(double));
            PersonDataTable.Columns.Add("it_is_single", typeof(float));
            PersonDataTable.Columns.Add("it_is_int_16", typeof(short));
            PersonDataTable.Columns.Add("it_is_int_32", typeof(int));
            PersonDataTable.Columns.Add("it_is_int_64", typeof(long));
        }
  
        public string GetSqlTableDefinition() => @"
CREATE TABLE #person
(
  it_is_guid UNIQUEIDENTIFIER,
  it_is_date_time DATETIME,
  it_is_date_time_offset DATETIMEOFFSET,
  it_is_date_time_span TIME,
  it_is_bool BIT,
  it_is_byte BINARY,
  it_is_short SMALLINT,
  it_is_int INTEGER,
  it_is_long BIGINT,
  it_is_decimal DECIMAL(15,2),
  it_is_double FLOAT,
  it_is_single REAL,
  it_is_int_16 SMALLINT,
  it_is_int_32 INTEGER,
  it_is_int_64 BIGINT
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
