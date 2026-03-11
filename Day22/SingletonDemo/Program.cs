// Manual Singleton (Static Factory Pattern)

// using System;
// using Microsoft.Extensions.DependencyInjection;

// var l1 = LoggerSingleton.Create();
// var l2 = LoggerSingleton.Create();
// var l3 = LoggerSingleton.Create();

// System.Console.WriteLine(l1.GetHashCode());
// System.Console.WriteLine(l2.GetHashCode());
// System.Console.WriteLine(l3.GetHashCode());

// public class LoggerSingleton
// {
//     private static LoggerSingleton ? _instance;

//     private static readonly object _lock = new();

//     private LoggerSingleton() {}

//     public static LoggerSingleton Create()
//     {
//         if (_instance == null)
//         {
//             lock(_lock)
//             {
//                 if (_instance == null)
//                 {
//                     _instance = new LoggerSingleton();
//                 }
//             }
//         }
//         return _instance;

//     }
// }

// Singleton via Dependency Injection

using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Register as Singleton in DI
services.AddSingleton<AppSingleton>();

var provider = services.BuildServiceProvider();

var s1 = provider.GetService<AppSingleton>();
var s2 = provider.GetService<AppSingleton>();

System.Console.WriteLine("Through DI");
System.Console.WriteLine(s1.GetHashCode());
System.Console.WriteLine(s2.GetHashCode());
public class AppSingleton
{
    public int Id { get; set; }
}