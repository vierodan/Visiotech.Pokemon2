using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Visiotech.Pokemon.Application.Common.Exceptions;

namespace Visiotech.Pokemon.Api.Common.Exceptions;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception while processing request.");

        var (statusCode, title) = exception switch
        {
            ApplicationValidationException => (HttpStatusCode.BadRequest, "Validation error"),
            ApplicationConflictException => (HttpStatusCode.Conflict, "Conflict"),
            ApplicationNotFoundException => (HttpStatusCode.NotFound, "Not found"),
            ArgumentException => (HttpStatusCode.BadRequest, "Validation error"),
            _ => (HttpStatusCode.InternalServerError, "Server error")
        };

        httpContext.Response.StatusCode = (int)statusCode;

        if (exception is ApplicationValidationException validationException)
        {
            await httpContext.Response.WriteAsJsonAsync(new HttpValidationProblemDetails(validationException.Errors)
            {
                Status = (int)statusCode,
                Title = title
            }, cancellationToken);

            return true;
        }

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = exception.Message
        };

        if (exception is ApplicationConflictException conflictException && !string.IsNullOrWhiteSpace(conflictException.Target))
        {
            problemDetails.Extensions["target"] = conflictException.Target;
        }

        if (exception is ApplicationNotFoundException notFoundException && !string.IsNullOrWhiteSpace(notFoundException.Target))
        {
            problemDetails.Extensions["target"] = notFoundException.Target;
        }

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
