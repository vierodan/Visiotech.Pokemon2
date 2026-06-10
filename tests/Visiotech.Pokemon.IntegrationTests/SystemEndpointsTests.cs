using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Visiotech.Pokemon.Contracts;
using Visiotech.Pokemon.Infrastructure.Persistence;

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

        var payload = await getResponse.Content.ReadFromJsonAsync<PokemonSpeciesCatalogContract>();
        Assert.NotNull(payload);
        Assert.Equal(1, payload.TotalCount);
        var species = Assert.Single(payload.Items);
        Assert.Equal(createdPayload.Id, species.Id);
        Assert.Equal("Charizard", species.Name);
    }

    [Fact]
    public async Task GetPokemonsCatalog_Should_List_And_Get_Detail_After_Creating_Mvp_Roster()
    {
        foreach (var pokemonSpecies in PokemonMvpRosterSeed.GetSpecies())
        {
            var createResponse = await _client.PostAsJsonAsync(
                "/api/v1/pokemons",
                new CreatePokemonSpeciesRequestContract(
                    pokemonSpecies.Name.Value,
                    pokemonSpecies.Types.Select(type => type.ToString()).ToArray(),
                    new PokemonBaseStatsContract(
                        pokemonSpecies.BaseStats.Health,
                        pokemonSpecies.BaseStats.Attack,
                        pokemonSpecies.BaseStats.Defense,
                        pokemonSpecies.BaseStats.SpecialAttack,
                        pokemonSpecies.BaseStats.SpecialDefense,
                        pokemonSpecies.BaseStats.Speed)));

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        }

        var listResponse = await _client.GetAsync("/api/v1/pokemons?page=1&pageSize=20");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var listPayload = await listResponse.Content.ReadFromJsonAsync<PokemonSpeciesCatalogContract>();
        Assert.NotNull(listPayload);
        Assert.Equal(10, listPayload.TotalCount);
        Assert.Contains(listPayload.Items, item => item.Name == "Blastoise");

        var blastoise = Assert.Single(listPayload.Items, item => item.Name == "Blastoise");
        var detailResponse = await _client.GetAsync($"/api/v1/pokemons/{blastoise.Id}");

        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);

        var detailPayload = await detailResponse.Content.ReadFromJsonAsync<PokemonSpeciesContract>();
        Assert.NotNull(detailPayload);
        Assert.Equal(blastoise.Id, detailPayload.Id);
        Assert.Equal(["Water"], detailPayload.Types);
    }

    [Fact]
    public async Task GetPokemonsCatalog_Should_Filter_And_Paginate()
    {
        await CreateSpeciesAsync("Charizard", ["Fire", "Flying"], 78, 84, 78, 109, 85, 100);
        await CreateSpeciesAsync("Charmander", ["Fire"], 39, 52, 43, 60, 50, 65);
        await CreateSpeciesAsync("Blastoise", ["Water"], 79, 83, 100, 85, 105, 78);

        var response = await _client.GetAsync("/api/v1/pokemons?type=Fire&name=char&page=2&pageSize=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<PokemonSpeciesCatalogContract>();
        Assert.NotNull(payload);
        Assert.Equal(2, payload.TotalCount);
        Assert.Equal(2, payload.TotalPages);
        Assert.Equal(2, payload.Page);
        var item = Assert.Single(payload.Items);
        Assert.Equal("Charmander", item.Name);
        Assert.Equal(["Fire"], item.Types);
    }

    [Fact]
    public async Task UpdatePokemonSpecies_Should_Update_Species_And_Keep_Mvp_Roster_Coherent()
    {
        foreach (var pokemonSpecies in PokemonMvpRosterSeed.GetSpecies())
        {
            var createResponse = await _client.PostAsJsonAsync(
                "/api/v1/pokemons",
                new CreatePokemonSpeciesRequestContract(
                    pokemonSpecies.Name.Value,
                    pokemonSpecies.Types.Select(type => type.ToString()).ToArray(),
                    new PokemonBaseStatsContract(
                        pokemonSpecies.BaseStats.Health,
                        pokemonSpecies.BaseStats.Attack,
                        pokemonSpecies.BaseStats.Defense,
                        pokemonSpecies.BaseStats.SpecialAttack,
                        pokemonSpecies.BaseStats.SpecialDefense,
                        pokemonSpecies.BaseStats.Speed)));

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        }

        var listResponse = await _client.GetAsync("/api/v1/pokemons");
        var listPayload = await listResponse.Content.ReadFromJsonAsync<PokemonSpeciesCatalogContract>();
        Assert.NotNull(listPayload);

        var charizard = Assert.Single(listPayload.Items, item => item.Name == "Charizard");

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/v1/pokemons/{charizard.Id}",
            new UpdatePokemonSpeciesRequestContract(
                "Charizard Apex",
                ["Fire", "Dragon"],
                new PokemonBaseStatsContract(80, 90, 82, 120, 90, 105)));

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updatedPayload = await updateResponse.Content.ReadFromJsonAsync<PokemonSpeciesContract>();
        Assert.NotNull(updatedPayload);
        Assert.Equal(charizard.Id, updatedPayload.Id);
        Assert.Equal(["Fire", "Dragon"], updatedPayload.Types);

        var refreshedListResponse = await _client.GetAsync("/api/v1/pokemons?page=1&pageSize=20");
        Assert.Equal(HttpStatusCode.OK, refreshedListResponse.StatusCode);

        var refreshedListPayload = await refreshedListResponse.Content.ReadFromJsonAsync<PokemonSpeciesCatalogContract>();
        Assert.NotNull(refreshedListPayload);
        Assert.Equal(10, refreshedListPayload.TotalCount);
        Assert.DoesNotContain(refreshedListPayload.Items, item => item.Name == "Charizard");

        var updatedSpecies = Assert.Single(refreshedListPayload.Items, item => item.Name == "Charizard Apex");
        Assert.Equal(charizard.Id, updatedSpecies.Id);

        var detailResponse = await _client.GetAsync($"/api/v1/pokemons/{charizard.Id}");
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);

        var detailPayload = await detailResponse.Content.ReadFromJsonAsync<PokemonSpeciesContract>();
        Assert.NotNull(detailPayload);
        Assert.Equal("Charizard Apex", detailPayload.Name);
        Assert.Equal(["Fire", "Dragon"], detailPayload.Types);
        Assert.Equal(120, detailPayload.BaseStats.SpecialAttack);
    }

    [Fact]
    public async Task UpdatePokemonSpecies_Should_Return_NotFound_When_Species_Does_Not_Exist()
    {
        var response = await _client.PutAsJsonAsync(
            $"/api/v1/pokemons/{Guid.NewGuid()}",
            new UpdatePokemonSpeciesRequestContract(
                "Charizard",
                ["Fire", "Flying"],
                new PokemonBaseStatsContract(78, 84, 78, 109, 85, 100)));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var payload = await JsonDocument.ParseAsync(responseStream);
        Assert.Equal("Not found", payload.RootElement.GetProperty("title").GetString());
        Assert.Equal("id", payload.RootElement.GetProperty("target").GetString());
    }

    [Fact]
    public async Task UpdatePokemonSpecies_Should_Return_Conflict_For_Duplicate_Name()
    {
        var firstSpecies = await CreateSpeciesAsync("Charizard", ["Fire", "Flying"], 78, 84, 78, 109, 85, 100);
        await CreateSpeciesAsync("Blastoise", ["Water"], 79, 83, 100, 85, 105, 78);

        var response = await _client.PutAsJsonAsync(
            $"/api/v1/pokemons/{firstSpecies.Id}",
            new UpdatePokemonSpeciesRequestContract(
                "Blastoise",
                ["Water"],
                new PokemonBaseStatsContract(79, 83, 100, 85, 105, 78)));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePokemonSpecies_Should_Return_Validation_Problem_For_Invalid_Types()
    {
        var species = await CreateSpeciesAsync("Golem", ["Rock", "Ground"], 80, 120, 130, 55, 65, 45);

        var response = await _client.PutAsJsonAsync(
            $"/api/v1/pokemons/{species.Id}",
            new UpdatePokemonSpeciesRequestContract(
                "Golem",
                ["Rock", "Rock", "Ground"],
                new PokemonBaseStatsContract(80, 120, 130, 55, 65, 45)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var payload = await JsonDocument.ParseAsync(responseStream);
        Assert.True(payload.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("types", out _));
    }

    [Fact]
    public async Task GetPokemonSpeciesDetail_Should_Return_NotFound_When_Species_Does_Not_Exist()
    {
        var response = await _client.GetAsync($"/api/v1/pokemons/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var payload = await JsonDocument.ParseAsync(responseStream);
        Assert.Equal("Not found", payload.RootElement.GetProperty("title").GetString());
        Assert.Equal("id", payload.RootElement.GetProperty("target").GetString());
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

    private async Task<PokemonSpeciesContract> CreateSpeciesAsync(
        string name,
        IReadOnlyCollection<string> types,
        int health,
        int attack,
        int defense,
        int specialAttack,
        int specialDefense,
        int speed)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/pokemons",
            new CreatePokemonSpeciesRequestContract(
                name,
                types,
                new PokemonBaseStatsContract(health, attack, defense, specialAttack, specialDefense, speed)));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<PokemonSpeciesContract>();
        Assert.NotNull(payload);
        return payload;
    }
}
