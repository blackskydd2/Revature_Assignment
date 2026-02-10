namespace DelegatesDemo;

class Program
{
    static void Main(string[] args)
    {
        DelegateDemoApp app = new DelegateDemoApp();
        app.Run();
    }
}

delegate int MathOperation(int a, int b);

delegate TResult genericTwoParameter<TFirst, TSecond, TResult>  
class DelegateDemoApp
{
    void PrintMessage(string msg)
    {
        Console.WriteLine(msg);
    }

    public void Run()
    {
        // ---------- Multicast delegate ----------
        MathOperation operation = Add;
        operation += Subtract;
        operation += Multiply;
        operation += Divide;
        operation -= Add;

        int res = operation(5, 3);
        Console.WriteLine($"Final Result (last method): {res}");

        // ---------- Func<int,int,int> ----------
        Func<int, int, int> genericOperation = Add;
        Console.WriteLine(genericOperation(10, 5));

        // ---------- String delegate ----------
        Action<string> stringAction = PrintMessage;
        stringAction("This is from Action<string>");

        // ---------- Custom generic delegate ----------
        
    }

    public int Add(int a, int b)
    {
        Console.WriteLine($"Sum: {a + b}");
        return a + b;
    }

    public int Subtract(int a, int b)
    {
        Console.WriteLine($"Difference: {a - b}");
        return a - b;
    }

    public int Multiply(int a, int b)
    {
        Console.WriteLine($"Product: {a * b}");
        return a * b;
    }

    public int Divide(int a, int b)
    {
        Console.WriteLine($"Quotient: {a / b}");
        return a / b;
    }
}
