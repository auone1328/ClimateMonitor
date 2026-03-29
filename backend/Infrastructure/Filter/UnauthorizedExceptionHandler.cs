using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Infrastructure.Filter
{
    public sealed class UnauthorizedExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetails;

        public UnauthorizedExceptionHandler(IProblemDetailsService problemDetails)
        {
            _problemDetails = problemDetails;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
        {
            if (exception is not UnauthorizedAccessException uae) return false;

            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            httpContext.Response.ContentType = "application/problem+json; charset=utf-8";

            var problem = new ProblemDetails
            {
                Status = 401,
                Title = "Не авторизован",
                Detail = string.IsNullOrWhiteSpace(uae.Message) ? "Недостаточно прав для выполнения операции." : uae.Message
            };

            await _problemDetails.WriteAsync(new() { HttpContext = httpContext, ProblemDetails = problem });

            return true;
        }
    }
}
