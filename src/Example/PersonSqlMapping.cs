using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Coding4fun.DataTableGenerator.Common;
using JetBrains.Annotations;

namespace Coding4fun.DataTableGenerator.Example
{
    public partial class PersonSqlMapping
    {
        [SqlMappingDeclaration]
        //[SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod")]
        [UsedImplicitly]
        private void Initialize()
        {
            // Notes:
            // 1. Keep same names for parameter in lambda expressions for the same type.
            //    For our example it's `person`.
            //    If you write for the first column: person => person.Id.
            //    At the second column it should be same: person => person.Age.
            //    It will be not valid for second and others columns: p => p.Age.
            // 2. Write always generic name. For C# in some scenarios generic name is redundant,
            //    but there it's required.
            //    Example: .AddSubTable<Job>(person => person.Jobs ...
            //                         ^   ^
            
            new TableBuilder<Person>()
                // It's possible to add some action, that will be added before
                // adding to DataTable.
                // Possible to create multi statement expression.
                // This method call is optional.
                .AddPreExecutionAction(person =>
                {
                    Console.WriteLine(person.LastName + " " + person.FirstName);
                })
                // Optional parameter - set up SQL table name.
                // If this method was not defined, then it will get table name from generic.
                // In our example it will be #PERSON.
                .SetName("#MY_PERSON")
                // person.Id has Guid type and column name will be ID in table #PERSON.
                // Type will be UNIQUEIDENTIFIER.
                .AddColumn(person => person.Id)
                .AddColumn(person => person.Age)
                // For string values it's desirable to set sql type,
                // because by default it will be NVARCHAR(MAX).
                .AddColumn(person => person.FirstName, "VARCHAR(50)")
                .AddColumn(person => person.LastName, "VARCHAR(50)")
                // person.CountryCode has 2 two attributes: [MinLength(2)], [MaxLength(2)].
                // In this case SQL type will be NCHAR(2).
                // If you want to set CHAR(2), you should add second argument explicitly.
                .AddColumn(person => person.CountryCode, columnName: "COUNTRY")
                // Logo is array of byte and it will have VARBINARY(MAX).
                .AddColumn(person => person.Logo)
                // Adding detail table #JOB for master table #PERSON.
                .AddSubTable(person => person.Jobs, jobBuilder => jobBuilder
                    // You should add relation identifiers between person and job yourself.
                    .AddPreExecutionAction((job, person) => job.PersonId = person.Id)
                    .AddColumn(job => job.PersonId)
                    .AddColumn(job => job.CompanyName, "VARCHAR(100)")
                    .AddColumn(job => job.Address, "VARCHAR(200)")
                ).AddSubTable<Skill>(person => person.SkillValues, skillBuilder => skillBuilder
                    .AddColumn(skill => skill.PersonId)
                    .AddColumn(skill => skill.Tag, "VARCHAR(100)")
                );
        }
    }
}