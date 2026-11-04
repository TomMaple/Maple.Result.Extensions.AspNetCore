using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Maple.Result.Extensions.Functions.Worker.AspNetCore.Tests.Functional.TestingInfrastructure.Application;

/// <summary>
///     Starts an in-memory ASP.NET Core application hosting the <see cref="FunctionEndpoints" />. It registers MVC with
///     the <c>AddMvc</c> method and maps no controllers, the same way as the Azure Functions (isolated worker model)
///     with ASP.NET Core integration.
/// </summary>
internal sealed class TestApplicationFactory : IAsyncDisposable
{
    private readonly WebApplication _application;

    private TestApplicationFactory(WebApplication application)
    {
        _application = application;
    }

    internal static async Task<TestApplicationFactory> CreateAsync(
        Action<ProblemDetailsContext>? customizeProblemDetails = null)
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();

        builder.Services.AddMvc();

        if (customizeProblemDetails is not null)
            builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = customizeProblemDetails);

        var application = builder.Build();
        application.MapFunctionEndpoints();

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
