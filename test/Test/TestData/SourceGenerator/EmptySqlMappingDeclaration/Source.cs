namespace Example
{
    public class SqlMappingDeclarationAttribute : System.Attribute
    {
    }
    
    public class TableBuilder<T>
    {
    
    }

    public partial class PersonSqlMapping
    {
        [SqlMappingDeclaration]  
        private void Initialize()
        {
            
        }
    }
}