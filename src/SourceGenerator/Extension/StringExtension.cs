namespace Coding4fun.DataTableGenerator.SourceGenerator.Extension
{
    public static class StringExtension
    {
        public static string AddSpacesAfterLn(this string str, int count) =>
            str.Replace("\n", '\n' + new string(' ', count));
    }
}