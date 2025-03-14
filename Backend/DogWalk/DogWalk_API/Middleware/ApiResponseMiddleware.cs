using Microsoft.AspNetCore.Http.Features;
using System.Text.Json;

namespace DogWalk_API.Middleware
{
    public class ApiResponseMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiResponseMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // Guarda la referencia al stream original
            var originalBodyStream = context.Response.Body;

            try
            {
                // Crea un nuevo stream en memoria
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                // Continúa con la ejecución del pipeline
                await _next(context);

                // Si la respuesta ya está siendo manejada por otro middleware (como el de errores)
                // o si es una respuesta de SignalR, no la modifiques
                if (context.Response.HasStarted || 
                    context.Response.StatusCode == 101 || // WebSockets
                    context.GetEndpoint()?.DisplayName?.Contains("Hub") == true)
                {
                    responseBody.Seek(0, SeekOrigin.Begin);
                    await responseBody.CopyToAsync(originalBodyStream);
                    return;
                }

                // Lee la respuesta
                responseBody.Seek(0, SeekOrigin.Begin);
                var responseContent = await new StreamReader(responseBody).ReadToEndAsync();
                
                // Prepara el objeto de respuesta estandarizado
                object? responseObject;
                
                if (string.IsNullOrEmpty(responseContent))
                {
                    responseObject = new
                    {
                        statusCode = context.Response.StatusCode,
                        success = context.Response.StatusCode >= 200 && context.Response.StatusCode < 300,
                        message = GetDefaultMessageForStatusCode(context.Response.StatusCode),
                        data = (object?)null
                    };
                }
                else
                {
                    try
                    {
                        // Intenta deserializar la respuesta original
                        var data = JsonSerializer.Deserialize<object>(responseContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        responseObject = new
                        {
                            statusCode = context.Response.StatusCode,
                            success = context.Response.StatusCode >= 200 && context.Response.StatusCode < 300,
                            message = GetDefaultMessageForStatusCode(context.Response.StatusCode),
                            data
                        };
                    }
                    catch
                    {
                        // Si no se puede deserializar, usa la respuesta original como string
                        responseObject = new
                        {
                            statusCode = context.Response.StatusCode,
                            success = context.Response.StatusCode >= 200 && context.Response.StatusCode < 300,
                            message = GetDefaultMessageForStatusCode(context.Response.StatusCode),
                            data = responseContent
                        };
                    }
                }

                // Escribe la respuesta estandarizada
                context.Response.ContentType = "application/json";
                var jsonResponse = JsonSerializer.Serialize(responseObject);
                
                context.Response.Body = originalBodyStream;
                await context.Response.WriteAsync(jsonResponse);
            }
            finally
            {
                // Restaura el stream original si es necesario
                if (context.Response.Body != originalBodyStream)
                {
                    context.Response.Body = originalBodyStream;
                }
            }
        }

        private string GetDefaultMessageForStatusCode(int statusCode)
        {
            return statusCode switch
            {
                200 => "Operación completada con éxito",
                201 => "Recurso creado con éxito",
                204 => "Operación completada sin contenido",
                400 => "Solicitud incorrecta",
                401 => "No autorizado",
                403 => "Acceso prohibido",
                404 => "Recurso no encontrado",
                500 => "Error interno del servidor",
                _ => "Operación completada"
            };
        }
    }
} 