var builder = WebApplication.CreateBuilder(args);

// Add controller services
builder.Services.AddControllers();

var app = builder.Build();

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();