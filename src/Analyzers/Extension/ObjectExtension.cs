using System;

namespace Coding4fun.DataTools.Analyzers.Extension
{
    public static class ObjectExtension
    {
        public static TTarget Cast<TTarget>(this object? it) => it == null
            ? throw new ArgumentNullException(nameof(it))
            : (TTarget)it;

        public static TTarget Let<TSource, TTarget>(this TSource it, Func<TSource, TTarget> converter) =>
            converter.Invoke(it);

        public static object?[] ToArrayOfObject<T>(this T source) => new[] { source.Cast<object?>() };
    }
}