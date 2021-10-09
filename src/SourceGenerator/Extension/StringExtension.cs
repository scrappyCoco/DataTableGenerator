using System;

namespace Coding4fun.DataTableGenerator.SourceGenerator.Extension
{
    public static class StringExtension
    {
        public static TEnum ParseEnum<TEnum>(this string? value) => (TEnum)Enum.Parse(typeof(TEnum), value);

    }
}