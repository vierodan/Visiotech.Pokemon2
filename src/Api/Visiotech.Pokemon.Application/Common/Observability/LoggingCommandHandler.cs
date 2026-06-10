using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Exceptions;

namespace Visiotech.Pokemon.Application.Common.Observability;

internal sealed class LoggingCommandHandler<TCommand, TResponse>(
    ICommandHandler<TCommand, TResponse> innerHandler,
    ILogger<LoggingCommandHandler<TCommand, TResponse>> logger)
    : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public async Task<TResponse> Handle(TCommand command, CancellationToken cancellationToken)
    {
        var commandName = typeof(TCommand).Name;
        var stopwatch = Stopwatch.StartNew();
        var payload = RequestLogContextFactory.CreateRequestPayload(command);

        logger.LogInformation(
            "Handling command {CommandName} with payload {@Payload}",
            commandName,
            payload);

        try
        {
            var response = await innerHandler.Handle(command, cancellationToken);

            logger.LogInformation(
                "Handled command {CommandName} in {ElapsedMs} ms with response {@ResponseSummary}",
                commandName,
                stopwatch.Elapsed.TotalMilliseconds,
                RequestLogContextFactory.CreateResponseSummary(response));

            return response;
        }
        catch (ApplicationValidationException exception)
        {
            logger.LogWarning(
                exception,
                "Command {CommandName} failed validation in {ElapsedMs} ms with errors {@Errors}",
                commandName,
                stopwatch.Elapsed.TotalMilliseconds,
                exception.Errors);
            throw;
        }
        catch (ApplicationConflictException exception)
        {
            logger.LogWarning(
                exception,
                "Command {CommandName} completed with conflict in {ElapsedMs} ms for target {Target}",
                commandName,
                stopwatch.Elapsed.TotalMilliseconds,
                exception.Target);
            throw;
        }
        catch (ApplicationNotFoundException exception)
        {
            logger.LogWarning(
                exception,
                "Command {CommandName} could not find the requested resource in {ElapsedMs} ms for target {Target}",
                commandName,
                stopwatch.Elapsed.TotalMilliseconds,
                exception.Target);
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Command {CommandName} failed unexpectedly after {ElapsedMs} ms",
                commandName,
                stopwatch.Elapsed.TotalMilliseconds);
            throw;
        }
    }
}
