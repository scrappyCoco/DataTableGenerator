using System;
using JetBrains.Annotations;

namespace Coding4fun.DataTools.Analyzers
{
    [PublicAPI]
    public class SqlMappingDeclarationAttribute: Attribute
    {
        public static readonly string Name = nameof(SqlMappingDeclarationAttribute).Replace(nameof(Attribute), "");
    }
}