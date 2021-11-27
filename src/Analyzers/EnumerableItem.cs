namespace Coding4fun.DataTools.Analyzers
{
    public class EnumerableItem
    {
        public object? Value { get; }
        public int Position { get; }
        public int Length { get; }

        public bool IsLast => Position + 1 == Length;

        public EnumerableItem(object? value, int position, int length)
        {
            Value = value;
            Position = position;
            Length = length;
        }
    }
}