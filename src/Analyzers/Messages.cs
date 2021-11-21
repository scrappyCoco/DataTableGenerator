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
            new("C4FDT0007", "Invalid type.");
        
        public static Message GetSqlMappingIsEmpty() =>
            new("C4FDT0014", "Sql mapping is empty.");
        
        public static Message GetUnableToResolveProperty(string propertyName) =>
            new("C4FDT0015", $"Unable to resolve {propertyName}.");
        
        public static Message GetCyclicDependenciesAreNotSupported(string typeFullName) =>
            new("C4FDT0016", $"Cyclic dependencies are not supported: {typeFullName}.");
        
        public static Message GetLambdaWithoutType() =>
            new("C4FDT0017", "Lambda expression without explicit type is not supported.");
        
        public static Message GetMemberAccessExpression() => new("C4FDT0018", "Unable to get member access expression im lambda body.");
    }
}