
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;

namespace FlightBooking.Infrastructure.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next,ILogger<GlobalExceptionMiddleware> logger)
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
            catch(Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context,Exception exception)
        {
            var requestPath = context.Request.Path;
            var requestMethod = context.Request.Method;

            _logger.LogError(exception,
                "CRITICAL PIPELINE EXCEPTION UNCAUGHT: An unhandled server error occurred during a {Method} request to path '{Path}'.",
                requestMethod, requestPath);

            
            context.Response.Redirect("/Home/Error");
            return Task.CompletedTask;
        }
    }
}
