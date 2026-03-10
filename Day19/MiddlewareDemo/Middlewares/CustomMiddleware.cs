
public class CustomMiddleware
{
    private readonly RequestDelegate _next;

    public CustomMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Custom logic before the next middleware
        // Custome Exception Handling
        Console.WriteLine("Custom Middleware: Before next middleware");
        try
        {
            await _next(context);
        }
        catch
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"message\": \"An error occurred in application. We apologize awe are looking into it.\"}");
        }

        // Custom logic after the next middleware
        Console.WriteLine("Custom Middleware: After next middleware");
    }
}