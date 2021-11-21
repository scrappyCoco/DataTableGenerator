namespace Example
{
    public class SqlMappingDeclarationAttribute : System.Attribute
    {
    }

    public partial class PersonSqlMapping
    {
        /*[__ERROR__*/[SqlMappingDeclaration]  
        private void Initialize()
        {
            
        }/*__ERROR__]*/
    }

    class Program
    {
        static void Main()
        {
        }
    }
}