using System;

namespace Coding4fun.DataTableGenerator.Common
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SqlMappingDeclarationAttribute: Attribute
    {
        public static readonly string Name = nameof(SqlMappingDeclarationAttribute).Replace(nameof(Attribute), "");
    }
}