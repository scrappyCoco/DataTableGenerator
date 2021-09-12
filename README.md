Column mapping for SQL:

```c#
using Coding4fun.DataTableGenerator.Common;

namespace MyExample
{
    public class Person
    {
        public short Age { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CountryCode { get; set; }
        public List<Job> Jobs { get; set; }
        public List<string> Skills { get; set; }
    }

    public class Job
    {
        public string CompanyName { get; set; }
        public string Address { get; set; }
    }


    public partial class PersonSqlMapping
    {
        static PersonSqlMapping()
        {
            new DataTableBuilder<Person>("#PERSON")
                .AddColumn("AGE", "SMALLINT", p => p.Age)
                .AddColumn("FIRST_NAME", "VARCHAR(50)", p => p.FirstName)
                .AddColumn("LAST_NAME", "VARCHAR(50)", p => p.LastName)
                .AddColumn("COUNTRY_CODE", "CHAR(2)", p => p.CountryCode)
                .AddSubTable<Job>("#JOB_HISTORY", p => p.Jobs, jobBuilder => jobBuilder
                    .AddColumn("COMPANY_NAME", "VARCHAR(100)", j => j.CompanyName)
                    .AddColumn("ADDRESS", "VARCHAR(200)", j => j.Address)
                ).AddBasicSubTable<string>("#SKILL", "VALUE", "VARCHAR(100)", p => p.Skills);
        }
    }
}
```

Generator will produce:
```c#
using Coding4fun.DataTableGenerator.Common;
using System.Collections.Generic;
using System.Data;

namespace MyExample

{
    public partial class PersonSqlMapping
    {
        public DataTable PERSON;
        public DataTable JOB_HISTORY;
        public DataTable SKILL;
        

        public PersonSqlMapping()
        {
            PERSON = new DataTable();
            PERSON.Columns.Add("AGE", typeof(short));
            PERSON.Columns.Add("FIRST_NAME", typeof(string));
            PERSON.Columns.Add("LAST_NAME", typeof(string));
            PERSON.Columns.Add("COUNTRY_CODE", typeof(string));
            
            JOB_HISTORY = new DataTable();
            JOB_HISTORY.Columns.Add("COMPANY_NAME", typeof(string));
            
            SKILL = new DataTable();
            SKILL.Columns.Add("VALUE", typeof(string));
            
        }

        public string GetSqlTableDefinition() => @"
CREATE TABLE #PERSON (
    AGE SMALLINT,
    FIRST_NAME VARCHAR(50),
    LAST_NAME VARCHAR(50),
    COUNTRY_CODE CHAR(2)
);
CREATE TABLE #JOB_HISTORY (
    COMPANY_NAME VARCHAR(100)
);
CREATE TABLE #SKILL (
    VALUE VARCHAR(100)
);
";

        public void FillDataTables(IEnumerable<Person> items)
        {
            PERSON.Clear();
            JOB_HISTORY.Clear();
            SKILL.Clear();
            

            foreach (var item in items)
            {
                AddPERSON(item);
            }
        }

        public void AddPERSON(Person p){
            PERSON.Rows.Add(
                p.Age,
                p.FirstName,
                p.LastName,
                p.CountryCode);
            foreach (var subItem in p.Jobs)
            {
                AddJOB_HISTORY(subItem);
            }
            foreach (var subItem in p.Skills)
            {
                AddSKILL(subItem);
            }
        }
        
        public void AddJOB_HISTORY(Job j){
            JOB_HISTORY.Rows.Add(
                j.CompanyName);
        }
        
        public void AddSKILL(string p){
            SKILL.Rows.Add(
                );
        }
        
    }
}
```

Also enough to use expression like in LINQ, but the performance of this method is a little bit slower.
```c#
        public SqlMappingWithExpression AddColumn(string columnName, string columnType, Expression<Func<CaseStat, object>> getCellExpression)
        {
            var func = getCellExpression.Compile();
            DataTable.Columns.Add(columnName, typeof(string));
            _cellGetters.Add(func);
            return this;
        }
```


|          Method |       Number of rows |       Mean |      Error |     StdDev | Rank |
|---------------- |--------------------- |-----------:|-----------:|-----------:|-----:|
|  Generated Code |                1 000 |   3.012 ms |  0.0195 ms |  0.0208 ms |    1 |
|      Expression |                1 000 |   4.392 ms |  0.0704 ms |  0.0811 ms |    2 |
|  Generated Code |               10 000 |  47.011 ms |  0.6708 ms |  0.7456 ms |    3 |
|      Expression |               10 000 |  61.714 ms |  0.8679 ms |  0.9995 ms |    4 |
|  Generated Code |              100 000 | 519.296 ms | 10.5864 ms | 11.7667 ms |    5 |
|      Expression |              100 000 | 629.166 ms |  2.6385 ms |  2.5914 ms |    6 |
