using Coding4fun.DataTools.Analyzers.Extension;
using Message = System.Collections.Generic.KeyValuePair<string, string>;

namespace Coding4fun.DataTools.Analyzers
{
    public static class Messages
    {
        private static readonly string TableBuilderName = typeof(TableBuilder<int>).GetNameWithoutGeneric();

        public static Message GetUnableToGetTableDefinition() =>
            new("C4FDT0002", $"Unable to find definition of {TableBuilderName}<TItem>.");
        
        public static Message GetInvalidType() =>
            new("C4FDT0007", "Only array of byte is available in AddColumn method.");
        
        public static Message GetSqlMappingIsEmpty() =>
            new("C4FDT0014", "Sql mapping is empty.");
        
        public static Message GetCyclicDependenciesAreNotSupported(string typeFullName) =>
            new("C4FDT0016", $"Cyclic dependencies are not supported: {typeFullName}.");
        
        public static Message GetLambdaWithoutType() =>
            new("C4FDT0017", "Lambda expression without explicit type is not supported. Example: AddColumn((Person person) => person.Age). ");
        
        public static Message Get19() => new("C4FDT0019", "Access to property is required in lambda expression body. Example: AddColumn((Person person) => person.Age)");
        public static Message Get20() => new("C4FDT0020", "Access to property is required in lambda expression body. Example: AddSubTable((Person person) => person.Jobs, ...)");
    }
}