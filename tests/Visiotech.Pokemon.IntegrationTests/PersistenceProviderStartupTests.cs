using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Visiotech.Pokemon.Contracts;
using Visiotech.Pokemon.Infrastructure;

namespace Visiotech.Pokemon.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class PersistenceProviderStartupTests
{
    [Fact]
    public async Task CreatePokemonSpecies_Should_Work_With_InMemory_When_Configured_In_Development()
    {
        var speciesName = $"Localmon-{Guid.NewGuid():N}";

        await using var factory = new ConfigurableWebApplicationFactory(
            "Development",
            new Dictionary<string, string?>
            {
                ["Persistence:Provider"] = "InMemory",
                ["Persistence:InMemoryDatabaseName"] = $"pokemon2-dev-{Guid.NewGuid():N}",
                ["Seed:ApplyMvpRoster"] = "false",
                ["Observability:SeqUrl"] = null
            });

        using var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/pokemons",
            new CreatePokemonSpeciesRequestContract(
                speciesName,
                ["Fire"],
                new PokemonBaseStatsContract(78, 84, 78, 109, 85, 100)));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var listResponse = await client.GetAsync("/api/v1/pokemons");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var payload = await listResponse.Content.ReadFromJsonAsync<PokemonSpeciesCatalogContract>();
        Assert.NotNull(payload);
        Assert.Contains(payload.Items, item => item.Name == speciesName);
    }

    [Fact]
    public void AddInfrastructure_Should_Fail_When_InMemory_Is_Configured_In_Production()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Persistence:Provider"] = "InMemory",
                    ["Persistence:InMemoryDatabaseName"] = $"pokemon2-prod-{Guid.NewGuid():N}",
                    ["Seed:ApplyMvpRoster"] = "false"
                })
            .Build();

        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddInfrastructure(
                configuration,
                new StubHostEnvironment("Production")));

        Assert.Contains("only allowed in Development", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class StubHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "Visiotech.Pokemon.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new PhysicalFileProvider(AppContext.BaseDirectory);
    }

    private sealed class ConfigurableWebApplicationFactory(
        string environmentName,
        IReadOnlyDictionary<string, string?> settings)
        : WebApplicationFactory<Program>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.UseEnvironment(environmentName);
            return base.CreateHost(builder);
        }

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(settings);
            });
        }
    }
}
