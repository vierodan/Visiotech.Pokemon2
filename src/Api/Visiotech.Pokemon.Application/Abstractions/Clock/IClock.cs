namespace Visiotech.Pokemon.Application.Abstractions.Clock;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

