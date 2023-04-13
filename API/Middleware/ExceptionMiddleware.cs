using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using API.Errors;

namespace API.Middleware
{
    public class ExceptionMiddleware
    {
        public RequestDelegate Next { get; }
        public ILogger<ExceptionMiddleware> Logger { get; }
        public IHostEnvironment Env { get; }
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            Env = env;
            Logger = logger;
            Next = next;

        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await Next(context);

            }
            catch (Exception ex)
            {

                Logger.LogError(ex, ex.Message);
                context.Response.ContentType = "Application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = Env.IsDevelopment()
                ? new ApiException(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString())
                 : new ApiException(context.Response.StatusCode, ex.Message, "Internal Server Error");
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var json = JsonSerializer.Serialize(response, options);
                await context.Response.WriteAsync(json);
            }
        }
    }
}