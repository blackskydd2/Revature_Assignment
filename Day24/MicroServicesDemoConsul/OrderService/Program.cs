using Consul;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IConsulClient, ConsulClient>(p =>
    new ConsulClient(cfg => cfg.Address = new Uri("http://localhost:8500")));

var app = builder.Build();

// Register with Consul
var consulClient = app.Services.GetRequiredService<IConsulClient>();
var registration = new AgentServiceRegistration()
{
    ID = $"order-service-{Guid.NewGuid()}",
    Name = "order-service",
    Address = "localhost",
    Port = 7000
};
await consulClient.Agent.ServiceRegister(registration);

app.MapControllers();
app.Run("http://localhost:7000");