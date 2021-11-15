using System.Linq;

namespace MyNamespace
{
    class Person
    {
        public string Name { get; set; }
    }
    
    class Program
    {
        static void Main()
        {
            string name = new Person().Name;
            string[] names = new Person[] { new Person() }.Select(p => p.Name).ToArray();
        }
    }
}