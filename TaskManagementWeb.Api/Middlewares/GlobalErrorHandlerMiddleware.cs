using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Text.Json;

namespace TaskManagementWeb.Api.Middlewares
{
        public class GlobalErrorHandlerMiddleware : IMiddleware
        {
            private const string URI_TYPE_ERROR = "Error";

            private readonly ILogger<GlobalErrorHandlerMiddleware> _logger;

            public GlobalErrorHandlerMiddleware(
                ILogger<GlobalErrorHandlerMiddleware> logger)
            {
                _logger = logger;
            }

            public async Task InvokeAsync(HttpContext context, RequestDelegate next)
            {
                try
                {
                    await next(context);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, message: e.Message);

                    var details = new ProblemDetails()
                    {
                        Detail = $"Ops. {e.Message}",
                        Instance = context.Request.Path,
                        Status = StatusCodes.Status500InternalServerError,
                        Title = "Internal error",
                        Type = URI_TYPE_ERROR,
                    };

                    string response = JsonSerializer.Serialize(details);

                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = MediaTypeNames.Application.Json;
                    await context.Response.WriteAsync(response);
                }
            }
        }
}
