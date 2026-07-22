using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace library.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled Exception: {Message}", ex.Message);

                context.Response.ContentType = "application/json";

                var (statusCode, message) = ex switch
                {
                    UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, "Unauthorized access."),
                    KeyNotFoundException => ((int)HttpStatusCode.NotFound, "Resource not found."),
                    ArgumentException argEx => ((int)HttpStatusCode.BadRequest, argEx.Message),
                    _ => ((int)HttpStatusCode.InternalServerError, "There was an error in the system, please try again later.")
                };

                context.Response.StatusCode = statusCode;

                var response = new
                {
                    statusCode = statusCode,
                    message = message,
                    details = _env.IsDevelopment() ? ex.Message : null
                };

                var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
            }
        }
    }
}
