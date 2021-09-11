using System.Diagnostics.CodeAnalysis;
using Coding4fun.DataTableGenerator.Common;
using Coding4fun.DataTableGenerator.Entity;

namespace Coding4fun.DataTableGenerator.Example
{
    public partial class PersonSqlMapping
    {
        [SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod")]
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
                ).AddBasicSubTable<string>("#SKILL", "SKILL", "VARCHAR(100)", p => p.Skills);
        }
    }
}