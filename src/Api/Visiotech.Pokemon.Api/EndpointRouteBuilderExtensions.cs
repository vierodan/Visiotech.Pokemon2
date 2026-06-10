using Microsoft.AspNetCore.Builder;

namespace Visiotech.Pokemon.Api;

public static class EndpointRouteBuilderExtensions
{
    public static WebApplication MapApi(this WebApplication app)
    {
        app.MapSystemEndpoints();
        app.MapMoveEndpoints();
        app.MapPokemonEndpoints();

        return app;
    }
}
