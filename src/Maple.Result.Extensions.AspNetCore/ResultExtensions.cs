using Maple.Result.Extensions.AspNetCore.Configuration;
using Maple.Result.Extensions.AspNetCore.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Maple.Result.Extensions.AspNetCore;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this ControllerBase controller, Result result)
    {
        return result.ToActionResult(controller);
    }

    public static IActionResult ToActionResult<T>(this ControllerBase controller, Result<T> result)
    {
        return result.ToActionResult(controller);
    }

    public static IActionResult ToActionResult(this Result result, ControllerBase controller)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(controller);

        return result.Match<IActionResult>(
            controller.NoContent,
            error => error.ToActionResult(controller));
    }

    public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(controller);

        return result.Match(
            value => value is null
                ? controller.NoContent()
                : controller.Ok(result.Value),
            error => error.ToActionResult(controller));
    }

    private static IActionResult ToActionResult(this Error error, ControllerBase controller)
    {
        // Check for custom mappings first
        var options = controller.HttpContext.RequestServices.GetService<IOptions<ResultMappingOptions>>();
        var mappings = options?.Value.Mappings;
        if (mappings is { Count: > 0 })
        {
            foreach (var mapping in mappings)
            {
                var mappingResult = mapping?.Invoke(error, controller);
                if (mappingResult is not null)
                    return mappingResult;
            }
        }

        // Fallback to default mapping
        var statusCode = ErrorCategoryMapper.GetStatusCode(error.Category);
        var extensions = ErrorMapper.MapExtensions(error);

        return controller.Problem(error.Detail, error.InstanceUri, statusCode, error.Title, error.TypeUri, extensions);
    }
}