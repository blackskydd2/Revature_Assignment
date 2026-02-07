using System;

public static class VarAndTypeInference
{
    public static void Run()
    {
        //var uses compile-time type inference, where the compiler determines the variable’s type from the assigned value, while keeping strong typing.
        System.Console.WriteLine("===========================");
        var x = 10; 
        var list = new List<string> { "A", "B" };
        Console.WriteLine($"x={x}, list count={list.Count}");
    }
}
