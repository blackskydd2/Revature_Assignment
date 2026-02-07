using System;


public static class NullableDemo
{
    public static void Run()
    {   
         int? n = null;
        int value = n ?? -1;
        string? s = null;
        int? length = s?.Length;
        System.Console.WriteLine("===========================");
        Console.WriteLine($"value={value}, length={length}");

    }
}
