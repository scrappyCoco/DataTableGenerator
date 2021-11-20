#nullable disable

using Coding4fun.DataTools.Analyzers;
using Coding4fun.DataTools.Analyzers.StringUtil;
using System;
using System.ComponentModel.DataAnnotations;

namespace Coding4fun.DataTools.Test.TestData.SourceGenerator
{
    public class Person
    {
        public Guid ItIsGuid { get; set; }
        public DateTime ItIsDateTime { get; set; }
        public DateTimeOffset ItIsDateTimeOffset { get; set; }
        public TimeSpan ItIsDateTimeSpan { get; set; }
        public bool ItIsBool { get; set; }
        public byte ItIsByte { get; set; }
        public short ItIsShort { get; set; }
        public int ItIsInt { get; set; }
        public long ItIsLong { get; set; }
        public decimal ItIsDecimal { get; set; }
        public double ItIsDouble { get; set; }
        public Single ItIsSingle { get; set; }
        public Int16 ItIsInt16 { get; set; }
        public Int32 ItIsInt32 { get; set; }
        public Int64 ItIsInt64 { get; set; }
    }

    public partial class PersonSqlMapping
    {
        [SqlMappingDeclaration]  
        private void Initialize()
        {
            new TableBuilder<Person>()
                .AddColumn((Person person) => person.ItIsGuid)
                .AddColumn((Person person) => person.ItIsDateTime)
                .AddColumn((Person person) => person.ItIsDateTimeOffset)
                .AddColumn((Person person) => person.ItIsDateTimeSpan)
                .AddColumn((Person person) => person.ItIsBool)
                .AddColumn((Person person) => person.ItIsByte)
                .AddColumn((Person person) => person.ItIsShort)
                .AddColumn((Person person) => person.ItIsInt)
                .AddColumn((Person person) => person.ItIsLong)
                .AddColumn((Person person) => person.ItIsDecimal)
                .AddColumn((Person person) => person.ItIsDouble)
                .AddColumn((Person person) => person.ItIsSingle)
                .AddColumn((Person person) => person.ItIsInt16)
                .AddColumn((Person person) => person.ItIsInt32)
                .AddColumn((Person person) => person.ItIsInt64);
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