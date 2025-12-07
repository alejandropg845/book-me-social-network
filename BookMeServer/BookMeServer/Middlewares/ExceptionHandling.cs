using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace BookMeServer.Middlewares
{
    public class ExceptionHandling
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandling> _logger;

        public ExceptionHandling(RequestDelegate next, ILogger<ExceptionHandling> logger)
        {
            _logger = logger;
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var problem = new ProblemDetails
                {
                    Status = context.Response.StatusCode,
                    Title = "Server error",
                    Type = "Server error",
                    Detail = "An error occurred while trying to do this request"
                };

                string serialized = JsonSerializer.Serialize(problem);
                await context.Response.WriteAsync(serialized);

            }
        }
    }
}
