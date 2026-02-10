using System;
using System.Collections;
using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;

namespace Day05
{
    class Program
    {
        static void Main(string[] args)
        {
            // var res1 = new Resource("Res1");
            // var res2 = new Resource("Res2");

            // res1 = null;   // Eligible for GC
            // res2 = res2;   // Still referenced

            // GC.Collect();
            // GC.WaitForPendingFinalizers();

            // Console.WriteLine("GC Completed");


            // Demo(); //use to generic type

            // ListDemo();

            // DictionaryDemo();

            DictionaryGeneric();








        }

        // private static void DisposableDemo()
        // {
        //     using (var manager = new FileManager("DisposableRes")) ;
        //     manager.OpenFile("path/to/file.txt"); // Use resource 

        // } //  res.Dispose() called automatically here 
        //  using var manager2 = new FileManager("DisposableRes"); // Dispose called at end of scope   

        private static void RecordDemo()
        {
            var Temp1 = new Temp { Id = 1, Name = "Temp" };
            var Temp2 = new Temp { Id = 1, Name = "Temp" };

            System.Console.WriteLine(Temp1);
            System.Console.WriteLine(Temp2);
            System.Console.WriteLine(Temp1 == Temp2);

            var Temp3 = Temp1 with { Id = 2 };
            System.Console.WriteLine(Temp3);
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

        static void Demo()
        {
            int x = 3, y = 4;
            Swap(ref x, ref y);
            System.Console.WriteLine($" x = {x}, y =  {y}");

            string a = "Hello", b = "World";
            Swap(ref a, ref b);
            System.Console.WriteLine($" a = {a}, b =  {b}");


        }


        // public static void ListDemo()
        // {
        //     // List<string> list = new List<string>();
        //     // list.Add("1");
        //     // list.Add("2");
        //     // list.Add("3");
        //     // list.Add("4");

        //     // foreach(var i in list)
        //     // {
        //     //     System.Console.WriteLine($"{i} and its type {i.GetType()}");
        //     // }

        //     List<int> marks = new List<int>(5);

        //     marks.Add(1);
        //     marks.Add(4);
        //     System.Console.WriteLine($"Count{marks.Count} , Capacity {marks.Capacity}");

        //     marks.Add(5);
        //     System.Console.WriteLine($"Count{marks.Count} , Capacity {marks.Capacity}");

        //     marks.AddRange(new int[] {10,20,30});
        //     System.Console.WriteLine();
        //     System.Console.WriteLine($"Count{marks.Count} , Capacity {marks.Capacity}");

        //     marks.AddRange(new int[] {40,50,60});
        //     System.Console.WriteLine();
        //     System.Console.WriteLine($"Count{marks.Count} , Capacity {marks.Capacity}");
        // }

        // public static void ArrayList()
        // {
        //     ArrayList list = new ArrayList();

        //     list.Add(10);
        //     list.Add(20);
        //     list.Add(30);
        //     list.Add(3.14);
        //     list.Add("Hello");

        //     foreach (var item in list)
        //     {
        //         Console.WriteLine(item);
        //     }

        //     int sum = 0;

        //     foreach (var item in list)
        //     {
        //         Console.WriteLine($"Item: {item}, type: {item.GetType()}");

        //         if (item is int)   // required to avoid runtime crash
        //         {
        //             sum += (int)item;   // unboxing
        //         }
        //     }

        //     Console.WriteLine($"Sum: {sum}");

        // }


        // public static void DictionaryDemo()
        // {
        //     Dictionary<int, char> dict = new Dictionary<int, char>();

        //     dict.Add(101, 'K');
        //     dict.Add(102, 'L');
        //     dict.Add(103, 'M');

        //     foreach (var i in dict)
        //     {
        //         Console.WriteLine($"Roll no is {i.Key} and grade {i.Value}");
        //     }
        // }


        public static void DictionaryGeneric()
        {
            Dictionary<String, List<int>> stud = new Dictionary<string, List<int>>();
            stud.Add("MP", new List<int> { 45, 48, 43 });
            stud.Add("KT", new List<int> { 40, 41, 38 });

            foreach (var s in stud)
            {
                Console.WriteLine($"Name: {s.Key}, Average: {s.Value.Average()}");
            }

            if (stud.ContainsKey("KT"))
            {
                System.Console.WriteLine("KT gave the eaxm and is present ..");
            }


        }

    }
}

