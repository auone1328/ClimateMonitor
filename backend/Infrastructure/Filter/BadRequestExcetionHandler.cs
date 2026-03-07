using Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Filter
{
    public sealed class BadRequestExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetails;

        public BadRequestExceptionHandler(IProblemDetailsService problemDetails)
        {
            _problemDetails = problemDetails;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
        {
            if (exception is not BadRequestException bre) return false;

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

            var problem = new ProblemDetails
            {
                Status = 400,
                Title = "Bad Request",
                Detail = bre.Message
            };

            await _problemDetails.WriteAsync(new() { HttpContext = httpContext, ProblemDetails = problem });

            return true;
        }
    }
}
