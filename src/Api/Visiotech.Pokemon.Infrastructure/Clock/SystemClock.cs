using Visiotech.Pokemon.Application.Abstractions.Clock;

namespace Visiotech.Pokemon.Infrastructure.Clock;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

