using System;

namespace Coding4fun.DataTableGenerator.Example
{
    internal static class Program
    {
        private static void Main()
        {
            var personSqlMapping = new PersonSqlMapping();
            Console.WriteLine(personSqlMapping.GetSqlTableDefinition());
        }
    }
}