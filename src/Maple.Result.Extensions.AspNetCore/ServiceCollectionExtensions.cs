using Maple.Result.Extensions.AspNetCore.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Maple.Result.Extensions.AspNetCore;

/// <summary>
///     The collection of extension methods for configuring result mappings in the service collection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Configures result mappings in the service collection using the provided options.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configureOptions">An action to configure the result mapping options.</param>
    public static IServiceCollection ConfigureResultMapping(this IServiceCollection services, Action<ResultMappingOptions>? configureOptions)
    {
        services.Configure<ResultMappingOptions>(options => { configureOptions?.Invoke(options); });

        return services;
    }
}
