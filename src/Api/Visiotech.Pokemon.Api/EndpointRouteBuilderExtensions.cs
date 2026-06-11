using Microsoft.AspNetCore.Builder;

namespace Visiotech.Pokemon.Api;

public static class EndpointRouteBuilderExtensions
{
    public static WebApplication MapApi(this WebApplication app)
    {
        app.MapBattleEndpoints();
        app.MapDamageCalculationEndpoints();
        app.MapSystemEndpoints();
        app.MapMoveEndpoints();
        app.MapPokemonEndpoints();
        app.MapMyPokemonEndpoints();

        return app;
    }
}
