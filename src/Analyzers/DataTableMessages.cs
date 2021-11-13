using Coding4fun.DataTools.Analyzers.Extension;
using Message = System.Collections.Generic.KeyValuePair<string, string>;

namespace Coding4fun.DataTools.Analyzers
{
    public static class DataTableMessages
    {
        private static readonly string TableBuilderName = typeof(TableBuilder<int>).GetNameWithoutGeneric();

        public static Message GetUnableToGetTableDefinition() =>
            new("C4FDT0002", $"Unable to find definition of {TableBuilderName}<TItem>.");
        
        public static Message GetUnableToGetTableTypeInfo() =>
            new("C4FDT0003", "Unable to get type info.");
        
        public static Message GetUnableToFindNamespace() =>
            new("C4FDT0004", "Unable to find namespace.");
        
        public static Message GetUnableToGetExpressionBody() =>
            new("C4FDT0005", "Unable to get expression body.");

        public static Message GetUnableToParseInvocationExpression() =>
            new("C4FDT0006", "Unable to parse invocation expression.");
        
        public static Message GetInvalidType() =>
            new("C4FDT0007", "Invalid type.");
        
        public static Message GetUnableToParseIntValueFromAttribute() =>
            new("C4FDT0008", "Unable to parse int value from attribute.");

        public static Message GetUnableToGetBodyOfLambdaExpression() =>
            new("C4FDT0009", "Unable to get body of lambda expression.");
        
        public static Message GetUnableToGetTypeOfEnumerable() =>
            new("C4FDT0010", "Unable to get type of enumerable.");
        
        public static Message GetUnableToGetGenericName() =>
            new("C4FDT0011", "Unable to get generic name.");
        
        public static Message GetUnableToGetExpression() =>
            new("C4FDT0012", "Unable to get expression.");
        
        public static Message GetUnableToGetGenericTypeOfSubTable() =>
            new("C4FDT0013", "Unable to get generic type of sub table.");
        
        public static Message GetSqlMappingIsEmpty() =>
            new("C4FDT0014", "Sql mapping is empty.");
        
        public static Message GetUnableToResolveProperty(string propertyName, string typeName) =>
            new("C4FDT0015", $"Unable to resolve {propertyName} in {typeName}.");
    }
}