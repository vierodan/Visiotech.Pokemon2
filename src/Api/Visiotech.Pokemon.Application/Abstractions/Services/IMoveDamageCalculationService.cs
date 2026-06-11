using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.Damage;

namespace Visiotech.Pokemon.Application.Abstractions.Services;

public interface IMoveDamageCalculationService
{
    Task<MoveDamageCalculationResponse> CalculateAsync(
        MoveDamageCalculationRequest request,
        CancellationToken cancellationToken);
}
