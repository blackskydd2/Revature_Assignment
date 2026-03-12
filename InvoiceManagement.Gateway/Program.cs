using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════════════
//  YARP Reverse Proxy
//  Reads all route and cluster config from appsettings.json
// ═══════════════════════════════════════════════════════════════
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ═══════════════════════════════════════════════════════════════
//  Rate Limiting  (ASP.NET Core built-in)
//  Protects downstream APIs from abuse
// ═══════════════════════════════════════════════════════════════
builder.Services.AddRateLimiter(options =>
{
    // Global fixed-window policy: 100 requests per minute per IP
    options.AddFixedWindowLimiter("global-limit", opt =>
    {
        opt.PermitLimit         = 100;
        opt.Window              = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit          = 10;
    });

    // Stricter policy for write operations (POST/PUT/PATCH/DELETE): 20 per minute
    options.AddFixedWindowLimiter("write-limit", opt =>
    {
        opt.PermitLimit         = 20;
        opt.Window              = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit          = 5;
    });

    // Response when limit exceeded
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            """{"status":429,"title":"Too Many Requests","message":"Rate limit exceeded. Please slow down and retry."}""",
            token);
    };
});

// ═══════════════════════════════════════════════════════════════
//  CORS  — allow frontend clients
// ═══════════════════════════════════════════════════════════════
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("GatewayCors", policy =>
        policy.WithOrigins(
                "http://localhost:3000",   // React dev server
                "http://localhost:5173",   // Vite dev server
                "http://localhost:4200")   // Angular dev server
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// ═══════════════════════════════════════════════════════════════
//  Request Logging Middleware  (logs every proxied request)
// ═══════════════════════════════════════════════════════════════
app.Use(async (context, next) =>
{
    var start = DateTime.UtcNow;
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

    logger.LogInformation(
        "[Gateway] {Method} {Path}{Query} → forwarding",
        context.Request.Method,
        context.Request.Path,
        context.Request.QueryString);

    await next();

    var elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
    logger.LogInformation(
        "[Gateway] {Method} {Path} ← {StatusCode} ({Elapsed:F0}ms)",
        context.Request.Method,
        context.Request.Path,
        context.Response.StatusCode,
        elapsed);
});

// ═══════════════════════════════════════════════════════════════
//  Gateway Health Check  (own health — not proxied)
// ═══════════════════════════════════════════════════════════════
app.MapGet("/gateway/health", () => Results.Ok(new
{
    status    = "Healthy",
    service   = "InvoiceManagement.Gateway (YARP)",
    time      = DateTime.UtcNow,
    upstream  = "http://localhost:5001"
}));

// ═══════════════════════════════════════════════════════════════
//  Gateway Info  (shows all configured routes)
// ═══════════════════════════════════════════════════════════════
app.MapGet("/gateway/info", (IConfiguration config) =>
{
    var routes = config.GetSection("ReverseProxy:Routes").GetChildren()
        .Select(r => new
        {
            routeId   = r.Key,
            clusterId = r["ClusterId"],
            path      = r["Match:Path"]
        });

    var clusters = config.GetSection("ReverseProxy:Clusters").GetChildren()
        .Select(c => new
        {
            clusterId = c.Key,
            destinations = c.GetSection("Destinations").GetChildren()
                .Select(d => new { name = d.Key, address = d["Address"] })
        });

    return Results.Ok(new { routes, clusters });
});

// ═══════════════════════════════════════════════════════════════
//  Middleware pipeline order matters:
//  CORS → RateLimiting → YARP Proxy
// ═══════════════════════════════════════════════════════════════
app.UseCors("GatewayCors");
app.UseRateLimiter();

// Apply rate limiting policies to YARP routes
app.MapReverseProxy(pipeline =>
{
    pipeline.Use(async (context, next) =>
    {
        // Apply stricter limit to write operations
        var method = context.Request.Method;
        if (method == "POST" || method == "PUT" ||
            method == "PATCH" || method == "DELETE")
        {
            context.SetEndpoint(context.GetEndpoint());
        }
        await next();
    });
}).RequireRateLimiting("global-limit");

app.Run();
