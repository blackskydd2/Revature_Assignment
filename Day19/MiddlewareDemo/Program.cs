var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();


app.Use(async (context, next) =>
{
    Console.WriteLine($"Before the next middleware {context.Request.Path}");
    // if(context.Request.Path.StartsWithSegments("/api/v1/customer"))
    // {
    //     Console.WriteLine("This is a customer request");
    //     // Write response stream
    //     context.Response.StatusCode = 403;
    //     context.Response.ContentType = "application/json";
    //     await context.Response.WriteAsync("{\"message\": \"Authronization\"}");
    // }
    // else
    await next.Invoke();
    Console.WriteLine("After the next middleware");
});

app.UseMiddleware<CustomMiddleware>();

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("AllowAll");

app.MapControllers();

app.Run();
