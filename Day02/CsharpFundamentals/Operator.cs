using System;

class Operators()
{
    public static void Show()
    {
        int marks = 75;
        
        System.Console.WriteLine("===========================");
        Console.WriteLine(marks == 75);   
        Console.WriteLine(marks != 60);   
        Console.WriteLine(marks > 50);    
        Console.WriteLine(marks < 40);    
        Console.WriteLine(marks >= 75);   
        Console.WriteLine(marks <= 100);  

        
        Console.WriteLine(marks >= 40 && marks <= 100); 
        Console.WriteLine(marks < 40 || marks > 100);           
       
    }
}