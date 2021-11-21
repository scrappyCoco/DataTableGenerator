﻿using System;
using System.Linq.Expressions;

public class SqlMappingDeclarationAttribute : System.Attribute
{
}
    
public class TableBuilder<TItem>
{
    public TableBuilder<TItem> AddColumn(Expression<Func<TItem, object>> valueGetter) => this;
}

public class Person
{
    public string FirstName { get; set; }
}

public partial class PersonSqlMapping
{
    [SqlMappingDeclaration]  
    private void Initialize()
    {
        new TableBuilder<Person>().AddColumn((Person person) => /*[__ERROR__*/"There must be member access expression: person.FirstName"/*__ERROR__]*/);
    }
}

class Program
{
    static void Main()
    {
    }
}