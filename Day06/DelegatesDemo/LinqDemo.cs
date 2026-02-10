// using System;

// namespace DelegatesDemo;

// class Student
// {
//     public string Name { get; set; }
//     public int Age { get; set; }
//     public string Grade { get; set; }
// }
// public class LinqDemo
// {
//     public void Run()
//     {
//         // var alice = new Student { Name = "Alice", Age = 20, Grade = "A" };

//         // var x = new { Name = alice.Name, Status = false };

//         // Console.WriteLine($"Hello {alice.Name} : Type: {alice.GetType().Name}!");
//         // Console.WriteLine($"Hello {x.Name} Staus: {x.Status} : Type: {x.GetType().Name}!");

//         var students = new List<Student>
//         {
//             new Student { Name = "Alice", Age = 20, Grade = "A" },
//             new Student { Name = "Bob", Age = 22, Grade = "B" },
//             new Student { Name = "Charlie", Age = 21, Grade = "A" },
//             new Student { Name = "David", Age = 23, Grade = "C" },
//             new Student { Name = "Eve", Age = 20, Grade = "B" }
//         };

//         // var olderStudents = from s in students
//         //                     where s.Age > 21
//         //                     select new { s.Name };

//         // var olderStudents = students.Where(filter);
//         // foreach (var s in students) {}
//         var olderStudents =
//             students
//                 .Where(s => s.Age > 21)
//                 .Select(s => new { s.Name });

//         foreach (var student in olderStudents)
//         {
//             Console.WriteLine($"{student.Name} more than 21 years old.");
//         }

//         var x = students
//                 .Where(s => s.Age > 21)
//                 .Select(s => new { s.Name })
//                 .FirstOrDefault();

//         Console.WriteLine($"{x.Name} is the first result.");

//         var orderedByAge = students
//                 .OrderByDescending(s => s.Age);

//         foreach (var student in orderedByAge)
//         {
//             Console.WriteLine($"{student.Name} is {student.Age}.");
//         }

//         var anyOlderThan25 = students
//             .Any(s => s.Age > 25);

//         Console.WriteLine($"Anybody older than 25: {anyOlderThan25}.");
//     }

//     // bool filter(Student s)
//     // {
//     //     return s.Age > 21;
//     // }
// }