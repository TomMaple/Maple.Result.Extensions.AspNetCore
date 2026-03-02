using System;
using Maple.Result.Extensions.AspNetCore.Mappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        var statusCode = ErrorCategoryMapper.GetStatusCode(error.Category);
        var errorObject = ErrorMapper.Map(error, statusCode);

        return statusCode switch
        {
            StatusCodes.Status400BadRequest => controller.BadRequest(errorObject),
            StatusCodes.Status401Unauthorized => controller.Unauthorized(errorObject),
            StatusCodes.Status404NotFound => controller.NotFound(errorObject),
            StatusCodes.Status409Conflict => controller.Conflict(errorObject),
            StatusCodes.Status422UnprocessableEntity => controller.UnprocessableEntity(errorObject),
            _ => controller.StatusCode(errorObject.Status!.Value, errorObject)
        };
    }
}