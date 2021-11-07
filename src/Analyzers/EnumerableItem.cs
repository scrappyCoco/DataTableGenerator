namespace Coding4fun.DataTools.Analyzers
{
    public class EnumerableItem<T>
    {
        public T Value { get; }
        public int Position { get; }
        public int Length { get; }

        public bool IsLast => Position + 1 == Length;

        public EnumerableItem(T value, int position, int length)
        {
            Value = value;
            Position = position;
            Length = length;
        }
    }
}