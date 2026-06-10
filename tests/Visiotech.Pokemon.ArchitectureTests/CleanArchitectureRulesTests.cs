using NetArchTest.Rules;
using Visiotech.Pokemon.Api;
using Visiotech.Pokemon.Application;
using Visiotech.Pokemon.Contracts;
using Visiotech.Pokemon.Domain;
using Visiotech.Pokemon.Domain.Abstractions;
using Visiotech.Pokemon.Host;
using Visiotech.Pokemon.Infrastructure;

namespace Visiotech.Pokemon.ArchitectureTests;

public sealed class CleanArchitectureRulesTests
{
    private static readonly string ApplicationNamespace = typeof(Visiotech.Pokemon.Application.AssemblyReference).Namespace!;
    private static readonly string ContractsNamespace = typeof(Visiotech.Pokemon.Contracts.AssemblyReference).Namespace!;
    private static readonly string InfrastructureNamespace = typeof(Visiotech.Pokemon.Infrastructure.AssemblyReference).Namespace!;
    private static readonly string ApiNamespace = typeof(Visiotech.Pokemon.Api.AssemblyReference).Namespace!;
    private static readonly string HostNamespace = typeof(Visiotech.Pokemon.Host.AssemblyReference).Namespace!;

    [Fact]
    public void Domain_Should_Not_Depend_On_Outer_Layers()
    {
        var result = Types
            .InAssembly(typeof(Visiotech.Pokemon.Domain.AssemblyReference).Assembly)
            .Should()
            .NotHaveDependencyOnAny(ApplicationNamespace, ContractsNamespace, InfrastructureNamespace, ApiNamespace, HostNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, Format(result.FailingTypeNames));
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Api_Or_Infrastructure()
    {
        var result = Types
            .InAssembly(typeof(Visiotech.Pokemon.Application.AssemblyReference).Assembly)
            .Should()
            .NotHaveDependencyOnAny(ContractsNamespace, InfrastructureNamespace, ApiNamespace, HostNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, Format(result.FailingTypeNames));
    }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Api()
    {
        var result = Types
            .InAssembly(typeof(Visiotech.Pokemon.Infrastructure.AssemblyReference).Assembly)
            .Should()
            .NotHaveDependencyOnAny(ContractsNamespace, ApiNamespace, HostNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, Format(result.FailingTypeNames));
    }

    [Fact]
    public void Api_Endpoints_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types
            .InAssembly(typeof(Visiotech.Pokemon.Api.AssemblyReference).Assembly)
            .That()
            .ResideInNamespace($"{ApiNamespace}.Endpoints")
            .Should()
            .NotHaveDependencyOnAny(InfrastructureNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, Format(result.FailingTypeNames));
    }

    [Fact]
    public void Application_Repository_Abstractions_Should_Be_Interfaces()
    {
        var result = Types
            .InAssembly(typeof(Visiotech.Pokemon.Application.AssemblyReference).Assembly)
            .That()
            .ResideInNamespace($"{ApplicationNamespace}.Abstractions.Persistence")
            .Should()
            .BeInterfaces()
            .GetResult();

        Assert.True(result.IsSuccessful, Format(result.FailingTypeNames));
    }

    [Fact]
    public void Api_Should_Not_Depend_On_Host()
    {
        var result = Types
            .InAssembly(typeof(Visiotech.Pokemon.Api.AssemblyReference).Assembly)
            .Should()
            .NotHaveDependencyOnAny(typeof(Visiotech.Pokemon.Domain.AssemblyReference).Namespace!, InfrastructureNamespace, HostNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, Format(result.FailingTypeNames));
    }

    [Fact]
    public void Contracts_Should_Not_Depend_On_Application_Domain_Infrastructure_Api_Or_Host()
    {
        var result = Types
            .InAssembly(typeof(Visiotech.Pokemon.Contracts.AssemblyReference).Assembly)
            .Should()
            .NotHaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, ApiNamespace, HostNamespace, typeof(Visiotech.Pokemon.Domain.AssemblyReference).Namespace!)
            .GetResult();

        Assert.True(result.IsSuccessful, Format(result.FailingTypeNames));
    }

    [Fact]
    public void Contracts_Public_Types_Should_End_With_Contract()
    {
        var contractAssembly = typeof(Visiotech.Pokemon.Contracts.AssemblyReference).Assembly;

        var invalidTypes = contractAssembly
            .GetExportedTypes()
            .Where(type => type.Name != nameof(Visiotech.Pokemon.Contracts.AssemblyReference))
            .Where(type => !type.Name.EndsWith("Contract", StringComparison.Ordinal))
            .Select(type => type.FullName ?? type.Name);

        Assert.True(!invalidTypes.Any(), Format(invalidTypes));
    }

    [Fact]
    public void Domain_ValueObjects_Should_Inherit_From_ValueObject_Base_Type()
    {
        var domainAssembly = typeof(Visiotech.Pokemon.Domain.AssemblyReference).Assembly;

        var valueObjectNames = new[]
        {
            "Name",
            "Level",
            "BaseStats",
            "Move",
            "Ability"
        };

        var invalidTypes = domainAssembly
            .GetTypes()
            .Where(type => valueObjectNames.Contains(type.Name, StringComparer.Ordinal))
            .Where(type => type.BaseType != typeof(ValueObject))
            .Select(type => type.FullName ?? type.Name);

        Assert.True(!invalidTypes.Any(), Format(invalidTypes));
    }

    private static string Format(IEnumerable<string>? failingTypeNames)
    {
        var failures = failingTypeNames?.ToArray() ?? [];
        return failures.Length == 0
            ? "No failing types."
            : $"Failing types: {string.Join(", ", failures)}";
    }
}
