using System;


public static class DataTypes
{
        public static void Show()
        {
           int a = 42;
        long big = 3_000_000_000L;
        float f = 3.14f;
        double d = 2.71828;
        decimal money = 19.99m;
        bool ok = true;
        char letter = 'A';

        System.Console.WriteLine("===========================");

        System.Console.WriteLine($"int = {a}");
        System.Console.WriteLine($"long = {big}");
        System.Console.WriteLine($"Float = {f}");
        System.Console.WriteLine($"Double = {d}");
        System.Console.WriteLine($"Decimal = {money}");
        System.Console.WriteLine($"Bool = {ok}");
        System.Console.WriteLine($"char = {letter}");

    }
}

        
