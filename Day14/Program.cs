using EFLinq;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

using var context = new AppDbContext();

// var employees = context.Employees.ToList();

// foreach (var employee in employees)
// {
//     System.Console.WriteLine($" empname : {employee.EName}");
    
// }


// Employees With Salary > 2000
// var empsalgreaterthan200 = context.Employees
//     .Where(e=> e.Sal > 2000)
//     .ToList();

// foreach(var salary in empsalgreaterthan200)
// {
//     System.Console.WriteLine($"Salary greater than 200  is {salary.Sal} of emp : {salary.EName}");
// }

//Join Department:
// var joinDept = context.Employees
//     .Include(e=> e.Department)
//     .ToList();

// foreach(var jo in joinDept)
// {
//     System.Console.WriteLine($" emp with department : {jo.Department.DName}");
// }

//group by department : 
var grpDept = context.Employees
    .GroupBy (e => e.DeptNo)
    .Select(g => new
    {
        Deptno = g.Key,
        Count = g.Count(),
        AvgSal = g.Average(e => e.Sal)
    })
    .ToList();

foreach(var item  in grpDept)
{
    System.Console.WriteLine($" Dept {item.Deptno} - employee : {item.Count} , Avg Sal : {item.AvgSal}");
}