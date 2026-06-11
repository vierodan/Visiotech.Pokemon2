using Visiotech.Pokemon.Application.Abstractions.Randomization;

namespace Visiotech.Pokemon.Infrastructure.Randomization;

public sealed class DamageRandomProvider : IDamageRandomProvider
{
    public int Next() => Random.Shared.Next(85, 101);
}
