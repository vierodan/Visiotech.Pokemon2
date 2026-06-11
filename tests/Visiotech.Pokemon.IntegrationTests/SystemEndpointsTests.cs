using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
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
    public async Task CreatePokemonMoves_Should_Load_Curated_Subset_For_Mvp_Catalog()
    {
        foreach (var pokemonMove in PokemonMvpMoveSeed.GetMoves())
        {
            var response = await _client.PostAsJsonAsync(
                "/api/v1/moves",
                new CreatePokemonMoveRequestContract(
                    pokemonMove.Name.Value,
                    pokemonMove.Type.ToString(),
                    pokemonMove.Category.ToString(),
                    pokemonMove.Power));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var payload = await response.Content.ReadFromJsonAsync<PokemonMoveContract>();
            Assert.NotNull(payload);
            Assert.Equal(pokemonMove.Name.Value, payload.Name);
            Assert.Equal(pokemonMove.Category.ToString(), payload.Category);
        }

        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PokemonDbContext>();
        Assert.Equal(PokemonMvpMoveSeed.GetMoves().Count, await dbContext.PokemonMoves.CountAsync());
    }

    [Fact]
    public async Task GetPokemonMovesCatalog_Should_List_And_Get_Detail_After_Creating_Curated_Subset()
    {
        foreach (var pokemonMove in PokemonMvpMoveSeed.GetMoves())
        {
            var createResponse = await _client.PostAsJsonAsync(
                "/api/v1/moves",
                new CreatePokemonMoveRequestContract(
                    pokemonMove.Name.Value,
                    pokemonMove.Type.ToString(),
                    pokemonMove.Category.ToString(),
                    pokemonMove.Power));

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        }

        var listResponse = await _client.GetAsync("/api/v1/moves?page=1&pageSize=50");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var listPayload = await listResponse.Content.ReadFromJsonAsync<PokemonMoveCatalogContract>();
        Assert.NotNull(listPayload);
        Assert.Equal(27, listPayload.TotalCount);
        Assert.Contains(listPayload.Items, item => item.Name == "Protect" && item.Category == "Status");

        var protect = Assert.Single(listPayload.Items, item => item.Name == "Protect");
        var detailResponse = await _client.GetAsync($"/api/v1/moves/{protect.Id}");

        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);

        var detailPayload = await detailResponse.Content.ReadFromJsonAsync<PokemonMoveContract>();
        Assert.NotNull(detailPayload);
        Assert.Equal(protect.Id, detailPayload.Id);
        Assert.Equal("Normal", detailPayload.Type);
        Assert.Equal("Status", detailPayload.Category);
        Assert.Equal(0, detailPayload.Power);
    }

    [Fact]
    public async Task GetPokemonMovesCatalog_Should_Filter_And_Paginate()
    {
        await CreateMoveAsync("Thunderbolt", "Electric", "Special", 90);
        await CreateMoveAsync("Thunder Punch", "Electric", "Physical", 75);
        await CreateMoveAsync("Surf", "Water", "Special", 90);

        var response = await _client.GetAsync("/api/v1/moves?type=Electric&category=Special&name=thunder&page=1&pageSize=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<PokemonMoveCatalogContract>();
        Assert.NotNull(payload);
        Assert.Equal(1, payload.TotalCount);
        Assert.Equal(1, payload.TotalPages);
        Assert.Equal(1, payload.Page);
        var item = Assert.Single(payload.Items);
        Assert.Equal("Thunderbolt", item.Name);
        Assert.Equal("Electric", item.Type);
        Assert.Equal("Special", item.Category);
    }

    [Fact]
    public async Task GetPokemonMoveDetail_Should_Return_NotFound_When_Move_Does_Not_Exist()
    {
        var response = await _client.GetAsync($"/api/v1/moves/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var payload = await JsonDocument.ParseAsync(responseStream);
        Assert.Equal("Not found", payload.RootElement.GetProperty("title").GetString());
        Assert.Equal("id", payload.RootElement.GetProperty("target").GetString());
    }

    [Fact]
    public async Task UpdatePokemonMove_Should_Update_Move_And_Keep_Curated_Catalog_Coherent()
    {
        foreach (var pokemonMove in PokemonMvpMoveSeed.GetMoves())
        {
            var createResponse = await _client.PostAsJsonAsync(
                "/api/v1/moves",
                new CreatePokemonMoveRequestContract(
                    pokemonMove.Name.Value,
                    pokemonMove.Type.ToString(),
                    pokemonMove.Category.ToString(),
                    pokemonMove.Power));

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        }

        var listResponse = await _client.GetAsync("/api/v1/moves?page=1&pageSize=50");
        var listPayload = await listResponse.Content.ReadFromJsonAsync<PokemonMoveCatalogContract>();
        Assert.NotNull(listPayload);

        var thunderbolt = Assert.Single(listPayload.Items, item => item.Name == "Thunderbolt");

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/v1/moves/{thunderbolt.Id}",
            new UpdatePokemonMoveRequestContract("Thunder Strike", "Electric", "Special", 95));

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updatedPayload = await updateResponse.Content.ReadFromJsonAsync<PokemonMoveContract>();
        Assert.NotNull(updatedPayload);
        Assert.Equal(thunderbolt.Id, updatedPayload.Id);
        Assert.Equal("Special", updatedPayload.Category);
        Assert.Equal(95, updatedPayload.Power);

        var refreshedListResponse = await _client.GetAsync("/api/v1/moves?page=1&pageSize=50");
        Assert.Equal(HttpStatusCode.OK, refreshedListResponse.StatusCode);

        var refreshedListPayload = await refreshedListResponse.Content.ReadFromJsonAsync<PokemonMoveCatalogContract>();
        Assert.NotNull(refreshedListPayload);
        Assert.Equal(27, refreshedListPayload.TotalCount);
        Assert.DoesNotContain(refreshedListPayload.Items, item => item.Name == "Thunderbolt");

        var updatedMove = Assert.Single(refreshedListPayload.Items, item => item.Name == "Thunder Strike");
        Assert.Equal(thunderbolt.Id, updatedMove.Id);
        Assert.Equal("Electric", updatedMove.Type);
        Assert.Equal("Special", updatedMove.Category);

        var detailResponse = await _client.GetAsync($"/api/v1/moves/{thunderbolt.Id}");
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);

        var detailPayload = await detailResponse.Content.ReadFromJsonAsync<PokemonMoveContract>();
        Assert.NotNull(detailPayload);
        Assert.Equal("Thunder Strike", detailPayload.Name);
        Assert.Equal(95, detailPayload.Power);
    }

    [Fact]
    public async Task UpdatePokemonMove_Should_Return_NotFound_When_Move_Does_Not_Exist()
    {
        var response = await _client.PutAsJsonAsync(
            $"/api/v1/moves/{Guid.NewGuid()}",
            new UpdatePokemonMoveRequestContract("Surf", "Water", "Special", 90));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var payload = await JsonDocument.ParseAsync(responseStream);
        Assert.Equal("Not found", payload.RootElement.GetProperty("title").GetString());
        Assert.Equal("id", payload.RootElement.GetProperty("target").GetString());
    }

    [Fact]
    public async Task UpdatePokemonMove_Should_Return_Conflict_For_Duplicate_Name()
    {
        var thunderbolt = await CreateMoveAsync("Thunderbolt", "Electric", "Special", 90);
        await CreateMoveAsync("Surf", "Water", "Special", 90);

        var response = await _client.PutAsJsonAsync(
            $"/api/v1/moves/{thunderbolt.Id}",
            new UpdatePokemonMoveRequestContract("Surf", "Water", "Special", 90));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePokemonMove_Should_Return_Validation_Problem_For_Invalid_Data()
    {
        var protect = await CreateMoveAsync("Protect", "Normal", "Status", 0);

        var response = await _client.PutAsJsonAsync(
            $"/api/v1/moves/{protect.Id}",
            new UpdatePokemonMoveRequestContract("Protect", "Normal", "Status", 10));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var payload = await JsonDocument.ParseAsync(responseStream);
        Assert.True(payload.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("power", out _));
    }

    [Fact]
    public async Task DeletePokemonMove_Should_Remove_Move_And_Keep_Curated_Catalog_Coherent()
    {
        foreach (var pokemonMove in PokemonMvpMoveSeed.GetMoves())
        {
            var createResponse = await _client.PostAsJsonAsync(
                "/api/v1/moves",
                new CreatePokemonMoveRequestContract(
                    pokemonMove.Name.Value,
                    pokemonMove.Type.ToString(),
                    pokemonMove.Category.ToString(),
                    pokemonMove.Power));

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        }

        var beforeDeleteResponse = await _client.GetAsync("/api/v1/moves?page=1&pageSize=50");
        var beforeDeletePayload = await beforeDeleteResponse.Content.ReadFromJsonAsync<PokemonMoveCatalogContract>();
        Assert.NotNull(beforeDeletePayload);

        var protect = Assert.Single(beforeDeletePayload.Items, item => item.Name == "Protect");

        var deleteResponse = await _client.DeleteAsync($"/api/v1/moves/{protect.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var listResponse = await _client.GetAsync("/api/v1/moves?page=1&pageSize=50");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var listPayload = await listResponse.Content.ReadFromJsonAsync<PokemonMoveCatalogContract>();
        Assert.NotNull(listPayload);
        Assert.Equal(26, listPayload.TotalCount);
        Assert.DoesNotContain(listPayload.Items, item => item.Id == protect.Id);
        Assert.Contains(listPayload.Items, item => item.Name == "Surf");

        var detailResponse = await _client.GetAsync($"/api/v1/moves/{protect.Id}");
        Assert.Equal(HttpStatusCode.NotFound, detailResponse.StatusCode);
    }

    [Fact]
    public async Task DeletePokemonMove_Should_Return_Validation_Problem_When_Dependencies_Exist()
    {
        var move = await CreateMoveAsync("Protect", "Normal", "Status", 0);

        using var blockingClient = CreateClientWithMoveDeletionDependencies(
            "Pokemon move cannot be deleted because it is referenced by 'MyPokemonMoveSlot'.");

        var response = await blockingClient.DeleteAsync($"/api/v1/moves/{move.Id}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var payload = await JsonDocument.ParseAsync(responseStream);
        Assert.True(payload.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("dependencies", out var dependencyErrors));
        Assert.Contains(
            dependencyErrors.EnumerateArray().Select(static item => item.GetString()),
            message => message == "Pokemon move cannot be deleted because it is referenced by 'MyPokemonMoveSlot'.");

        var detailResponse = await _client.GetAsync($"/api/v1/moves/{move.Id}");
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
    }

    [Fact]
    public async Task UpdatePokemonSpeciesLearnableMoves_Should_Associate_Moves_To_Species()
    {
        var blastoise = await CreateSpeciesAsync("Blastoise", ["Water"], 79, 83, 100, 85, 105, 78);
        var surf = await CreateMoveAsync("Surf", "Water", "Special", 90);
        var protect = await CreateMoveAsync("Protect", "Normal", "Status", 0);

        var response = await _client.PutAsJsonAsync(
            $"/api/v1/pokemons/{blastoise.Id}/learnable-moves",
            new UpdatePokemonLearnableMovesRequestContract([surf.Id, protect.Id], []));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<PokemonLearnableMovesContract>();
        Assert.NotNull(payload);
        Assert.Equal(blastoise.Id, payload.PokemonSpeciesId);
        Assert.Equal("Blastoise", payload.PokemonSpeciesName);
        Assert.Equal(2, payload.Moves.Count);
        Assert.Contains(payload.Moves, move => move.Id == surf.Id && move.Name == "Surf");
        Assert.Contains(payload.Moves, move => move.Id == protect.Id && move.Name == "Protect");

        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PokemonDbContext>();
        Assert.Equal(2, await dbContext.PokemonLearnableMoves.CountAsync());
    }

    [Fact]
    public async Task UpdatePokemonSpeciesLearnableMoves_Should_Remove_Existing_Association()
    {
        var venusaur = await CreateSpeciesAsync("Venusaur", ["Grass", "Poison"], 80, 82, 83, 100, 100, 80);
        var solarBeam = await CreateMoveAsync("Solar Beam", "Grass", "Special", 120);
        var sludgeWave = await CreateMoveAsync("Sludge Wave", "Poison", "Special", 95);

        var associateResponse = await _client.PutAsJsonAsync(
            $"/api/v1/pokemons/{venusaur.Id}/learnable-moves",
            new UpdatePokemonLearnableMovesRequestContract([solarBeam.Id, sludgeWave.Id], []));
        Assert.Equal(HttpStatusCode.OK, associateResponse.StatusCode);

        var removeResponse = await _client.PutAsJsonAsync(
            $"/api/v1/pokemons/{venusaur.Id}/learnable-moves",
            new UpdatePokemonLearnableMovesRequestContract([], [sludgeWave.Id]));

        Assert.Equal(HttpStatusCode.OK, removeResponse.StatusCode);

        var payload = await removeResponse.Content.ReadFromJsonAsync<PokemonLearnableMovesContract>();
        Assert.NotNull(payload);
        var remainingMove = Assert.Single(payload.Moves);
        Assert.Equal(solarBeam.Id, remainingMove.Id);
    }

    [Fact]
    public async Task UpdatePokemonSpeciesLearnableMoves_Should_Return_Validation_Problem_For_Duplicate_Association()
    {
        var charizard = await CreateSpeciesAsync("Charizard", ["Fire", "Flying"], 78, 84, 78, 109, 85, 100);
        var flamethrower = await CreateMoveAsync("Flamethrower", "Fire", "Special", 90);

        var firstResponse = await _client.PutAsJsonAsync(
            $"/api/v1/pokemons/{charizard.Id}/learnable-moves",
            new UpdatePokemonLearnableMovesRequestContract([flamethrower.Id], []));
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        var duplicateResponse = await _client.PutAsJsonAsync(
            $"/api/v1/pokemons/{charizard.Id}/learnable-moves",
            new UpdatePokemonLearnableMovesRequestContract([flamethrower.Id], []));

        Assert.Equal(HttpStatusCode.BadRequest, duplicateResponse.StatusCode);

        await using var responseStream = await duplicateResponse.Content.ReadAsStreamAsync();
        using var payload = await JsonDocument.ParseAsync(responseStream);
        Assert.True(payload.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("addMoveIds", out _));
    }

    [Fact]
    public async Task UpdatePokemonSpeciesLearnableMoves_Should_Return_Error_For_Inexistent_References()
    {
        var response = await _client.PutAsJsonAsync(
            $"/api/v1/pokemons/{Guid.NewGuid()}/learnable-moves",
            new UpdatePokemonLearnableMovesRequestContract([Guid.NewGuid()], []));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var species = await CreateSpeciesAsync("Snorlax", ["Normal"], 160, 110, 65, 65, 110, 30);

        var missingMoveResponse = await _client.PutAsJsonAsync(
            $"/api/v1/pokemons/{species.Id}/learnable-moves",
            new UpdatePokemonLearnableMovesRequestContract([Guid.NewGuid()], []));

        Assert.Equal(HttpStatusCode.BadRequest, missingMoveResponse.StatusCode);

        await using var responseStream = await missingMoveResponse.Content.ReadAsStreamAsync();
        using var payload = await JsonDocument.ParseAsync(responseStream);
        Assert.True(payload.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("moveIds", out _));
    }

    [Fact]
    public async Task GetPokemonSpeciesLearnableMoves_Should_Return_Learnable_Move_List_For_Blastoise()
    {
        var blastoise = await CreateSpeciesAsync("Blastoise", ["Water"], 79, 83, 100, 85, 105, 78);
        var surf = await CreateMoveAsync("Surf", "Water", "Special", 90);
        var iceBeam = await CreateMoveAsync("Ice Beam", "Ice", "Special", 90);
        var protect = await CreateMoveAsync("Protect", "Normal", "Status", 0);

        var associationResponse = await _client.PutAsJsonAsync(
            $"/api/v1/pokemons/{blastoise.Id}/learnable-moves",
            new UpdatePokemonLearnableMovesRequestContract([surf.Id, iceBeam.Id, protect.Id], []));
        Assert.Equal(HttpStatusCode.OK, associationResponse.StatusCode);

        var response = await _client.GetAsync($"/api/v1/pokemons/{blastoise.Id}/learnable-moves");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<PokemonLearnableMovesContract>();
        Assert.NotNull(payload);
        Assert.Equal(blastoise.Id, payload.PokemonSpeciesId);
        Assert.Equal("Blastoise", payload.PokemonSpeciesName);
        Assert.Equal(3, payload.Moves.Count);
        Assert.Collection(
            payload.Moves,
            move => Assert.Equal(("Ice Beam", "Ice", "Special", 90), (move.Name, move.Type, move.Category, move.Power)),
            move => Assert.Equal(("Protect", "Normal", "Status", 0), (move.Name, move.Type, move.Category, move.Power)),
            move => Assert.Equal(("Surf", "Water", "Special", 90), (move.Name, move.Type, move.Category, move.Power)));
    }

    [Fact]
    public async Task GetPokemonSpeciesLearnableMoves_Should_Return_NotFound_When_Species_Does_Not_Exist()
    {
        var response = await _client.GetAsync($"/api/v1/pokemons/{Guid.NewGuid()}/learnable-moves");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var payload = await JsonDocument.ParseAsync(responseStream);
        Assert.Equal("Not found", payload.RootElement.GetProperty("title").GetString());
        Assert.Equal("id", payload.RootElement.GetProperty("target").GetString());
    }

    [Fact]
    public async Task GetPokemonSpeciesLearnableMoves_Should_Return_Empty_List_When_Species_Has_No_Associations()
    {
        var ditto = await CreateSpeciesAsync("Ditto", ["Normal"], 48, 48, 48, 48, 48, 48);

        var response = await _client.GetAsync($"/api/v1/pokemons/{ditto.Id}/learnable-moves");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<PokemonLearnableMovesContract>();
        Assert.NotNull(payload);
        Assert.Equal(ditto.Id, payload.PokemonSpeciesId);
        Assert.Equal("Ditto", payload.PokemonSpeciesName);
        Assert.Empty(payload.Moves);
    }

    [Fact]
    public async Task GetPokemonMoveSharedSpecies_Should_Return_Species_Sharing_Protect()
    {
        var charizard = await CreateSpeciesAsync("Charizard", ["Fire", "Flying"], 78, 84, 78, 109, 85, 100);
        var blastoise = await CreateSpeciesAsync("Blastoise", ["Water"], 79, 83, 100, 85, 105, 78);
        var venusaur = await CreateSpeciesAsync("Venusaur", ["Grass", "Poison"], 80, 82, 83, 100, 100, 80);
        var protect = await CreateMoveAsync("Protect", "Normal", "Status", 0);

        foreach (var speciesId in new[] { charizard.Id, blastoise.Id, venusaur.Id })
        {
            var associationResponse = await _client.PutAsJsonAsync(
                $"/api/v1/pokemons/{speciesId}/learnable-moves",
                new UpdatePokemonLearnableMovesRequestContract([protect.Id], []));

            Assert.Equal(HttpStatusCode.OK, associationResponse.StatusCode);
        }

        var response = await _client.GetAsync($"/api/v1/moves/{protect.Id}/pokemon-species");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<PokemonMoveSharedSpeciesContract>();
        Assert.NotNull(payload);
        Assert.Equal(protect.Id, payload.PokemonMoveId);
        Assert.Equal("Protect", payload.PokemonMoveName);
        Assert.Equal(3, payload.PokemonSpecies.Count);
        Assert.Collection(
            payload.PokemonSpecies,
            species => Assert.Equal("Blastoise", species.Name),
            species => Assert.Equal("Charizard", species.Name),
            species => Assert.Equal("Venusaur", species.Name));
    }

    [Fact]
    public async Task GetPokemonMoveSharedSpecies_Should_Return_NotFound_When_Move_Does_Not_Exist()
    {
        var response = await _client.GetAsync($"/api/v1/moves/{Guid.NewGuid()}/pokemon-species");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var payload = await JsonDocument.ParseAsync(responseStream);
        Assert.Equal("Not found", payload.RootElement.GetProperty("title").GetString());
        Assert.Equal("id", payload.RootElement.GetProperty("target").GetString());
    }

    [Fact]
    public async Task GetPokemonMoveSharedSpecies_Should_Return_Empty_List_When_Move_Has_No_Associations()
    {
        var protect = await CreateMoveAsync("Protect", "Normal", "Status", 0);

        var response = await _client.GetAsync($"/api/v1/moves/{protect.Id}/pokemon-species");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<PokemonMoveSharedSpeciesContract>();
        Assert.NotNull(payload);
        Assert.Equal(protect.Id, payload.PokemonMoveId);
        Assert.Equal("Protect", payload.PokemonMoveName);
        Assert.Empty(payload.PokemonSpecies);
    }

    [Fact]
    public async Task CreateMyPokemon_Should_Create_Playable_Instance_When_Request_Is_Valid()
    {
        var charizard = await CreateSpeciesAsync("Charizard", ["Fire", "Flying"], 78, 84, 78, 109, 85, 100);
        var flamethrower = await CreateMoveAsync("Flamethrower", "Fire", "Special", 90);
        var fly = await CreateMoveAsync("Fly", "Flying", "Physical", 90);

        var associationResponse = await _client.PutAsJsonAsync(
            $"/api/v1/pokemons/{charizard.Id}/learnable-moves",
            new UpdatePokemonLearnableMovesRequestContract([flamethrower.Id, fly.Id], []));
        Assert.Equal(HttpStatusCode.OK, associationResponse.StatusCode);

        var response = await _client.PostAsJsonAsync(
            "/api/v1/my-pokemons",
            new CreateMyPokemonRequestContract(charizard.Id, 50, 120, 150, [flamethrower.Id, fly.Id]));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<MyPokemonContract>();
        Assert.NotNull(payload);
        Assert.Equal("Charizard", payload.Species.Name);
        Assert.Equal(50, payload.Level);
        Assert.Equal(120, payload.CurrentHealthPoints);
        Assert.Equal(150, payload.TotalHealthPoints);
        Assert.Equal(2, payload.EquippedMoves.Count);
        Assert.Collection(
            payload.EquippedMoves,
            move => Assert.Equal("Flamethrower", move.Name),
            move => Assert.Equal("Fly", move.Name));

        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PokemonDbContext>();
        Assert.Equal(1, await dbContext.MyPokemons.CountAsync());
        Assert.Equal(2, await dbContext.MyPokemonMoveSlots.CountAsync());
    }

    [Fact]
    public async Task CreateMyPokemon_Should_Return_NotFound_When_Species_Does_Not_Exist()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/my-pokemons",
            new CreateMyPokemonRequestContract(Guid.NewGuid(), 50, 120, 150, [Guid.NewGuid()]));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var payload = await JsonDocument.ParseAsync(responseStream);
        Assert.Equal("Not found", payload.RootElement.GetProperty("title").GetString());
        Assert.Equal("pokemonSpeciesId", payload.RootElement.GetProperty("target").GetString());
    }

    [Fact]
    public async Task CreateMyPokemon_Should_Return_Validation_Problem_For_Invalid_Level()
    {
        var species = await CreateSpeciesAsync("Blastoise", ["Water"], 79, 83, 100, 85, 105, 78);
        var surf = await CreateMoveAsync("Surf", "Water", "Special", 90);

        var associationResponse = await _client.PutAsJsonAsync(
            $"/api/v1/pokemons/{species.Id}/learnable-moves",
            new UpdatePokemonLearnableMovesRequestContract([surf.Id], []));
        Assert.Equal(HttpStatusCode.OK, associationResponse.StatusCode);

        var response = await _client.PostAsJsonAsync(
            "/api/v1/my-pokemons",
            new CreateMyPokemonRequestContract(species.Id, 0, 100, 120, [surf.Id]));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var payload = await JsonDocument.ParseAsync(responseStream);
        Assert.True(payload.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("level", out _));
    }

    [Fact]
    public async Task CreateMyPokemon_Should_Return_Validation_Problem_For_Inconsistent_Health_Points()
    {
        var species = await CreateSpeciesAsync("Venusaur", ["Grass", "Poison"], 80, 82, 83, 100, 100, 80);
        var solarBeam = await CreateMoveAsync("Solar Beam", "Grass", "Special", 120);

        var associationResponse = await _client.PutAsJsonAsync(
            $"/api/v1/pokemons/{species.Id}/learnable-moves",
            new UpdatePokemonLearnableMovesRequestContract([solarBeam.Id], []));
        Assert.Equal(HttpStatusCode.OK, associationResponse.StatusCode);

        var response = await _client.PostAsJsonAsync(
            "/api/v1/my-pokemons",
            new CreateMyPokemonRequestContract(species.Id, 45, 160, 120, [solarBeam.Id]));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var payload = await JsonDocument.ParseAsync(responseStream);
        Assert.True(payload.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("currentHealthPoints", out _));
    }

    [Fact]
    public async Task CreateMyPokemon_Should_Return_Validation_Problem_For_NonLearnable_Move()
    {
        var species = await CreateSpeciesAsync("Pikachu", ["Electric"], 35, 55, 40, 50, 50, 90);
        var thunderbolt = await CreateMoveAsync("Thunderbolt", "Electric", "Special", 90);
        var surf = await CreateMoveAsync("Surf", "Water", "Special", 90);

        var associationResponse = await _client.PutAsJsonAsync(
            $"/api/v1/pokemons/{species.Id}/learnable-moves",
            new UpdatePokemonLearnableMovesRequestContract([thunderbolt.Id], []));
        Assert.Equal(HttpStatusCode.OK, associationResponse.StatusCode);

        var response = await _client.PostAsJsonAsync(
            "/api/v1/my-pokemons",
            new CreateMyPokemonRequestContract(species.Id, 30, 70, 90, [thunderbolt.Id, surf.Id]));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var payload = await JsonDocument.ParseAsync(responseStream);
        Assert.True(payload.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("equippedMoveIds", out var moveErrors));
        Assert.Contains(moveErrors.EnumerateArray().Select(static item => item.GetString()), message => message?.Contains("not learnable", StringComparison.OrdinalIgnoreCase) == true);
    }

    [Fact]
    public async Task CreateMyPokemon_Should_Return_Validation_Problem_For_Duplicate_Moves()
    {
        var species = await CreateSpeciesAsync("Gengar", ["Ghost", "Poison"], 60, 65, 60, 130, 75, 110);
        var shadowBall = await CreateMoveAsync("Shadow Ball", "Ghost", "Special", 80);

        var associationResponse = await _client.PutAsJsonAsync(
            $"/api/v1/pokemons/{species.Id}/learnable-moves",
            new UpdatePokemonLearnableMovesRequestContract([shadowBall.Id], []));
        Assert.Equal(HttpStatusCode.OK, associationResponse.StatusCode);

        var response = await _client.PostAsJsonAsync(
            "/api/v1/my-pokemons",
            new CreateMyPokemonRequestContract(species.Id, 45, 90, 110, [shadowBall.Id, shadowBall.Id]));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var payload = await JsonDocument.ParseAsync(responseStream);
        Assert.True(payload.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("equippedMoveIds", out _));
    }

    [Fact]
    public async Task CreateMyPokemon_Should_Return_Validation_Problem_For_More_Than_Four_Moves()
    {
        var species = await CreateSpeciesAsync("Dragonite", ["Dragon", "Flying"], 91, 134, 95, 100, 100, 80);
        var moveIds = new List<Guid>();

        foreach (var move in new[]
                 {
                     await CreateMoveAsync("Hyper Beam", "Normal", "Special", 150),
                     await CreateMoveAsync("Earthquake", "Ground", "Physical", 100),
                     await CreateMoveAsync("Air Slash", "Flying", "Special", 75),
                     await CreateMoveAsync("Thunder Punch", "Electric", "Physical", 75),
                     await CreateMoveAsync("Ice Punch", "Ice", "Physical", 75)
                 })
        {
            moveIds.Add(move.Id);
        }

        var associationResponse = await _client.PutAsJsonAsync(
            $"/api/v1/pokemons/{species.Id}/learnable-moves",
            new UpdatePokemonLearnableMovesRequestContract(moveIds, []));
        Assert.Equal(HttpStatusCode.OK, associationResponse.StatusCode);

        var response = await _client.PostAsJsonAsync(
            "/api/v1/my-pokemons",
            new CreateMyPokemonRequestContract(species.Id, 55, 140, 160, moveIds));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var payload = await JsonDocument.ParseAsync(responseStream);
        Assert.True(payload.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("equippedMoveIds", out _));
    }

    [Fact]
    public async Task GetMyPokemonsCatalog_Should_List_Playable_Instances_With_Species_And_Equipped_Moves()
    {
        var charizard = await CreateSpeciesAsync("Charizard", ["Fire", "Flying"], 78, 84, 78, 109, 85, 100);
        var blastoise = await CreateSpeciesAsync("Blastoise", ["Water"], 79, 83, 100, 85, 105, 78);
        var flamethrower = await CreateMoveAsync("Flamethrower", "Fire", "Special", 90);
        var fly = await CreateMoveAsync("Fly", "Flying", "Physical", 90);
        var surf = await CreateMoveAsync("Surf", "Water", "Special", 90);
        var protect = await CreateMoveAsync("Protect", "Normal", "Status", 0);

        await AssociateLearnableMovesAsync(charizard.Id, flamethrower.Id, fly.Id);
        await AssociateLearnableMovesAsync(blastoise.Id, surf.Id, protect.Id);

        var firstMyPokemon = await CreateMyPokemonAsync(charizard.Id, 50, 120, 150, flamethrower.Id, fly.Id);
        var secondMyPokemon = await CreateMyPokemonAsync(blastoise.Id, 45, 110, 140, surf.Id, protect.Id);

        var response = await _client.GetAsync("/api/v1/my-pokemons?page=1&pageSize=20");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<MyPokemonCatalogContract>();
        Assert.NotNull(payload);
        Assert.Equal(2, payload.TotalCount);
        Assert.Equal(1, payload.Page);
        Assert.Equal(20, payload.PageSize);
        Assert.Equal(1, payload.TotalPages);
        Assert.Contains(payload.Items, item => item.Id == firstMyPokemon.Id && item.Species.Name == "Charizard");
        Assert.Contains(payload.Items, item => item.Id == secondMyPokemon.Id && item.Species.Name == "Blastoise");

        var listedCharizard = Assert.Single(payload.Items, item => item.Id == firstMyPokemon.Id);
        Assert.Equal(50, listedCharizard.Level);
        Assert.Equal(120, listedCharizard.CurrentHealthPoints);
        Assert.Equal(150, listedCharizard.TotalHealthPoints);
        Assert.Collection(
            listedCharizard.EquippedMoves,
            move => Assert.Equal("Flamethrower", move.Name),
            move => Assert.Equal("Fly", move.Name));
    }

    [Fact]
    public async Task GetMyPokemonDetail_Should_Return_Instance_When_It_Exists()
    {
        var species = await CreateSpeciesAsync("Venusaur", ["Grass", "Poison"], 80, 82, 83, 100, 100, 80);
        var solarBeam = await CreateMoveAsync("Solar Beam", "Grass", "Special", 120);
        var sludgeWave = await CreateMoveAsync("Sludge Wave", "Poison", "Special", 95);

        await AssociateLearnableMovesAsync(species.Id, solarBeam.Id, sludgeWave.Id);
        var myPokemon = await CreateMyPokemonAsync(species.Id, 55, 130, 160, solarBeam.Id, sludgeWave.Id);

        var response = await _client.GetAsync($"/api/v1/my-pokemons/{myPokemon.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<MyPokemonContract>();
        Assert.NotNull(payload);
        Assert.Equal(myPokemon.Id, payload.Id);
        Assert.Equal("Venusaur", payload.Species.Name);
        Assert.Equal(["Grass", "Poison"], payload.Species.Types);
        Assert.Equal(55, payload.Level);
        Assert.Equal(130, payload.CurrentHealthPoints);
        Assert.Equal(160, payload.TotalHealthPoints);
        Assert.Collection(
            payload.EquippedMoves,
            move => Assert.Equal("Solar Beam", move.Name),
            move => Assert.Equal("Sludge Wave", move.Name));
    }

    [Fact]
    public async Task GetMyPokemonDetail_Should_Return_NotFound_When_Instance_Does_Not_Exist()
    {
        var response = await _client.GetAsync($"/api/v1/my-pokemons/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var payload = await JsonDocument.ParseAsync(responseStream);
        Assert.Equal("Not found", payload.RootElement.GetProperty("title").GetString());
        Assert.Equal("id", payload.RootElement.GetProperty("target").GetString());
    }

    [Fact]
    public async Task UpdateMyPokemon_Should_Reequip_Moves_And_Update_Battle_State_When_Request_Is_Valid()
    {
        var charizard = await CreateSpeciesAsync("Charizard", ["Fire", "Flying"], 78, 84, 78, 109, 85, 100);
        var flamethrower = await CreateMoveAsync("Flamethrower", "Fire", "Special", 90);
        var fly = await CreateMoveAsync("Fly", "Flying", "Physical", 90);
        var airSlash = await CreateMoveAsync("Air Slash", "Flying", "Special", 75);
        var protect = await CreateMoveAsync("Protect", "Normal", "Status", 0);

        await AssociateLearnableMovesAsync(charizard.Id, flamethrower.Id, fly.Id, airSlash.Id, protect.Id);
        var myPokemon = await CreateMyPokemonAsync(charizard.Id, 50, 120, 150, flamethrower.Id, fly.Id);
        var payload = await UpdateMyPokemonAsync(
            myPokemon.Id,
            55,
            140,
            170,
            airSlash.Id,
            protect.Id,
            flamethrower.Id,
            fly.Id);

        Assert.Equal(myPokemon.Id, payload.Id);
        Assert.Equal(charizard.Id, payload.Species.Id);
        Assert.Equal("Charizard", payload.Species.Name);
        Assert.Equal(55, payload.Level);
        Assert.Equal(140, payload.CurrentHealthPoints);
        Assert.Equal(170, payload.TotalHealthPoints);
        Assert.Collection(
            payload.EquippedMoves,
            move => Assert.Equal("Air Slash", move.Name),
            move => Assert.Equal("Protect", move.Name),
            move => Assert.Equal("Flamethrower", move.Name),
            move => Assert.Equal("Fly", move.Name));

        var speciesResponse = await _client.GetAsync($"/api/v1/pokemons/{charizard.Id}");
        Assert.Equal(HttpStatusCode.OK, speciesResponse.StatusCode);

        var speciesPayload = await speciesResponse.Content.ReadFromJsonAsync<PokemonSpeciesContract>();
        Assert.NotNull(speciesPayload);
        Assert.Equal("Charizard", speciesPayload.Name);
        Assert.Equal(["Fire", "Flying"], speciesPayload.Types);

        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PokemonDbContext>();

        var storedMyPokemon = await dbContext.MyPokemons.SingleAsync(item => item.Id == myPokemon.Id);
        Assert.Equal(55, storedMyPokemon.Level.Value);
        Assert.Equal(140, storedMyPokemon.CurrentHealthPoints);
        Assert.Equal(170, storedMyPokemon.TotalHealthPoints);

        var storedSlots = await dbContext.MyPokemonMoveSlots
            .Where(slot => slot.MyPokemonId == myPokemon.Id)
            .OrderBy(slot => slot.SlotNumber)
            .ToArrayAsync();

        Assert.Equal(4, storedSlots.Length);
        Assert.Equal(
            [airSlash.Id, protect.Id, flamethrower.Id, fly.Id],
            storedSlots.Select(slot => slot.PokemonMoveId).ToArray());
    }

    [Fact]
    public async Task UpdateMyPokemon_Should_Return_NotFound_When_Instance_Does_Not_Exist()
    {
        var response = await _client.PutAsJsonAsync(
            $"/api/v1/my-pokemons/{Guid.NewGuid()}",
            new UpdateMyPokemonRequestContract(55, 140, 170, [Guid.NewGuid()]));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var payload = await JsonDocument.ParseAsync(responseStream);
        Assert.Equal("Not found", payload.RootElement.GetProperty("title").GetString());
        Assert.Equal("id", payload.RootElement.GetProperty("target").GetString());
    }

    [Fact]
    public async Task UpdateMyPokemon_Should_Return_Validation_Problem_For_NonLearnable_Move()
    {
        var blastoise = await CreateSpeciesAsync("Blastoise", ["Water"], 79, 83, 100, 85, 105, 78);
        var surf = await CreateMoveAsync("Surf", "Water", "Special", 90);
        var thunderbolt = await CreateMoveAsync("Thunderbolt", "Electric", "Special", 90);

        await AssociateLearnableMovesAsync(blastoise.Id, surf.Id);
        var myPokemon = await CreateMyPokemonAsync(blastoise.Id, 45, 110, 140, surf.Id);

        var response = await _client.PutAsJsonAsync(
            $"/api/v1/my-pokemons/{myPokemon.Id}",
            new UpdateMyPokemonRequestContract(48, 115, 140, [surf.Id, thunderbolt.Id]));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var payload = await JsonDocument.ParseAsync(responseStream);
        Assert.True(payload.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("equippedMoveIds", out var moveErrors));
        Assert.Contains(
            moveErrors.EnumerateArray().Select(static item => item.GetString()),
            message => message?.Contains("not learnable", StringComparison.OrdinalIgnoreCase) == true);
    }

    [Fact]
    public async Task UpdateMyPokemon_Should_Return_Validation_Problem_For_More_Than_Four_Moves()
    {
        var dragonite = await CreateSpeciesAsync("Dragonite", ["Dragon", "Flying"], 91, 134, 95, 100, 100, 80);
        var moveIds = new List<Guid>();

        foreach (var move in new[]
                 {
                     await CreateMoveAsync("Hyper Beam", "Normal", "Special", 150),
                     await CreateMoveAsync("Earthquake", "Ground", "Physical", 100),
                     await CreateMoveAsync("Air Slash", "Flying", "Special", 75),
                     await CreateMoveAsync("Thunder Punch", "Electric", "Physical", 75),
                     await CreateMoveAsync("Ice Punch", "Ice", "Physical", 75)
                 })
        {
            moveIds.Add(move.Id);
        }

        await AssociateLearnableMovesAsync(dragonite.Id, moveIds.ToArray());
        var myPokemon = await CreateMyPokemonAsync(dragonite.Id, 55, 140, 160, moveIds[0], moveIds[1], moveIds[2], moveIds[3]);

        var response = await _client.PutAsJsonAsync(
            $"/api/v1/my-pokemons/{myPokemon.Id}",
            new UpdateMyPokemonRequestContract(56, 145, 165, moveIds));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var payload = await JsonDocument.ParseAsync(responseStream);
        Assert.True(payload.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("equippedMoveIds", out _));
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
    public async Task DeletePokemonSpecies_Should_Remove_Species_And_Keep_Mvp_Roster_Coherent()
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

        var beforeDeleteResponse = await _client.GetAsync("/api/v1/pokemons?page=1&pageSize=20");
        var beforeDeletePayload = await beforeDeleteResponse.Content.ReadFromJsonAsync<PokemonSpeciesCatalogContract>();
        Assert.NotNull(beforeDeletePayload);

        var charizard = Assert.Single(beforeDeletePayload.Items, item => item.Name == "Charizard");

        var deleteResponse = await _client.DeleteAsync($"/api/v1/pokemons/{charizard.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var listResponse = await _client.GetAsync("/api/v1/pokemons?page=1&pageSize=20");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var listPayload = await listResponse.Content.ReadFromJsonAsync<PokemonSpeciesCatalogContract>();
        Assert.NotNull(listPayload);
        Assert.Equal(9, listPayload.TotalCount);
        Assert.DoesNotContain(listPayload.Items, item => item.Id == charizard.Id);
        Assert.Contains(listPayload.Items, item => item.Name == "Blastoise");

        var detailResponse = await _client.GetAsync($"/api/v1/pokemons/{charizard.Id}");
        Assert.Equal(HttpStatusCode.NotFound, detailResponse.StatusCode);
    }

    [Fact]
    public async Task DeletePokemonSpecies_Should_Return_Validation_Problem_When_Dependencies_Exist()
    {
        var species = await CreateSpeciesAsync("Venusaur", ["Grass", "Poison"], 80, 82, 83, 100, 100, 80);

        using var blockingClient = CreateClientWithDeletionDependencies(
            "Pokemon species cannot be deleted because it is referenced by 'MyPokemon'.");

        var response = await blockingClient.DeleteAsync($"/api/v1/pokemons/{species.Id}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var payload = await JsonDocument.ParseAsync(responseStream);
        Assert.True(payload.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("dependencies", out var dependencyErrors));
        Assert.Contains(
            dependencyErrors.EnumerateArray().Select(static item => item.GetString()),
            message => message == "Pokemon species cannot be deleted because it is referenced by 'MyPokemon'.");

        var detailResponse = await _client.GetAsync($"/api/v1/pokemons/{species.Id}");
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
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

    [Fact]
    public async Task CreatePokemonMove_Should_Return_Conflict_For_Duplicate_Name()
    {
        var request = new CreatePokemonMoveRequestContract("Surf", "Water", "Special", 90);

        var firstResponse = await _client.PostAsJsonAsync("/api/v1/moves", request);
        var secondResponse = await _client.PostAsJsonAsync("/api/v1/moves", request);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task CreatePokemonMove_Should_Return_Validation_Problem_For_Invalid_Type()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/moves",
            new CreatePokemonMoveRequestContract("Thunderbolt", "Light", "Special", 90));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var payload = await JsonDocument.ParseAsync(responseStream);
        Assert.True(payload.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("type", out _));
    }

    [Fact]
    public async Task CreatePokemonMove_Should_Return_Validation_Problem_For_Invalid_Category()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/moves",
            new CreatePokemonMoveRequestContract("Thunderbolt", "Electric", "Support", 90));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var payload = await JsonDocument.ParseAsync(responseStream);
        Assert.True(payload.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("category", out _));
    }

    [Fact]
    public async Task CreatePokemonMove_Should_Return_Validation_Problem_For_Inconsistent_Power()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/moves",
            new CreatePokemonMoveRequestContract("Protect", "Normal", "Status", 10));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var payload = await JsonDocument.ParseAsync(responseStream);
        Assert.True(payload.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("power", out _));
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

    private async Task<PokemonMoveContract> CreateMoveAsync(
        string name,
        string type,
        string category,
        int power)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/moves",
            new CreatePokemonMoveRequestContract(name, type, category, power));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<PokemonMoveContract>();
        Assert.NotNull(payload);
        return payload;
    }

    private async Task AssociateLearnableMovesAsync(Guid speciesId, params Guid[] moveIds)
    {
        var response = await _client.PutAsJsonAsync(
            $"/api/v1/pokemons/{speciesId}/learnable-moves",
            new UpdatePokemonLearnableMovesRequestContract(moveIds, []));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<MyPokemonContract> CreateMyPokemonAsync(
        Guid pokemonSpeciesId,
        int level,
        int currentHealthPoints,
        int totalHealthPoints,
        params Guid[] equippedMoveIds)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/my-pokemons",
            new CreateMyPokemonRequestContract(
                pokemonSpeciesId,
                level,
                currentHealthPoints,
                totalHealthPoints,
                equippedMoveIds));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<MyPokemonContract>();
        Assert.NotNull(payload);
        return payload;
    }

    private async Task<MyPokemonContract> UpdateMyPokemonAsync(
        Guid myPokemonId,
        int level,
        int currentHealthPoints,
        int totalHealthPoints,
        params Guid[] equippedMoveIds)
    {
        var response = await _client.PutAsJsonAsync(
            $"/api/v1/my-pokemons/{myPokemonId}",
            new UpdateMyPokemonRequestContract(
                level,
                currentHealthPoints,
                totalHealthPoints,
                equippedMoveIds));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<MyPokemonContract>();
        Assert.NotNull(payload);
        return payload;
    }

    private HttpClient CreateClientWithDeletionDependencies(params string[] blockingReasons) =>
        _factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IPokemonSpeciesDeletionDependencyChecker>();
                services.AddScoped<IPokemonSpeciesDeletionDependencyChecker>(
                    _ => new StubPokemonSpeciesDeletionDependencyChecker(blockingReasons));
            }))
            .CreateClient();

    private HttpClient CreateClientWithMoveDeletionDependencies(params string[] blockingReasons) =>
        _factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IPokemonMoveDeletionDependencyChecker>();
                services.AddScoped<IPokemonMoveDeletionDependencyChecker>(
                    _ => new StubPokemonMoveDeletionDependencyChecker(blockingReasons));
            }))
            .CreateClient();

    private sealed class StubPokemonSpeciesDeletionDependencyChecker(IReadOnlyCollection<string> blockingReasons)
        : IPokemonSpeciesDeletionDependencyChecker
    {
        public Task<IReadOnlyCollection<string>> GetBlockingReasonsAsync(
            Guid pokemonSpeciesId,
            CancellationToken cancellationToken) =>
            Task.FromResult(blockingReasons);
    }

    private sealed class StubPokemonMoveDeletionDependencyChecker(IReadOnlyCollection<string> blockingReasons)
        : IPokemonMoveDeletionDependencyChecker
    {
        public Task<IReadOnlyCollection<string>> GetBlockingReasonsAsync(
            Guid pokemonMoveId,
            CancellationToken cancellationToken) =>
            Task.FromResult(blockingReasons);
    }
}
