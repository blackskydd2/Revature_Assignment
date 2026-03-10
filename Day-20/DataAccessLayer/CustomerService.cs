using System.ComponentModel.DataAnnotations;
using AutoMapper;

using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer;

public class CrmDbContext : DbContext
{
    public CrmDbContext(DbContextOptions<CrmDbContext> options) : base(options)
    {
    }
    public DbSet<Customer> Customers { get; set; }
}
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}
public interface ICustomerService
{
    IEnumerable<Customer> GetAllCustomers();
}
public class CustomerService : ICustomerService
{
    private readonly CrmDbContext dbContext;

    public CustomerService(CrmDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public IEnumerable<Customer> GetAllCustomers()
    {
        // SELECT * FROM cUSTOMERS;
        return dbContext.Customers.ToList();
    }

    public Customer GetCustomersById(int id)
    {
        // Assuming there is a City property in the Customer entity
        // SLECT * FROM Customer WHERE id = id;
        return dbContext.Customers.Where(c => c.Id == id).FirstOrDefault();
    }
}

public class CustomerDTO
{
    public string FullName { get; set; }
}

// Model for Crate Customer
public class CreateCustomerDTO
{
    // [Required]
    // [StringLength(100)]
    [MinLength(2)]
    // [MaxLength(100)]
    // [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name can only contain letters and spaces.")]
    public string Name { get; set; }
    public string Email { get; set; }

    public int Age { get; set; }
}

public class CustomerProfile : Profile
{
    public CustomerProfile()
    {
        CreateMap<Customer, CustomerDTO>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Name));
    }
}