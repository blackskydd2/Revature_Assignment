using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;

class Program
{
    public static void Main(string[] args)
    {
        // //String : 
        // string s = "Hello" + "World"; //HelloWorld
        // string template = $"User: {Environment.UserName}, Date: {DateTime.Today:d}"; //User: Kads, Date: 14-02-2026


        // StringBuilder sb = new StringBuilder();
        // sb.Append("Line1 ").AppendLine();
        // sb.AppendFormat("{0} items ", 5);
        // string result = sb.ToString();
        // /*Line1 
        // 5 items
        // */


        // //Var :
        // var price = 499.9;
        // var quantity = 3;
        // var total = price * quantity; //1499.6999999999998


        // var Student = new List<string>()
        // {
        //     "Aman",
        //     "Priya",
        //     "Karan"
        // };

        // foreach (var i in Student)
        // {
        //     System.Console.WriteLine(i);
        //     /*Aman
        //     Priya
        //     Karan
        //     */
        // }


        // //Nullable : 1. Value type, 2. Reference Type

        // // int number = null;
        // // byte bt = null;
        // // short st = null;
        // // long lg = null;
        // // float ft = null;
        // // double db = null;

        // // because they are non-nullable value type

        // // int? number = null;
        // // byte? bt = null;
        // // short? st = null;
        // // long? lg = null;
        // // float? ft = null;
        // // double? db = null;


        // Nullable<int> num = null;
        // // if(num is null)
        // // {
        // //     System.Console.WriteLine("null");
        // // }
        // // else
        // // {
        // //     System.Console.WriteLine(num);
        // // }

        // // System.Console.WriteLine(num ?? -1); //-1


        // //Patteren Matching = Pattern matching was introduced to unify those steps. 
        // // Instead of checking the type and then casting separately, you describe a “pattern” that the object must match.
        // //  If it matches, C# automatically gives you access to the data inside it. So conceptually, pattern matching is about describing 
        // // the structure you expect, and letting the language verify and unpack it for you.

        // // Shape s1 = new Circle { Radius = 5 };
        // // System.Console.WriteLine($"Area cirlce : {GetArea(s1)}");

        // // Shape s2 = new Rectangle { Width = 10, Height = 5 };
        // // System.Console.WriteLine($"Area rectangle : {GetArea(s2)}");

        // // Shape s3 = new Triangle{Base = 14, Height = 10};
        // // System.Console.WriteLine($"Area of Triangle is : {GetArea(s3)}");

        // =====================================================================================================================

        // //if-else
        // int x = 42; //pos
 
        // if(x > 0)
        // {
        //     System.Console.WriteLine("Positive");
        // }
        // else if(x < 0)
        // {
        //     System.Console.WriteLine("negative");
        // }
        // else
        // {
        //     System.Console.WriteLine("Zero");
        // }


        //SWITCH

        // int x = 0; //zero
        // var result = x switch 
        // {
        //     0 => "Zero",
        //     > 0 => "Positive",
        //     < 0 => "Negative",
            
        // };



        Object?[] values = { null, 42, "hello", 3.14, true };

        foreach (var val in values)
        {
            string desc = Describe(val);       // Line to debug
            Console.WriteLine($"Value: {val ?? "null"} => {desc}");
        }
        static string Describe(object? obj) => obj switch
        {
        null => "none",               // Line 1: if obj is null, return "none"
        int i => $"int {i}",          // Line 2: if obj is int, bind to i and return "int {i}"
        string s => $"str({s})",      // Line 3: if obj is string, bind to s and return "str({s})"
        _ => "other"                  // Line 4: default for anything else
        };



        
       








    }

    public static double GetArea(Shape shape) =>
 shape switch
 {
     Circle { Radius: var r } => Math.PI * r * r,
     Rectangle { Width: var w, Height: var h } => w * h,
     Triangle { Base: var b, Height: var h } => 0.5 * b * h
 };


    //if-else 
}

abstract class Shape { }
class Circle : Shape
{
    public double Radius { get; set; }
}

class Rectangle : Shape
{
    public double Width { get; set; }
    public double Height { get; set; }
}

class Triangle : Shape
{
    public double Base { get; set; }

    public double Height { get; set; }
}
