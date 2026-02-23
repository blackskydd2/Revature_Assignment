using System;
using Microsoft.EntityFrameworkCore;


class Program
{
    static void Main(string[] args)
    {

        using (var db = new AppDbContext())
        {
            // Apply any pending migrations (FORWARD)
            Console.WriteLine("Applying migrations (if any)...");
            db.Database.Migrate();
            Console.WriteLine("Migrations applied.");

            // Optional sanity check: add and read a record
            if (!db.Customers.Any())
            {
                db.Customers.Add(new Customer { Name = "First User", Email = "first@example.com" });
                db.SaveChanges();
                Console.WriteLine("Inserted a sample customer.");
            }

            Console.WriteLine("Current customers:");
            foreach (var c in db.Customers.AsNoTracking().ToList())
            {
                Console.WriteLine($"  {c.Id}: {c.Name} - {c.Email}");
            }
        }

        Console.WriteLine("Done. You can now run EF CLI commands to go up/down.");
    }
}