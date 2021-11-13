using System;
using System.Linq.Expressions;

namespace Example
{
    public class SqlMappingDeclarationAttribute : System.Attribute
    {
    }
    
    public class TableBuilder<TItem>
    {
        public TableBuilder<TItem> AddColumn(
            Expression<Func<TItem, object>> valueGetter,
            string? sqlType = null,
            string? columnName = null) => this;
    }

    public partial class PersonSqlMapping
    {
        [SqlMappingDeclaration]  
        private void Initialize()
        {
            new TableBuilder<Person>().AddColumn(person =>
        }
    }
}