using System;

class Controlflow()
{
    public static void Run()
    {
        int x = -5;

        System.Console.WriteLine("===========================");
        if (x > 0) Console.WriteLine("Positive");
        else if (x < 0) Console.WriteLine("Negative");
        else Console.WriteLine("Zero");

        var result = x switch
        {
            0 => "zero",
            > 0 => "positive",
            < 0 => "negative"
        };
        Console.WriteLine(result);

    }
}