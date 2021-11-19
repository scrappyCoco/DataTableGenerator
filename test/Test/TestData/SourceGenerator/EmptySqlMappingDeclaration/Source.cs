namespace Example
{
    public class SqlMappingDeclarationAttribute : System.Attribute
    {
    }

    public partial class PersonSqlMapping
    {
        [SqlMappingDeclaration]  
        private void Initialize()
        {
            
        }
    }

    class Program
    {
        static void Main()
        {
        }
    }
}