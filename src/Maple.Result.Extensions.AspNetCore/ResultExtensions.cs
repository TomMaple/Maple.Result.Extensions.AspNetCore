using Maple.Result.Extensions.AspNetCore.Configuration;
using Maple.Result.Extensions.AspNetCore.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

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
    /// <param name="customMapping">
    ///     An optional custom mapping function used to convert an <see cref="Error" /> to
    ///     an <see cref="IActionResult" />. It is evaluated before the mappings registered with
    ///     the <see cref="ServiceCollectionExtensions.ConfigureResultMapping" /> method, and it is used
    ///     when it returns a non-<see langword="null" /> value.
    /// </param>
    /// <returns>An <see cref="IActionResult" /> representing the <see cref="Result" />.</returns>
    public static IActionResult ToActionResult(this ControllerBase controller, Result result,
        Func<Error, ControllerBase, IActionResult?>? customMapping = null)
    {
        return result.ToActionResult(controller, customMapping);
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
    /// <param name="customMapping">
    ///     An optional custom mapping function used to convert an <see cref="Error" /> to
    ///     an <see cref="IActionResult" />. It is evaluated before the mappings registered with
    ///     the <see cref="ServiceCollectionExtensions.ConfigureResultMapping" /> method, and it is used
    ///     when it returns a non-<see langword="null" /> value.
    /// </param>
    /// <returns>An <see cref="IActionResult" /> representing the <see cref="Result{T}" />.</returns>
    public static IActionResult ToActionResult<T>(this ControllerBase controller, Result<T> result,
        Func<Error, ControllerBase, IActionResult?>? customMapping = null)
    {
        return result.ToActionResult(controller, customMapping);
    }

    /// <summary>
    ///     Creates an <see cref="IActionResult" /> from a <see cref="Result" /> instance.
    /// </summary>
    /// <param name="result">The <see cref="Result" /> to convert.</param>
    /// <param name="controller">The controller instance.</param>
    /// <param name="customMapping">
    ///     An optional custom mapping function used to convert an <see cref="Error" /> to
    ///     an <see cref="IActionResult" />. It is evaluated before the mappings registered with
    ///     the <see cref="ServiceCollectionExtensions.ConfigureResultMapping" /> method, and it is used
    ///     when it returns a non-<see langword="null" /> value.
    /// </param>
    /// <returns>An <see cref="IActionResult" /> representing the <see cref="Result" />.</returns>
    public static IActionResult ToActionResult(this Result result, ControllerBase controller,
        Func<Error, ControllerBase, IActionResult?>? customMapping = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(controller);

        return result.Match<IActionResult>(
            controller.NoContent,
            error => error.ToActionResult(controller, customMapping));
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
    /// <param name="customMapping">
    ///     An optional custom mapping function used to convert an <see cref="Error" /> to
    ///     an <see cref="IActionResult" />. It is evaluated before the mappings registered with
    ///     the <see cref="ServiceCollectionExtensions.ConfigureResultMapping" /> method, and it is used
    ///     when it returns a non-<see langword="null" /> value.
    /// </param>
    /// <returns>An <see cref="IActionResult" /> representing the <see cref="Result{T}" />.</returns>
    public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller,
        Func<Error, ControllerBase, IActionResult?>? customMapping = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(controller);

        return result.Match(
            value => value is null
                ? controller.NoContent()
                : controller.Ok(result.Value),
            error => error.ToActionResult(controller, customMapping));
    }

    #region helper methods

    private static IActionResult ToActionResult(this Error error, ControllerBase controller,
        Func<Error, ControllerBase, IActionResult?>? customMapping)
    {
        // Check for the custom mapping passed to the method first
        var mappingResult = customMapping?.Invoke(error, controller);
        if (mappingResult is not null)
            return mappingResult;

        // Then check for the registered custom mappings
        mappingResult = TryMapUsingResultMappingOptions(error, controller);
        if (mappingResult is not null)
            return mappingResult;

        // Fallback to default mapping
        var statusCode = ErrorCategoryMapper.GetStatusCode(error.Category);
        var extensions = ErrorMapper.MapExtensions(error);

        return controller.Problem(error.Detail, error.InstanceUri, statusCode, error.Title, error.TypeUri, extensions);
    }

    private static IActionResult? TryMapUsingResultMappingOptions(Error error, ControllerBase controller)
    {
        var options = controller.HttpContext.RequestServices.GetService<IOptions<ResultMappingOptions>>();
        var mappings = options?.Value.Mappings;
        if (mappings is not { Count: > 0 })
            return null;

        foreach (var mapping in mappings)
        {
            var mappingResult = mapping?.Invoke(error, controller);
            if (mappingResult is not null)
                return mappingResult;
        }

        return null;
    }

    #endregion
}
