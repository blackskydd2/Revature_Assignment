using DataAccessLayer;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

public static class DalServices
{
    public static void AddDataAccessLayer(this IServiceCollection services)
    {
        // register DbContext and CustomerService
        services.AddScoped<CrmDbContext>();
        services.AddScoped<ICustomerService, CustomerService>();

        // Fluent Validation
        services.AddScoped<IValidator<CreateCustomerDTO>, CreateCustomerDTOValidator>();
        services.AddScoped<IValidator<CreateCustomerDTO>, UKNameCreateCustomerDTOValidator>();
    }
}