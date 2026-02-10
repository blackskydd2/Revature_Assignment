public class TempClass
{
    public int Id { get; init; }
    public string Name { get; init; }

    public TempClass(int id, string name)
    {
        Id = id;
        Name = name;
    }
}


    public record Temp
{
    public int Id { get; init; }
    public string Name { get; init; }
}

public struct TempStruct
{
    public int Id { get; init; }
    public string Name { get; init; }
}