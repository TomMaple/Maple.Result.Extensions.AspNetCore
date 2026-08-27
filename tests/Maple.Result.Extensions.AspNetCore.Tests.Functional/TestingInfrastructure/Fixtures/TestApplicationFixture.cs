using Maple.Result.Extensions.AspNetCore.Configuration;
using Maple.Result.Extensions.AspNetCore.Tests.Functional.TestingInfrastructure.Application;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Maple.Result.Extensions.AspNetCore.Tests.Functional.TestingInfrastructure.Fixtures;

public class TestApplicationFixture : IAsyncLifetime
{
    private TestApplicationFactory _application = null!;

    internal HttpClient Client { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        _application = await TestApplicationFactory.CreateAsync(ConfigureResultMapping);
        Client = _application.CreateClient();
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();

        await _application.DisposeAsync();
    }

    private protected virtual Action<ResultMappingOptions>? ConfigureResultMapping => null;
}
