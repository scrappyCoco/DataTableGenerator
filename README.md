# Coding4fun.DataTools



## DataTable source generator

Someone who have deal with C# DataTable knows, that it has many lines of code, located in different places. It has some problems.

For example:
```c#
// 1. We must to declare tables. It's not difficult.
DataTable personDataTable = new DataTable();
DataTable jobDataTable = new DataTable();
DataTable skillDataTable = new DataTable();

// 2. We must to declare columns. Why we must to write types expclicitly? It could be got from Person type`
personDataTable.Columns.Add("ID", typeof(Guid));
personDataTable.Columns.Add("AGE", typeof(short));
personDataTable.Columns.Add("FIRST_NAME", typeof(string));
personDataTable.Columns.Add("LAST_NAME", typeof(string));
personDataTable.Columns.Add("COUNTRY", typeof(string));
personDataTable.Columns.Add("LOGO", typeof(byte[]));

jobDataTable.Columns.Add("PERSON_ID", typeof(Guid));
jobDataTable.Columns.Add("COMPANY_NAME", typeof(string));
jobDataTable.Columns.Add("ADDRESS", typeof(string));

skillDataTable.Columns.Add("PERSON_ID", typeof(Guid));
skillDataTable.Columns.Add("TAG", typeof(string));

// 3. We must to fill DataTables.
// Same columns are involved.
// In cases, where columns count is big it's difficult to map columns to it's values,
// especially if column names are not equals to property names.
IEnumerable<Person> persons = new Person[] { };// There some data source.
foreach (var person in persons)
{
    personDataTable.Rows.Add(
        person.Id,
        person.Age,
        person.FirstName,
        person.LastName,
        person.CountryCode,
        person.Logo
    );
    
    foreach (var job in person.Jobs)
    {
        job.PersonId = person.Id;
        jobDataTable.Rows.Add(
            job.PersonId,
            job.CompanyName,
            job.Address
        );
    }
    
    foreach (var skill in person.SkillValues)
    {
        skillDataTable.Rows.Add(
            skill.PersonId,
            skill.Tag
        );
    }
}

// 4. If we want write to temp tables, we must to define it.
// Another one time we specify same columns.
const string tempTableDefinition = @"
CREATE TABLE #MY_PERSON
(
  ID UNIQUEIDENTIFIER,
  AGE SMALLINT,
  FIRST_NAME VARCHAR(50),
  LAST_NAME VARCHAR(50),
  COUNTRY NVARCHAR(MAX),
  LOGO VARBINARY(MAX)
);
CREATE TABLE #JOB
(
  PERSON_ID UNIQUEIDENTIFIER,
  COMPANY_NAME VARCHAR(100),
  ADDRESS VARCHAR(200)
);
CREATE TABLE #SKILL
(
  PERSON_ID UNIQUEIDENTIFIER,
  TAG VARCHAR(100)
);";

await using SqlConnection targetConnection = new SqlConnection("server=localhost;Integrated Security=True;");
await using SqlCommand createTempTablesCommand = new SqlCommand(tempTableDefinition, targetConnection);
await targetConnection.OpenAsync();

// 5. We need to copy each DataTable.
using (SqlBulkCopy personSqlBulkCopy = new SqlBulkCopy(targetConnection))
{
    personSqlBulkCopy.DestinationTableName = "#MY_PERSON";
    await personSqlBulkCopy.WriteToServerAsync(personDataTable);
}
using (SqlBulkCopy jobSqlBulkCopy = new SqlBulkCopy(targetConnection))
{
    jobSqlBulkCopy.DestinationTableName = "#JOB";
    await jobSqlBulkCopy.WriteToServerAsync(jobDataTable);
}
using (SqlBulkCopy skillSqlBulkCopy = new SqlBulkCopy(targetConnection))
{
    skillSqlBulkCopy.DestinationTableName = "#SKILL";
    await skillSqlBulkCopy.WriteToServerAsync(skillDataTable);
}

// TODO: do something with that tables.

await targetConnection.CloseAsync();
```

Keep in mind, that this model is basic and only for example. In real world we have deal with more complex objects.

As you can notice, there a lot boilerplate code. I think, that there could be presented model, that will generate all this code:
```c#
// Create partial class with mapping.
// Source generator will produce read-only class PersonSqlMapping.Generated.cs
public partial class PersonSqlMapping
{
    // We must define attribute on method, that will contain TableBuilder.
    [SqlMappingDeclaration]
    private void Initialize()
    {
        // Notes:
        //    Keep same names for parameter in lambda expressions for the same type.
        //    For our example it's `person`.
        //    If you write for the first column: person => person.Id.
        //    At the second column it should be same: person => person.Age.
        //    It will be not valid for second and others columns: p => p.Age.
        
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
```

This code will produce read-only c# code:
```c#
public partial class PersonSqlMapping
{
    public DataTable PersonDataTable { get; } = new DataTable();
    public DataTable JobDataTable { get; } = new DataTable();
    public DataTable SkillDataTable { get; } = new DataTable();

    public PersonSqlMapping()
    {
        PersonDataTable.Columns.Add("ID", typeof(System.Guid));
        PersonDataTable.Columns.Add("AGE", typeof(short));
        PersonDataTable.Columns.Add("FIRST_NAME", typeof(string));
        PersonDataTable.Columns.Add("LAST_NAME", typeof(string));
        PersonDataTable.Columns.Add("COUNTRY", typeof(string));
        PersonDataTable.Columns.Add("LOGO", typeof(byte[]));

        JobDataTable.Columns.Add("PERSON_ID", typeof(System.Guid));
        JobDataTable.Columns.Add("COMPANY_NAME", typeof(string));
        JobDataTable.Columns.Add("ADDRESS", typeof(string));

        SkillDataTable.Columns.Add("PERSON_ID", typeof(System.Guid));
        SkillDataTable.Columns.Add("TAG", typeof(string));
    }

    public string GetSqlTableDefinition() => @"
CREATE TABLE #MY_PERSON
(
  ID UNIQUEIDENTIFIER,
  AGE SMALLINT,
  FIRST_NAME VARCHAR(50),
  LAST_NAME VARCHAR(50),
  COUNTRY NVARCHAR(MAX),
  LOGO VARBINARY(MAX)
);
CREATE TABLE #JOB
(
  PERSON_ID UNIQUEIDENTIFIER,
  COMPANY_NAME VARCHAR(100),
  ADDRESS VARCHAR(200)
);
CREATE TABLE #SKILL
(
  PERSON_ID UNIQUEIDENTIFIER,
  TAG VARCHAR(100)
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
            jobSqlBulkCopy.DestinationTableName = "#JOB";
            await jobSqlBulkCopy.WriteToServerAsync(JobDataTable);
        }
        using (SqlBulkCopy skillSqlBulkCopy = new SqlBulkCopy(targetConnection))
        {
            skillSqlBulkCopy.DestinationTableName = "#SKILL";
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
            person.Logo
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
```

## TableBuilder analyzer and code fix

Describing a model by hand is tedious, and there comes to the rescue analyzer, that will produce this mapping. At first, we need to create partial class with constructor invocation:
```c#
public partial class PersonSqlMapping
{
    [SqlMappingDeclaration]
    private void Initialize()
    {
        new TableBuilder<Person>();
    }
}
```

Then you could see wavy line, move cursor over it, press the light bulb and press **Create SQL mapping for DataBuilder**. You will get sql mapping. Then you should customize:
1. Table names
2. Column names
3. Column types 
4. Relations between entities

![TableBuilderCodeFix](https://raw.githubusercontent.com/scrappyCoco/DataTools/master/screenshots/TableBuilderCodeFix.png)

Then we must to consume generated code:
```c#
Person[] persons = JsonSerializer.Deserialize<Person[]>(await File.ReadAllTextAsync("Persons.json"));

PersonSqlMapping personSqlMapping = new PersonSqlMapping();
personSqlMapping.FillDataTables(persons);

Console.WriteLine(personSqlMapping.GetSqlTableDefinition());

await using SqlConnection targetConnection = new SqlConnection("server=localhost;Integrated Security=True;");
await using SqlCommand createTempTablesCommand = new SqlCommand(personSqlMapping.GetSqlTableDefinition(), targetConnection);
await targetConnection.OpenAsync();
await createTempTablesCommand.ExecuteNonQueryAsync();
await personSqlMapping.BulkCopyAsync(targetConnection);

// TODO: do something with that tables.
await targetConnection.CloseAsync();
```

As result we have got very compat data model, where each column described in one place. It's easy to read and refactoring.