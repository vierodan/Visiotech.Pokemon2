using System.Net;
using System.Net.Http.Json;
using Visiotech.Pokemon.Contracts;

namespace Visiotech.Pokemon.IntegrationTests;

public sealed class SystemEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SystemEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetSystemInfo_Should_Return_Ok()
    {
        var response = await _client.GetAsync("/api/v1/system");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<SystemInfoContract>();
        Assert.NotNull(payload);
        Assert.Equal("Visiotech.Pokemon.Api", payload.Service);
    }

    [Fact]
    public async Task GetPokemons_Should_Return_Pokemons_With_Moves_And_Abilities()
    {
        var response = await _client.GetAsync("/api/v1/pokemons");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<PokemonContract[]>();
        Assert.NotNull(payload);
        Assert.NotEmpty(payload);
        Assert.All(payload, pokemon => Assert.InRange(pokemon.Moves.Count, 0, 4));
        Assert.All(payload, pokemon => Assert.InRange(pokemon.Abilities.Count, 0, 4));
    }
}
