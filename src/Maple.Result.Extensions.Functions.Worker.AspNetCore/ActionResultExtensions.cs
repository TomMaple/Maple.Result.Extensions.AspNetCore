using Maple.Result.Extensions.AspNetCore.Mappers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;

namespace Maple.Result.Extensions.Functions.Worker.AspNetCore;

/// <summary>
///     The collection of extension methods for converting <see cref="Result" /> and <see cref="Result{T}" /> to
///     <see cref="ActionResult" /> in Azure Functions (isolated worker model) with ASP.NET Core integration.
/// </summary>
public static class ActionResultExtensions
{
    #region Result

    /// <summary>
    ///     Creates an <see cref="ActionResult" /> from a <see cref="Result" /> instance.
    /// </summary>
    /// <param name="result">The <see cref="Result" /> to convert.</param>
    /// <param name="customSuccessMapping">
    ///     An optional mapping function used to convert a successful <see cref="Result" /> to
    ///     an <see cref="ActionResult" />. When it is not provided, the 204 No Content response is returned.
    /// </param>
    /// <param name="customErrorMapping">
    ///     An optional custom mapping function used to convert an <see cref="Error" /> to
    ///     an <see cref="ActionResult" />. The default mapping is used when it returns <see langword="null" />.
    /// </param>
    /// <returns>An <see cref="ActionResult" /> representing the <see cref="Result" />.</returns>
    public static ActionResult ToActionResult(this Result result,
        Func<ActionResult>? customSuccessMapping = null,
        Func<Error, ActionResult?>? customErrorMapping = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.Match<ActionResult>(
            () => customSuccessMapping is null ? new NoContentResult() : customSuccessMapping(),
            error => error.ToActionResult(customErrorMapping));
    }

    /// <summary>
    ///     Creates an <see cref="ActionResult" /> from a <see cref="Result" /> instance, using the given
    ///     HTTP status code when it is successful.
    /// </summary>
    /// <param name="result">The <see cref="Result" /> to convert.</param>
    /// <param name="successStatusCode">The HTTP status code returned when the <see cref="Result" /> is successful.</param>
    /// <param name="customErrorMapping">
    ///     An optional custom mapping function used to convert an <see cref="Error" /> to
    ///     an <see cref="ActionResult" />. The default mapping is used when it returns <see langword="null" />.
    /// </param>
    /// <returns>An <see cref="ActionResult" /> representing the <see cref="Result" />.</returns>
    public static ActionResult ToActionResult(this Result result, HttpStatusCode successStatusCode,
        Func<Error, ActionResult?>? customErrorMapping = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.Match<ActionResult>(
            () => new StatusCodeResult((int)successStatusCode),
            error => error.ToActionResult(customErrorMapping));
    }

    #endregion

    #region Result<T>

    /// <summary>
    ///     Creates an <see cref="ActionResult" /> from a <see cref="Result{T}" /> instance.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="Result{T}" /> value.</typeparam>
    /// <param name="result">The <see cref="Result{T}" /> to convert.</param>
    /// <param name="customSuccessMapping">
    ///     An optional mapping function used to convert the value of a successful <see cref="Result{T}" /> to
    ///     an <see cref="ActionResult" />. It is also invoked when the value is <see langword="null" />.
    ///     When it is not provided, the 200 OK response with the value is returned, or the 204 No Content response
    ///     when the value is <see langword="null" />.
    /// </param>
    /// <param name="customErrorMapping">
    ///     An optional custom mapping function used to convert an <see cref="Error" /> to
    ///     an <see cref="ActionResult" />. The default mapping is used when it returns <see langword="null" />.
    /// </param>
    /// <returns>An <see cref="ActionResult" /> representing the <see cref="Result{T}" />.</returns>
    public static ActionResult ToActionResult<T>(this Result<T> result,
        Func<T, ActionResult>? customSuccessMapping = null,
        Func<Error, ActionResult?>? customErrorMapping = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.Match(
            value => customSuccessMapping is null
                ? CreateDefaultActionResult(value)
                : customSuccessMapping(value),
            error => error.ToActionResult(customErrorMapping));
    }

    /// <summary>
    ///     Creates an <see cref="ActionResult" /> from a <see cref="Result{T}" /> instance, using the given
    ///     HTTP status code when it is successful.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="Result{T}" /> value.</typeparam>
    /// <param name="result">The <see cref="Result{T}" /> to convert.</param>
    /// <param name="successStatusCode">The HTTP status code returned when the <see cref="Result{T}" /> is successful.</param>
    /// <param name="successNoResponseStatusCode">
    ///     An optional HTTP status code returned when the <see cref="Result{T}" /> is successful, but its value
    ///     is <see langword="null" />. When it is not provided, the <paramref name="successStatusCode" /> is used.
    /// </param>
    /// <param name="customErrorMapping">
    ///     An optional custom mapping function used to convert an <see cref="Error" /> to
    ///     an <see cref="ActionResult" />. The default mapping is used when it returns <see langword="null" />.
    /// </param>
    /// <returns>An <see cref="ActionResult" /> representing the <see cref="Result{T}" />.</returns>
    public static ActionResult ToActionResult<T>(this Result<T> result, HttpStatusCode successStatusCode,
        HttpStatusCode? successNoResponseStatusCode = null, Func<Error, ActionResult?>? customErrorMapping = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.Match(
            value => value is null
                ? new StatusCodeResult((int)(successNoResponseStatusCode ?? successStatusCode))
                : new ObjectResult(value) { StatusCode = (int)successStatusCode },
            error => error.ToActionResult(customErrorMapping));
    }

    #endregion

    #region helper methods

    private static ActionResult CreateDefaultActionResult<T>(T value)
    {
        return value is null
            ? new NoContentResult()
            : new OkObjectResult(value);
    }

    private static ActionResult ToActionResult(this Error error, Func<Error, ActionResult?>? customErrorMapping)
    {
        // Check for the custom mapping passed to the method first
        var mappingResult = customErrorMapping?.Invoke(error);
        if (mappingResult is not null)
            return mappingResult;

        // Fallback to default mapping
        var statusCode = ErrorCategoryMapper.GetStatusCode(error.Category);
        var extensions = ErrorMapper.MapExtensions(error);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Type = error.TypeUri,
            Title = error.Title,
            Detail = error.Detail,
            Instance = error.InstanceUri
        };

        if (extensions is not null)
            problemDetails.Extensions = extensions;

        return new ProblemDetailsResult(problemDetails);
    }

    #endregion
}
