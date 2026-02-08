using System;

namespace Day3MethodsDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Day 3: Methods & Members (Non-Async) ===");

            System.Console.WriteLine("Method Declaration ======================");
            Calculator calc = new Calculator();
            Console.WriteLine($"Add: {calc.Add(2, 3)}");
            Console.WriteLine($"Add (expression-bodied): {calc.AddExpr(4, 5)}");

            System.Console.WriteLine("Parameters & Return Types (Tuple)===============");
            var result = calc.Summarize(new int[] { 1, 2, 3, 4 });
            Console.WriteLine($"Sum = {result.sum}, Count = {result.count}");

            System.Console.WriteLine("Method Overloading=================");
            Logger.Log("Simple log message");
            Logger.Log("Formatted {0} + {1} = {2}", 2, 3, 5);
            Logger.Log(new Exception("Invalid input"), "Error");

            System.Console.WriteLine("Named & Optional Parameters==========");
            Configure();
            Configure(verbose: true);
            Configure(5, verbose: true);

            System.Console.WriteLine("out parameter ======================");
            if (TryGetMax(new int[] { 10, 5, 8 }, out int max))
                Console.WriteLine($"Max value: {max}");

            System.Console.WriteLine("ref parameter ====================");
            int a = 1, b = 2;
            Swap(ref a, ref b);
            Console.WriteLine($"After swap: a={a}, b={b}");

            System.Console.WriteLine("params keyword ==============================");
            Console.WriteLine($"Sum(params): {Sum(1, 2, 3, 4, 5)}");

            System.Console.WriteLine("Delegate Invocation ==============================");
            Func<int, int, int> multiply = (x, y) => x * y;
            Console.WriteLine($"Delegate result: {multiply(3, 4)}");

            System.Console.WriteLine("Extension Method =======================");
            Car car = new Car { Brand = "Toyota" };
            car.Drive();
        }

        static void Configure(int retries = 3, bool verbose = false) //named and local parameter
        {
            Console.WriteLine($"Configure -> retries={retries}, verbose={verbose}");
        }

        static bool TryGetMax(int[] numbers, out int max) //out parameter
        {
            max = int.MinValue;
            if (numbers.Length == 0) return false;

            foreach (int n in numbers)
                if (n > max) max = n;

            return true;
        }

        static void Swap(ref int x, ref int y) //ref parameter
        {
            int temp = x;
            x = y;
            y = temp;
        }

        static int Sum(params int[] values) //params keyword
        {
            int total = 0;
            foreach (int v in values)
                total += v;
            return total;
        }
    }

    public class Calculator
    {
        public int Add(int x, int y)
        {
            return x + y;
        }

        public int AddExpr(int x, int y) => x + y;

        public (int sum, int count) Summarize(int[] items)
        {
            int sum = 0;
            foreach (int i in items)
                sum += i;

            return (sum, items.Length);
        }
    }

    public static class Logger //method overloading
    {
        public static void Log(string message)
        {
            Console.WriteLine(message);
        }

        public static void Log(string format, params object[] args)
        {
            Console.WriteLine(string.Format(format, args));
        }

        public static void Log(Exception ex, string message)
        {
            Console.WriteLine($"{message}: {ex.Message}");
        }
    }

    public static class StringExtensions //extension methods
    {
        public static bool IsPalindrome(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return false;

            char[] arr = s.ToCharArray();
            Array.Reverse(arr);
            string reversed = new string(arr);

            return s.Equals(reversed, StringComparison.OrdinalIgnoreCase);
        }
    }

    class Car
    {
        public string Brand { get; set; }
    }

    static class CarExtension
    {
        public static void Drive(this Car car)
        {
            System.Console.WriteLine($"{car.Brand} chal rahi hai");
        }
    }
}
