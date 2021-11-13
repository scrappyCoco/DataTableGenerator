namespace Example
{
    public class SqlMappingDeclarationAttribute : System.Attribute
    {
    }
    
    public class TableBuilder<T>
    {
    }

    public class Person
    {
    }

    public partial class PersonSqlMapping
    {
        [SqlMappingDeclaration]  
        private void Initialize()
        {
            new TableBuilder<Person>();
        }
    }
}