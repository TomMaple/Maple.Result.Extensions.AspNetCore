using System;
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
        var errorObject = ErrorDetails.FromError(error, statusCode);

        return controller.StatusCode(errorObject.Status!.Value, errorObject);
    }
}