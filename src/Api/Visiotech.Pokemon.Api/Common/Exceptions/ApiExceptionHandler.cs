using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
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
        var (statusCode, title) = exception switch
        {
            ApplicationValidationException => (HttpStatusCode.BadRequest, "Validation error"),
            ApplicationConflictException => (HttpStatusCode.Conflict, "Conflict"),
            ApplicationNotFoundException => (HttpStatusCode.NotFound, "Not found"),
            ArgumentException => (HttpStatusCode.BadRequest, "Validation error"),
            _ => (HttpStatusCode.InternalServerError, "Server error")
        };

        var endpoint = httpContext.GetEndpoint();
        var endpointName = endpoint?.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName ?? endpoint?.DisplayName ?? "unknown";

        LogException(httpContext, exception, statusCode, endpointName);

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

    private void LogException(HttpContext httpContext, Exception exception, HttpStatusCode statusCode, string endpointName)
    {
        var requestMethod = httpContext.Request.Method;
        var requestPath = httpContext.Request.Path.Value ?? "/";
        var traceId = httpContext.TraceIdentifier;

        switch (exception)
        {
            case ApplicationValidationException validationException:
                logger.LogWarning(
                    exception,
                    "Validation error while processing {RequestMethod} {RequestPath} on endpoint {EndpointName}. TraceId {TraceId}. Errors {@Errors}",
                    requestMethod,
                    requestPath,
                    endpointName,
                    traceId,
                    validationException.Errors);
                break;
            case ApplicationConflictException conflictException:
                logger.LogWarning(
                    exception,
                    "Conflict while processing {RequestMethod} {RequestPath} on endpoint {EndpointName}. TraceId {TraceId}. Target {Target}. StatusCode {StatusCode}",
                    requestMethod,
                    requestPath,
                    endpointName,
                    traceId,
                    conflictException.Target,
                    (int)statusCode);
                break;
            case ApplicationNotFoundException notFoundException:
                logger.LogWarning(
                    exception,
                    "Not found while processing {RequestMethod} {RequestPath} on endpoint {EndpointName}. TraceId {TraceId}. Target {Target}. StatusCode {StatusCode}",
                    requestMethod,
                    requestPath,
                    endpointName,
                    traceId,
                    notFoundException.Target,
                    (int)statusCode);
                break;
            default:
                logger.LogError(
                    exception,
                    "Unhandled exception while processing {RequestMethod} {RequestPath} on endpoint {EndpointName}. TraceId {TraceId}. StatusCode {StatusCode}",
                    requestMethod,
                    requestPath,
                    endpointName,
                    traceId,
                    (int)statusCode);
                break;
        }
    }
}
