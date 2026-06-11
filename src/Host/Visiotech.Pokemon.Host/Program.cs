using DotNetEnv;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using Visiotech.Pokemon.Api;
using Visiotech.Pokemon.Application;
using Visiotech.Pokemon.Infrastructure;
using Visiotech.Pokemon.Infrastructure.Persistence;

LoadEnvironmentVariablesFromDotEnv();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    {
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "Visiotech.Pokemon.Api")
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
            .WriteTo.Console();

        var seqUrl = context.Configuration["Observability:SeqUrl"];
        if (!string.IsNullOrWhiteSpace(seqUrl))
        {
            loggerConfiguration.WriteTo.Seq(seqUrl);
        }
    });

    builder.Services.AddApi();
    builder.Services.AddOpenApi("v1");
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

    var app = builder.Build();

    Log.Information(
        "Starting {Application} in {Environment}. Seq configured: {SeqConfigured}",
        "Visiotech.Pokemon.Api",
        app.Environment.EnvironmentName,
        !string.IsNullOrWhiteSpace(app.Configuration["Observability:SeqUrl"]));

    await app.Services.GetRequiredService<DatabaseInitializer>().InitializeAsync(app.Lifetime.ApplicationStopping);

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.GetLevel = static (httpContext, _, exception) =>
        {
            if (exception is not null || httpContext.Response.StatusCode >= StatusCodes.Status500InternalServerError)
            {
                return LogEventLevel.Error;
            }

            if (httpContext.Response.StatusCode >= StatusCodes.Status400BadRequest)
            {
                return LogEventLevel.Warning;
            }

            return LogEventLevel.Information;
        };
        options.EnrichDiagnosticContext = static (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("TraceId", httpContext.TraceIdentifier);
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? string.Empty);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("RequestProtocol", httpContext.Request.Protocol);
            diagnosticContext.Set("EndpointName", httpContext.GetEndpoint()?.DisplayName ?? "unknown");
            diagnosticContext.Set("HasQueryString", httpContext.Request.QueryString.HasValue);
        };
    });

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
}
catch (Exception exception)
{
    Log.Fatal(exception, "The API host terminated unexpectedly during startup.");
}
finally
{
    await Log.CloseAndFlushAsync();
}

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
