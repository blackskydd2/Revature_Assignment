using InvoiceManagement.BLL.Engines;
using InvoiceManagement.BLL.Services.Implementations;
using InvoiceManagement.BLL.Services.Interfaces;
using InvoiceManagement.BLL.Validators;
using InvoiceManagement.DAL.Context;
using InvoiceManagement.DAL.Repositories.Implementations;
using InvoiceManagement.DAL.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceManagement.BLL
{
    /// <summary>
    /// Extension method to register all Invoice Management services in a DI container.
    /// Call services.AddInvoiceManagement(settings) from any host.
    /// </summary>
    public static class ServiceRegistration
    {
        public static IServiceCollection AddInvoiceManagement(
            this IServiceCollection services,
            MongoDbSettings mongoSettings)
        {
            // MongoDB context (singleton — MongoClient is thread-safe)
            services.AddSingleton(mongoSettings);
            services.AddSingleton<MongoDbContext>();

            // Repositories (scoped)
            services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
            services.AddScoped<IAuditRepository, AuditRepository>();
            services.AddScoped<IReconciliationRepository, ReconciliationRepository>();
            services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();

            // Engines (singleton — stateless)
            services.AddSingleton<ICalculationEngine, CalculationEngine>();
            services.AddSingleton<IInvoiceStateMachine, InvoiceStateMachine>();
            services.AddSingleton<IAgingEngine, AgingEngine>();
            services.AddSingleton<IDsoEngine, DsoEngine>();

            // Validators (singleton — stateless)
            services.AddSingleton<IInvoiceValidator, InvoiceValidator>();

            // BLL Services (scoped)
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IReportingService, ReportingService>();

            return services;
        }
    }
}
