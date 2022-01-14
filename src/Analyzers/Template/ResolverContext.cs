using System;
using System.Collections.Generic;
using System.Linq;
using Coding4fun.DataTools.Analyzers.Extension;

namespace Coding4fun.DataTools.Analyzers.Template
{
    public class ResolverContext
    {
        public object?[] SingleNullObjects { get; }
        private readonly object?[] _emptyObjects;
        private readonly object?[] _commaObjects;
        private readonly LinkedList<EnumerableItem> _objects;

        public ResolverContext()
        {
            _objects = new LinkedList<EnumerableItem>();
            SingleNullObjects = new object?[] { null };
            _commaObjects = new object?[] { "," };
            _emptyObjects = new object?[] { };
        }

        public IReadOnlyCollection<EnumerableItem> Objects => _objects;

        internal void Add(EnumerableItem item)
        {
            _objects.AddLast(item);
        }

        internal void RemoveLast() => _objects.RemoveLast();

        public object?[] GetValue<T>(Func<T, object?> valueGetter)
        {
            T requiredObject = Objects.Reverse()
                .Select(t => t.Value)
                .OfType<T>()
                .First()!;
                
            return requiredObject.Let(it => (valueGetter.Invoke(it) ?? it).ToArrayOfObject());
        }

        public object?[] GetBool<T>(Func<T, bool> predicate) => Objects.Last()
            .Value
            .Cast<T>()
            .Let(it => predicate.Invoke(it) ? SingleNullObjects : _emptyObjects);

        public object?[] GetArray<TSource, TTarget>(Func<TSource, IEnumerable<TTarget>> enumerableGetter) => Objects
            .Last()
            .Value
            .Cast<TSource>()
            .Let(it => enumerableGetter.Invoke(it).Cast<object?>().ToArray());

        public object?[] GetTableOffset()
        {
            int offsetLength = (Objects
                .Count(t => t.Value is TableDescription) + 2) * 4;
                 
            return new object?[] { new string(' ', offsetLength) };
        }

        public object?[] GetComma()
        {
            EnumerableItem lastItem = Objects.Last();
            return lastItem.IsLast ? SingleNullObjects : _commaObjects;
        }

        public object?[] GetLast() => Objects.Last().Value.ToArrayOfObject();
    }
}