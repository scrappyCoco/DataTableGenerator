class TableBuilder<TItem> { }
class Person { }

class Program
{
    static void Main()
    {
        var tableBuilder = [|new TableBuilder<Person>()|];
    }
}