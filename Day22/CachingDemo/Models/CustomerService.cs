using System.ComponentModel.DataAnnotations;
using AutoMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;

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

    public required string Name { get; set; }

    public required string Email { get; set; }

    public DateTime CreatedAt { get; set; }
}
public interface ICustomerService
{
    IEnumerable<Customer> GetAllCustomers();
}
public class CustomerService : ICustomerService
{
    private readonly CrmDbContext dbContext;

    private readonly IMemoryCache _cache;


    public CustomerService(CrmDbContext dbContext, IMemoryCache cache)
    {
        this.dbContext = dbContext;
        _cache = cache;
    }

    public IEnumerable<Customer> GetAllCustomers()
    {
        return dbContext.Customers.ToList();
    }
}

public class CustomerDTO
{
    public required string FullName { get; set; } 
}

// Model for Crate Customer
public class CreateCustomerDTO
{
    // [Required]
    // [StringLength(100)]
    // [MinLength(2)]
    // [MaxLength(100)]
    // [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name can only contain letters and spaces.")]
    public required string Name { get; set; }
    public required string Email { get; set; }

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