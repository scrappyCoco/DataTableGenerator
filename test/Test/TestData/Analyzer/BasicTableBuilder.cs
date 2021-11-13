namespace Example
{
    class TableBuilder<TItem> { }
    class Person { }

    class Program
    {
        static void Main()
        {
            [|new TableBuilder<Person>()|];
        }
    }
}