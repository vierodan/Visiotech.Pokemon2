using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Exceptions;

namespace Visiotech.Pokemon.Application.Common.Observability;

internal sealed class LoggingQueryHandler<TQuery, TResponse>(
    IQueryHandler<TQuery, TResponse> innerHandler,
    ILogger<LoggingQueryHandler<TQuery, TResponse>> logger)
    : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    public async Task<TResponse> Handle(TQuery query, CancellationToken cancellationToken)
    {
        var queryName = typeof(TQuery).Name;
        var stopwatch = Stopwatch.StartNew();
        var payload = RequestLogContextFactory.CreateRequestPayload(query);

        logger.LogInformation(
            "Handling query {QueryName} with payload {@Payload}",
            queryName,
            payload);

        try
        {
            var response = await innerHandler.Handle(query, cancellationToken);

            logger.LogInformation(
                "Handled query {QueryName} in {ElapsedMs} ms with response {@ResponseSummary}",
                queryName,
                stopwatch.Elapsed.TotalMilliseconds,
                RequestLogContextFactory.CreateResponseSummary(response));

            return response;
        }
        catch (ApplicationValidationException exception)
        {
            logger.LogWarning(
                exception,
                "Query {QueryName} failed validation in {ElapsedMs} ms with errors {@Errors}",
                queryName,
                stopwatch.Elapsed.TotalMilliseconds,
                exception.Errors);
            throw;
        }
        catch (ApplicationNotFoundException exception)
        {
            logger.LogWarning(
                exception,
                "Query {QueryName} could not find the requested resource in {ElapsedMs} ms for target {Target}",
                queryName,
                stopwatch.Elapsed.TotalMilliseconds,
                exception.Target);
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Query {QueryName} failed unexpectedly after {ElapsedMs} ms",
                queryName,
                stopwatch.Elapsed.TotalMilliseconds);
            throw;
        }
    }
}
