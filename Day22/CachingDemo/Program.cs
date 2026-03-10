
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// add appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddControllers();

// register DbContext and CustomerService
builder.Services.AddScoped<CrmDbContext>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

// AutoMapper
builder.Services.AddAutoMapper(config =>
{
    config.AddProfile<CustomerProfile>();
});

// Fluent Validation
builder.Services.AddScoped<IValidator<CreateCustomerDTO>, CreateCustomerDTOValidator>();
builder.Services.AddScoped<IValidator<CreateCustomerDTO>, UKNameCreateCustomerDTOValidator>();

// Add Sql Server
// builder.Services.AddDbContext<CrmDbContext>(options =>
//     options.UseSqlServer(builder.Configuration.GetConnectionString("CrmDbConnection")));
builder.Services.AddDbContextPool<CrmDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CrmDbConnection"))
    , poolSize: 128);


builder.Services.AddMemoryCache();

builder.Services.AddScoped<ICustomerService,CustomerService>();


builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = "CustomerCache";
});

var app = builder.Build();

app.UseRouting();

app.MapControllers();

app.Run();
