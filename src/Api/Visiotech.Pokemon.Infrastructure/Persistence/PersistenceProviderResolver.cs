using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Visiotech.Pokemon.Infrastructure.Persistence;

internal static class PersistenceProviderResolver
{
    public static PersistenceProvider Resolve(IConfiguration configuration, IHostEnvironment environment)
    {
        var configuredProvider = configuration[$"{PersistenceOptions.SectionName}:Provider"];

        if (string.IsNullOrWhiteSpace(configuredProvider))
        {
            return PersistenceProvider.Postgres;
        }

        if (!Enum.TryParse<PersistenceProvider>(configuredProvider, ignoreCase: true, out var provider))
        {
            throw new InvalidOperationException(
                $"Unsupported persistence provider '{configuredProvider}'. Allowed values: {nameof(PersistenceProvider.Postgres)}, {nameof(PersistenceProvider.InMemory)}.");
        }

        if (provider == PersistenceProvider.InMemory && !environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                $"Persistence provider '{nameof(PersistenceProvider.InMemory)}' is only allowed in Development. Current environment: '{environment.EnvironmentName}'.");
        }

        return provider;
    }
}
