using System;
using System.Text.RegularExpressions;

namespace Coding4fun.DataTableGenerator.SourceGenerator.Extension
{
    public static class TypeExtension
    {
        private static readonly Regex GenericTrashRegex = new("`.+$");
        public static string GetNameWithoutGeneric(this Type type) => GenericTrashRegex.Replace(type.Name, "");
    }
}