namespace Coding4fun.DataTableGenerator.SourceGenerator.Extension
{
    public static class ObjectExtension
    {
        public static TTarget Cast<TTarget>(this object it)
        {
            return (TTarget)it;
        }
    }
}