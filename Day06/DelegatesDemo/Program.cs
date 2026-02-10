using System;

namespace DelegatesDemo;

class Program
{
    static void Main(string[] args)
    {
        DelegateDemoApp app = new DelegateDemoApp();
        // app.Run();
        // app.LambdAExpressionDemo();
        // app.AnonymousDemo();
        // app.HigherOrderFunction();

        
        // =================Event Handler ============
        // Button but = new Button();

        // but.OnClick += () => System.Console.WriteLine("Bill rings (subscriber 1)");
        // but.OnClick += () => System.Console.WriteLine("Door need to be open (subscriber 2)");
        // but.OnClick += () => System.Console.WriteLine("Order Cousin to do it (subscriber 3)");
        // but.OnClick += () => System.Console.WriteLine("Opened th door (subscriber 4)");

        // but.Click();

        
        // ================== Link ===================
        // LinqDemo linq = new LinqDemo();

        // linq.Run();

        // ======================= Assignmet ==========
         Assignment assignment = new Assignment();
        assignment.Run();

    }
}

// Events =========================================
// publisher : =================
public class Button
{
    public delegate void OnClickPublisher();

    public event OnClickPublisher OnClick;

    public void Click()
    {
        OnClick?.Invoke();
    }
}


//  ===============================================
delegate int MathOperation(int a, int b);

delegate TResult GenericTwoParameter<TFirst, TSecond, TResult>(TFirst a, TSecond b);
class DelegateDemoApp
{


    public void HigherOrderFunction()
    {
        var res = CalculateArea(AreaOfRectangle);

        System.Console.WriteLine($"Area is {res}");
        
    }

    int CalculateArea(Func<int, int, int> areaFunction)
    {
        return areaFunction(5,10);
    }

    int AreaOfRectangle(int length, int breadth)
    {
        return length*breadth;
    }

    int AreaOfTriangle(int baseLength, int height)
    {
        return (baseLength*height) / 2;
    }


    // =========================================================
    public void LambdAExpressionDemo()
    {
        Func<int, int> f;

        f = (int x) => x * x;

        var result = f(5);

        System.Console.WriteLine($"The square is {result}");
    }


    public void AnonymousDemo()
    {
        MathOperation operation = delegate (int a, int b)
        {
            System.Console.WriteLine($"ANNONYMOUS METHOD ADDING {a} AND {b} give {a + b}");
            return a + b;
        };
        operation(5, 2);

    }

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
        Console.WriteLine($"Final Result : {res}");

        // ---------- Func<int,int,int> ----------
        Func<int, int, int> genericOperation = Add;
        Console.WriteLine(genericOperation(10, 5));

        // ---------- String delegate ----------
        Action<string> stringAction = PrintMessage;
        stringAction("This is from Action<string> method");


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
