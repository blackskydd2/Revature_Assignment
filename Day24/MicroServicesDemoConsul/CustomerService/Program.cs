using Consul;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSingleton<IConsulClient, ConsulClient>(p =>
    new ConsulClient(cfg => cfg.Address = new Uri("http://localhost:8500")));

var app = builder.Build();

// Register with Consul
var consulClient = app.Services.GetRequiredService<IConsulClient>();
var registration = new AgentServiceRegistration()
{
    ID = $"customer-service-{Guid.NewGuid()}",
    Name = "customer-service",
    Address = "localhost",
    Port = 5000
};
await consulClient.Agent.ServiceRegister(registration);

app.MapControllers();
app.Run("http://localhost:5000");