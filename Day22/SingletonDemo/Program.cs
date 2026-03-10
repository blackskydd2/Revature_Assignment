
// null coalesing demo
// string name = "Zia";

// // null coalescing operator
// var defaultName = name ?? "Sarah";

// var isTrue = 1 == 1;

// Console.WriteLine($"name: {defaultName}");

// return;




// var logger = new SingletonLogger();

using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddSingleton<DiSignleton>();

var serviceProvider = services.BuildServiceProvider();

var diLogger1 = serviceProvider.GetService<DiSignleton>();
var diLogger2 = serviceProvider.GetService<DiSignleton>();

var loggerManual = new DiSignleton();

Console.WriteLine(diLogger1.GetHashCode());
Console.WriteLine(diLogger2.GetHashCode());
Console.WriteLine(loggerManual.GetHashCode());

public class DiSignleton
{
    public int Value { get; set; }
}

// var logger1 = SingletonLogger.CreateStaticLogger();
// var logger2 = SingletonLogger.CreateStaticLogger();
// var logger3 = SingletonLogger.CreateStaticLogger();


// Console.WriteLine(logger1.GetHashCode());
// Console.WriteLine(logger2.GetHashCode());
// Console.WriteLine(logger3.GetHashCode());

// public class SingletonLogger
// {
//     private static SingletonLogger? singletonInstance;
//     private SingletonLogger() { }

//     // Factory
//     public static SingletonLogger CreateStaticLogger()
//     {
//         // singletonInstance ??= new SingletonLogger();

//         lock (typeof(SingletonLogger))
//         {
//             // Critical Session
//             if (singletonInstance == null)
//             {
//                 singletonInstance = new SingletonLogger();
//             }
//         }

//         return singletonInstance;
//     }
// }