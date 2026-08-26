using Maple.Result.Extensions.AspNetCore.Configuration;
using Maple.Result.Extensions.AspNetCore.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Maple.Result.Extensions.AspNetCore;

/// <summary>
///     The collection of extension methods for converting <see cref="Result" /> and <see cref="Result{T}" /> to <see cref="IActionResult" />.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    ///     Creates an <see cref="IActionResult" /> from a <see cref="Result" /> instance.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="result">The <see cref="Result" /> to convert.</param>
    /// <returns>An <see cref="IActionResult" /> representing the <see cref="Result" />.</returns>
    public static IActionResult ToActionResult(this ControllerBase controller, Result result)
    {
        return result.ToActionResult(controller);
    }

    /// <summary>
    ///     Creates an <see cref="IActionResult" /> from a <see cref="Result{T}" /> instance.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the <see cref="Result{T}" /> value used to determine whether to execute the passed
    ///     function. If successful, this is also the type of the parameter passed to that function.
    /// </typeparam>
    /// <param name="controller">The controller instance.</param>
    /// <param name="result">The <see cref="Result{T}" /> to convert.</param>
    /// <returns>An <see cref="IActionResult" /> representing the <see cref="Result{T}" />.</returns>
    public static IActionResult ToActionResult<T>(this ControllerBase controller, Result<T> result)
    {
        return result.ToActionResult(controller);
    }

    /// <summary>
    ///     Creates an <see cref="IActionResult" /> from a <see cref="Result" /> instance.
    /// </summary>
    /// <param name="result">The <see cref="Result" /> to convert.</param>
    /// <param name="controller">The controller instance.</param>
    /// <returns>An <see cref="IActionResult" /> representing the <see cref="Result" />.</returns>
    public static IActionResult ToActionResult(this Result result, ControllerBase controller)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(controller);

        return result.Match<IActionResult>(
            controller.NoContent,
            error => error.ToActionResult(controller));
    }

    /// <summary>
    ///     Creates an <see cref="IActionResult" /> from a <see cref="Result{T}" /> instance.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the <see cref="Result{T}" /> value used to determine whether to execute the passed
    ///     function. If successful, this is also the type of the parameter passed to that function.
    /// </typeparam>
    /// <param name="result">The <see cref="Result{T}" /> to convert.</param>
    /// <param name="controller">The controller instance.</param>
    /// <returns>An <see cref="IActionResult" /> representing the <see cref="Result{T}" />.</returns>
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

    #region helper methods

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

    #endregion
}
