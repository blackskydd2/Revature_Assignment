using InvoiceManagement.API.Middleware;
using InvoiceManagement.BLL;
using InvoiceManagement.DAL.Context;

var builder = WebApplication.CreateBuilder(args);

// ── MongoDB Settings ─────────────────────────────────────────────
var mongoSettings = builder.Configuration.GetSection("MongoDB").Get<MongoDbSettings>()
    ?? new MongoDbSettings();

// ── Register all Invoice Management services (DAL + BLL) ────────
builder.Services.AddInvoiceManagement(mongoSettings);

// ── ASP.NET Core ─────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title       = "Invoice Management API",
        Version     = "v1",
        Description = "REST API for the Invoice Management System (MongoDB backend)"
    });
});

// ── CORS — allow Gateway and any local dev client ─────────────────
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowGateway", policy =>
        policy.WithOrigins("http://localhost:5000", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Invoice Management API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseGlobalExceptionHandler();
app.UseCors("AllowGateway");
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// ── Health check endpoint ─────────────────────────────────────────
app.MapGet("/health", () => Results.Ok(new
{
    status  = "Healthy",
    service = "InvoiceManagement.API",
    time    = DateTime.UtcNow
}));

app.Run();
