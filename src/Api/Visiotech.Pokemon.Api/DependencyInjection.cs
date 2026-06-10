using Microsoft.Extensions.DependencyInjection;

namespace Visiotech.Pokemon.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<Common.Exceptions.ApiExceptionHandler>();
        services.AddEndpointsApiExplorer();

        return services;
    }
}

