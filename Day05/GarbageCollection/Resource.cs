public class Resource
{
    public string Name { get; set; }

    public Resource(string name)
    {
        Name = name;
        Console.WriteLine($"{Name} created");
    }

    ~Resource()  // Destructor Finalizer
    {
        Console.WriteLine($"{Name} destroyed by GC");
    }
}
