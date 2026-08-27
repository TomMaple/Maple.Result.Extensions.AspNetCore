using Maple.Result.Extensions.AspNetCore.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Maple.Result.Extensions.AspNetCore.Tests.Functional.TestingInfrastructure.Application;

/// <summary>
///     Starts an in-memory ASP.NET Core application hosting the <see cref="Controllers.ResultsController" />
///     and the Minimal API endpoints mapped by the <see cref="MinimalApiEndpoints" /> class.
/// </summary>
internal sealed class TestApplicationFactory : IAsyncDisposable
{
    private readonly WebApplication _application;

    private TestApplicationFactory(WebApplication application)
    {
        _application = application;
    }

    internal static async Task<TestApplicationFactory> CreateAsync(
        Action<ResultMappingOptions>? configureResultMapping = null)
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();

        builder.Services
            .AddControllers()
            .AddApplicationPart(typeof(TestApplicationFactory).Assembly);

        if (configureResultMapping is not null)
            builder.Services.ConfigureResultMapping(configureResultMapping);

        var application = builder.Build();
        application.MapControllers();
        application.MapMinimalApiEndpoints();

        await application.StartAsync();

        return new TestApplicationFactory(application);
    }

    internal HttpClient CreateClient()
    {
        return _application.GetTestClient();
    }

    public async ValueTask DisposeAsync()
    {
        await _application.StopAsync();
        await _application.DisposeAsync();
    }
}
