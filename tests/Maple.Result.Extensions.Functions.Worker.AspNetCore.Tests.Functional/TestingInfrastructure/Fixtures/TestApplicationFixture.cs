using Maple.Result.Extensions.Functions.Worker.AspNetCore.Tests.Functional.TestingInfrastructure.Application;
using Microsoft.AspNetCore.Http;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Maple.Result.Extensions.Functions.Worker.AspNetCore.Tests.Functional.TestingInfrastructure.Fixtures;

public class TestApplicationFixture : IAsyncLifetime
{
    private TestApplicationFactory _application = null!;

    internal HttpClient Client { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        _application = await TestApplicationFactory.CreateAsync(CustomizeProblemDetails);
        Client = _application.CreateClient();
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();

        await _application.DisposeAsync();
    }

    private protected virtual Action<ProblemDetailsContext>? CustomizeProblemDetails => null;
}
