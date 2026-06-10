namespace Visiotech.Pokemon.Application.Common.Exceptions;

public sealed class ApplicationNotFoundException(string message, string? target = null) : Exception(message)
{
    public string? Target { get; } = target;
}
