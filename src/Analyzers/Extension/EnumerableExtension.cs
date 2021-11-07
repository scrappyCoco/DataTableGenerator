using System.Collections.Generic;
using System.Linq;

namespace Coding4fun.DataTools.Analyzers.Extension
{
    public static class EnumerableExtension
    {
        public static EnumerableItem<T>[] ToArrayOfItem<T>(this IEnumerable<T> enumerable)
        {
            T[] items = enumerable.ToArray();
            return items
                .Select((value, position) => new EnumerableItem<T>(value, position, items.Length))
                .ToArray();
        }
    }
}