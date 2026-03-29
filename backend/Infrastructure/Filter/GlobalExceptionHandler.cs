using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Filter
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetails;
        private readonly IHostEnvironment _env;

        public GlobalExceptionHandler(IProblemDetailsService problemDetails, IHostEnvironment env)
        {
            _problemDetails = problemDetails;
            _env = env;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
        {
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            httpContext.Response.ContentType = "application/problem+json; charset=utf-8";

            var problem = new ProblemDetails
            {
                Status = 500,
                Title = "Ошибка сервера",
                Detail = _env.IsDevelopment()
                    ? exception.Message
                    : "Произошла внутренняя ошибка сервера."
            };

            await _problemDetails.WriteAsync(new() { HttpContext = httpContext, ProblemDetails = problem });

            return true;
        }
    }
}
