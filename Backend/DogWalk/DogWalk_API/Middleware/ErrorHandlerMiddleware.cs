using System.Net;
using System.Text.Json;

namespace DogWalk_API.Middleware
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                var response = context.Response;
                response.ContentType = "application/json";

                response.StatusCode = error switch
                {
                    ApplicationException => (int)HttpStatusCode.BadRequest,
                    KeyNotFoundException => (int)HttpStatusCode.NotFound,
                    UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                    InvalidOperationException => (int)HttpStatusCode.BadRequest,
                    ArgumentException => (int)HttpStatusCode.BadRequest,
                    _ => (int)HttpStatusCode.InternalServerError,
                };

                _logger.LogError(error, "Error en la solicitud: {Message}", error.Message);

                object errorDetails = _env.IsDevelopment() ? new
                {
                    exception = error.GetType().Name,
                    message = error.Message,
                    stackTrace = error.StackTrace,
                    innerException = error.InnerException?.Message
                } : new { message = "Error interno del servidor" };

                var result = JsonSerializer.Serialize(new
                {
                    statusCode = response.StatusCode,
                    success = false,
                    message = GetErrorMessage(error),
                    data = errorDetails
                });

                await response.WriteAsync(result);
            }
        }

        private string GetErrorMessage(Exception error)
        {
            return error switch
            {
                ApplicationException => "Error de aplicación: " + error.Message,
                KeyNotFoundException => "Recurso no encontrado: " + error.Message,
                UnauthorizedAccessException => "Acceso no autorizado: " + error.Message,
                InvalidOperationException => "Operación inválida: " + error.Message,
                ArgumentException => "Argumento inválido: " + error.Message,
                _ => _env.IsDevelopment() ? error.Message : "Error interno del servidor"
            };
        }
    }

    public static class ErrorHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomErrorHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlerMiddleware>();
        }
    }
} 