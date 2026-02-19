using System.Net.Mail;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=EFMigrationDemo;Trusted_Connection=True;TrustServerCertificate=True;");
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>().HasData(
            new Customer
            
            {
                Id = 1,
                Name = "Alice Johnson",
                Email = "alice@example.com",
                DateOfBirth = new DateTime(1990, 1, 15)
            },
            new Customer
            {
                Id = 2,
                Name = "Bob Singh",
                Email = "bob@example.com",
                DateOfBirth = null // nullable example
            }

        );
    }
}
