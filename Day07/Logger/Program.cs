// add service provider
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

var services = new ServiceCollection();

services.AddLogging(config =>
{
    config.AddSerilog(new LoggerConfiguration()
        .WriteTo.Console()
        .MinimumLevel.Debug()
        .CreateLogger(), dispose: true);

    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Debug);
});

services.AddScoped<UserService>();

var serviceProvider = services.BuildServiceProvider();


var userService = serviceProvider.GetService<UserService>();

userService?.CreateUser("John");

public class UserService
{
    private readonly ILogger<UserService> _logger;

    public UserService(ILogger<UserService> logger)
    {
        _logger = logger;
    }

    public void CreateUser(string name)
    {
        // Log: "Creating user: John"
        _logger.LogInformation("Creating user: {Name}", name);

        try
        {
            // Create user...
            _logger.LogInformation("User {Name} created successfully", name);
        }
        catch (Exception ex)
        {
            // Log error with exception
            _logger.LogError(ex, "Failed to create user: {Name}", name);
        }
    }
}