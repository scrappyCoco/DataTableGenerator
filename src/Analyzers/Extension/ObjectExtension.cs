namespace Coding4fun.DataTools.Analyzers.Extension
{
    public static class ObjectExtension
    {
        public static TTarget Cast<TTarget>(this object it)
        {
            return (TTarget)it;
        }
    }
}