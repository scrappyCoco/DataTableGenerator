﻿#nullable disable

using Coding4fun.DataTools.Analyzers;
using Coding4fun.DataTools.Analyzers.StringUtil;
using System;
using System.ComponentModel.DataAnnotations;

namespace Coding4fun.DataTools.Test.TestData.SourceGenerator
{
    public class Person
    {
        public string FirstName { get; set; }
        public string GetFirstName() => FirstName;
    }

    public partial class PersonSqlMapping
    {
        [SqlMappingDeclaration]  
        private void Initialize()
        {
            new TableBuilder<Person>(NamingConvention.SnakeCase)
                .AddColumn((Person person) => person.GetFirstName());
        }
    }

    static class Program
    {
        static void Main()
        {
            new PersonSqlMapping();
        }
    }
}