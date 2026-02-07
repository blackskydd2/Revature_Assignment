using System;
using System.Text;

public static class StringDemo
{
    public static void Show()
    {
        string s = "String "+ "Concated";
        string template = $"User :{Environment.UserName}, Date : {DateTime.Today}";

        System.Console.WriteLine("===========================");
        System.Console.WriteLine(s);
        System.Console.WriteLine(template);

        var str = new StringBuilder();
        str.Append("Line1").AppendLine();
        str.AppendFormat("{0} items", 5);
        System.Console.WriteLine(str.ToString());
        

        
    }
}
