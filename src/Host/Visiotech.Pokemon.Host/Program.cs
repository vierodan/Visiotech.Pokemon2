using DotNetEnv;
using Scalar.AspNetCore;
using Visiotech.Pokemon.Api;
using Visiotech.Pokemon.Application;
using Visiotech.Pokemon.Infrastructure;

LoadEnvironmentVariablesFromDotEnv();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApi();
builder.Services.AddOpenApi("v1");
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/openapi/{documentName}.json");
    app.MapScalarApiReference("/scalar", options =>
    {
        options.WithTitle("Visiotech Pokemon API")
            .AddDocument("v1", "Visiotech Pokemon API v1", "/openapi/v1.json")
            .DisableAgent();
    });
}

app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();
app.MapApi();

app.Run();

static void LoadEnvironmentVariablesFromDotEnv()
{
    var envFilePath = FindFileInAncestors(AppContext.BaseDirectory, ".env");
    if (envFilePath is null)
    {
        return;
    }

    Env.NoClobber().Load(envFilePath);
}

static string? FindFileInAncestors(string startingDirectory, string fileName)
{
    var directory = new DirectoryInfo(startingDirectory);

    while (directory is not null)
    {
        var candidatePath = Path.Combine(directory.FullName, fileName);
        if (File.Exists(candidatePath))
        {
            return candidatePath;
        }

        directory = directory.Parent;
    }

    return null;
}

public partial class Program;
