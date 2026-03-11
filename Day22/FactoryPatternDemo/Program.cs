// //Manual Design Pattern
// using System;
// using System.Runtime.CompilerServices;

// var circle = ShapeFactory.Create("Circle");
// var square = ShapeFactory.Create("Square");

// System.Console.WriteLine("//Manual Factor Pattern");
// circle.Draw();
// square.Draw();
// public static class ShapeFactory
// {
//     public static Ishape Create (string type)
//     {
//         return type switch
//         {
//             "Circle" => new Circle(),
//             "Square" => new Square(),
//              _ => throw new ArgumentException ("Invalid Shape INput")
//         };
//     }
// }
using System;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Register multiple Shapes
services.AddTransient<IShape, Circle>();
services.AddTransient<IShape, Square>();

var provider = services.BuildServiceProvider();


var shapes = provider.GetServices<IShape>();

Console.WriteLine("DI Factory Pattern:");
foreach (var shape in shapes)
{
    shape.Draw();
}

//+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
// Interface 
public interface IShape
{
    void Draw();
}

// Implementations:
public class Circle : IShape
{
    public void Draw() => Console.WriteLine("Drawing a Circle");
}

public class Square : IShape
{
    public void Draw() => Console.WriteLine("Drawing a Square");
}