using System;
using MongoDB.Driver;

public class Program
{
    public static void Main(string[] args)
    {
        var client = new MongoClient("mongodb://localhost:27017");
        var databse = client.GetDatabase("studentDB");
        var collection = databse.GetCollection<Student>("students");

        System.Console.WriteLine("Connected to MongoDb");

        //CRUD Opertaions:
        var student = new Student { Name = "Aayush", Age = 22, Course = "Computer Engineering" };
        collection.InsertOne(student);
        Console.WriteLine("Student inserted!");

        var students = collection.Find(s => true).ToList();
        foreach (var s in students)
        {
            Console.WriteLine($"{s.Name} - {s.Course}");
        }



        var filter = Builders<Student>.Filter.Eq("name", "Aayush");
        var update = Builders<Student>.Update.Set("course", "Backend Development");
        collection.UpdateOne(filter, update);
        Console.WriteLine("Student updated!");
        student = collection.Find(s => s.Name == "Aayush").FirstOrDefault();
        Console.Write(student?.Name);
        Console.WriteLine(student?.Course);


        var deleteFilter = Builders<Student>.Filter.Eq("name", "Aayush");
        collection.DeleteOne(deleteFilter);
        Console.WriteLine("Student deleted!");

    }
}