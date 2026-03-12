using System.Net;
using System.Text.Json;

namespace InvoiceManagement.API.Middleware
{
    /// <summary>
    /// Catches unhandled exceptions and returns a clean JSON error response
    /// instead of exposing stack traces to callers.
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var (statusCode, title) = ex switch
            {
                ArgumentException      => (HttpStatusCode.BadRequest,          "Validation Error"),
                KeyNotFoundException   => (HttpStatusCode.NotFound,            "Not Found"),
                InvalidOperationException => (HttpStatusCode.UnprocessableEntity, "Business Rule Violation"),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized,   "Unauthorized"),
                _                      => (HttpStatusCode.InternalServerError, "Internal Server Error")
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode  = (int)statusCode;

            var body = JsonSerializer.Serialize(new
            {
                status  = (int)statusCode,
                title,
                message = ex.Message,
                path    = context.Request.Path.Value,
                timestamp = DateTime.UtcNow
            });

            return context.Response.WriteAsync(body);
        }
    }

    // Extension method for clean registration
    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
            => app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
