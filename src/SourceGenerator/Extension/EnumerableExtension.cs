using System.Collections;
using System.Collections.Generic;

namespace Coding4fun.DataTableGenerator.SourceGenerator.Extension
{
    public static class EnumerableExtension
    {
        public static IEnumerable<TTarget> IsInstanceOf<TTarget>(this IEnumerable enumerable)
        {
            foreach (object item in enumerable)
                if (item is TTarget target)
                    yield return target;
        }
    }
}