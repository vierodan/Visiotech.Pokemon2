using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Visiotech.Pokemon.Contracts;

namespace Visiotech.Pokemon.IntegrationTests;

public sealed class SystemEndpointsTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public SystemEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

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
    public async Task CreatePokemonSpecies_Then_GetCatalog_Should_Persist_Species()
    {
        var createResponse = await _client.PostAsJsonAsync(
            "/api/v1/pokemons",
            new CreatePokemonSpeciesRequestContract(
                "Charizard",
                ["Fire", "Flying"],
                new PokemonBaseStatsContract(78, 84, 78, 109, 85, 100)));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdPayload = await createResponse.Content.ReadFromJsonAsync<PokemonSpeciesContract>();
        Assert.NotNull(createdPayload);
        Assert.Equal(["Fire", "Flying"], createdPayload.Types);

        var getResponse = await _client.GetAsync("/api/v1/pokemons");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var payload = await getResponse.Content.ReadFromJsonAsync<PokemonSpeciesContract[]>();
        Assert.NotNull(payload);
        var species = Assert.Single(payload);
        Assert.Equal(createdPayload.Id, species.Id);
        Assert.Equal("Charizard", species.Name);
    }

    [Fact]
    public async Task CreatePokemonSpecies_Should_Return_Validation_Problem_For_Invalid_Request()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/pokemons",
            new CreatePokemonSpeciesRequestContract(
                "Pikachu",
                [],
                new PokemonBaseStatsContract(35, 55, 40, 50, 50, 90)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var payload = await JsonDocument.ParseAsync(responseStream);
        Assert.True(payload.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("types", out _));
    }

    [Fact]
    public async Task CreatePokemonSpecies_Should_Return_Conflict_For_Duplicate_Name()
    {
        var request = new CreatePokemonSpeciesRequestContract(
            "Blastoise",
            ["Water"],
            new PokemonBaseStatsContract(79, 83, 100, 85, 105, 78));

        var firstResponse = await _client.PostAsJsonAsync("/api/v1/pokemons", request);
        var secondResponse = await _client.PostAsJsonAsync("/api/v1/pokemons", request);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }
}
