namespace Visiotech.Pokemon.IntegrationTests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class IntegrationTestCollection
{
    public const string Name = "Integration host tests";
}
