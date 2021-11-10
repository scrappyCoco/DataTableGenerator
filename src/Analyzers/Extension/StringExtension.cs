using System;

namespace Coding4fun.DataTools.Analyzers.Extension
{
    public static class StringExtension
    {
        public static TEnum ParseEnum<TEnum>(this string? value) => (TEnum)Enum.Parse(typeof(TEnum), value ?? throw new ArgumentNullException(nameof(value)));
        public static bool EqualsIgnoreCase(this string? first, string? second)
        {
            if (first == null && second == null) return true;
            if (first == null || second == null) return false;
            return first.Equals(second, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}