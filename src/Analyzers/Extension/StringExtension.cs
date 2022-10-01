using System;
using System.Diagnostics.CodeAnalysis;

namespace Coding4fun.DataTools.Analyzers.Extension
{
    public static class StringExtension
    {
        [ExcludeFromCodeCoverage]
        public static bool EqualsIgnoreCase(this string? first, string? second)
        {
            if (first == null && second == null) return true;
            if (first == null || second == null) return false;
            return first.Equals(second, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}