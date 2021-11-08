using System;
using System.Data.SqlClient;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Coding4fun.DataTableGenerator.Example
{
    static class Program
    {
        private static async Task Main()
        {
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
        }
    }
}