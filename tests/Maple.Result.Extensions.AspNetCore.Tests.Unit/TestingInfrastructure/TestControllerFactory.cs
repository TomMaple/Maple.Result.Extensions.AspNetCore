using Maple.Result.Extensions.AspNetCore.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Maple.Result.Extensions.AspNetCore.Tests.Unit.TestingInfrastructure;

internal static class TestControllerFactory
{
    /// <summary>
    ///     Creates a controller backed by a service provider, which the extension methods need in order to
    ///     resolve the <see cref="ProblemDetailsFactory" /> and the registered <see cref="ResultMappingOptions" />.
    /// </summary>
    internal static ControllerBase Create(Action<ResultMappingOptions>? configureResultMapping = null)
    {
        var services = new ServiceCollection();
        services.AddControllers();

        if (configureResultMapping is not null)
            services.ConfigureResultMapping(configureResultMapping);

        var serviceProvider = services.BuildServiceProvider();

        return new TestController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = serviceProvider }
            }
        };
    }
}
