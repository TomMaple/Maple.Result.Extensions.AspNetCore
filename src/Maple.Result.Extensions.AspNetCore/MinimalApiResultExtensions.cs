using Maple.Result.Extensions.AspNetCore.Mappers;
using Microsoft.AspNetCore.Http;
using System;

namespace Maple.Result.Extensions.AspNetCore;

// The alias is declared inside the namespace so that it takes precedence over the Maple.Result.IResult type.
using IResult = Microsoft.AspNetCore.Http.IResult;

/// <summary>
///     The collection of extension methods for converting <see cref="Result" /> and <see cref="Result{T}" />
///     to the Minimal API <see cref="IResult" />.
/// </summary>
public static class MinimalApiResultExtensions
{
    #region (Result)

    /// <summary>
    ///     Creates an <see cref="IResult" /> from a <see cref="Result" /> instance.
    /// </summary>
    /// <param name="result">The <see cref="Result" /> to convert.</param>
    /// <param name="customMapping">
    ///     An optional custom mapping function used to convert an <see cref="Error" /> to an <see cref="IResult" />.
    ///     It is used when it returns a non-<see langword="null" /> value.
    /// </param>
    /// <returns>An <see cref="IResult" /> representing the <see cref="Result" />.</returns>
    public static IResult ToMinimalApiResult(this Result result, Func<Error, IResult?>? customMapping = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.Match(
            Results.NoContent,
            error => error.ToMinimalApiResult(customMapping));
    }

    /// <summary>
    ///     Creates an <see cref="IResult" /> from a <see cref="Result" /> instance, using the given
    ///     HTTP status code when it is successful.
    /// </summary>
    /// <param name="result">The <see cref="Result" /> to convert.</param>
    /// <param name="successStatusCode">The HTTP status code returned when the <see cref="Result" /> is successful.</param>
    /// <param name="customMapping">
    ///     An optional custom mapping function used to convert an <see cref="Error" /> to an <see cref="IResult" />.
    ///     It is used when it returns a non-<see langword="null" /> value.
    /// </param>
    /// <returns>An <see cref="IResult" /> representing the <see cref="Result" />.</returns>
    public static IResult ToMinimalApiResult(this Result result, int successStatusCode,
        Func<Error, IResult?>? customMapping = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.Match(
            () => Results.StatusCode(successStatusCode),
            error => error.ToMinimalApiResult(customMapping));
    }

    /// <summary>
    ///     Creates an <see cref="IResult" /> from a <see cref="Result" /> instance, using the given
    ///     mapping function when it is successful.
    /// </summary>
    /// <param name="result">The <see cref="Result" /> to convert.</param>
    /// <param name="successMapping">
    ///     The mapping function used to convert a successful <see cref="Result" /> to an <see cref="IResult" />.
    /// </param>
    /// <param name="customMapping">
    ///     An optional custom mapping function used to convert an <see cref="Error" /> to an <see cref="IResult" />.
    ///     It is used when it returns a non-<see langword="null" /> value.
    /// </param>
    /// <returns>An <see cref="IResult" /> representing the <see cref="Result" />.</returns>
    public static IResult ToMinimalApiResult(this Result result, Func<IResult> successMapping,
        Func<Error, IResult?>? customMapping = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(successMapping);

        return result.Match(
            successMapping,
            error => error.ToMinimalApiResult(customMapping));
    }

    #endregion

    #region (Result<T>)

    /// <summary>
    ///     Creates an <see cref="IResult" /> from a <see cref="Result{T}" /> instance.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the <see cref="Result{T}" /> value used to determine whether to execute
    ///     the passed function. If successful, this is also the type of the parameter passed to that function.
    /// </typeparam>
    /// <param name="result">The <see cref="Result{T}" /> to convert.</param>
    /// <param name="customMapping">
    ///     An optional custom mapping function used to convert an <see cref="Error" /> to an <see cref="IResult" />.
    ///     It is used when it returns a non-<see langword="null" /> value.
    /// </param>
    /// <returns>An <see cref="IResult" /> representing the <see cref="Result{T}" />.</returns>
    public static IResult ToMinimalApiResult<T>(this Result<T> result, Func<Error, IResult?>? customMapping = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.Match(
            value => value is null
                ? Results.NoContent()
                : Results.Ok(result.Value),
            error => error.ToMinimalApiResult(customMapping));
    }

    /// <summary>
    ///     Creates an <see cref="IResult" /> from a <see cref="Result{T}" /> instance, using the given
    ///     HTTP status code when it is successful.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the <see cref="Result{T}" /> value used to determine whether to execute
    ///     the passed function. If successful, this is also the type of the parameter passed to that function.
    /// </typeparam>
    /// <param name="result">The <see cref="Result{T}" /> to convert.</param>
    /// <param name="successStatusCode">The HTTP status code returned when the <see cref="Result{T}" /> is successful.</param>
    /// <param name="successNoResponseStatusCode">
    ///     An optional HTTP status code returned when the <see cref="Result{T}" /> is successful, but its value
    ///     is <see langword="null" />. When it is not provided, the <paramref name="successStatusCode" /> is used.
    /// </param>
    /// <param name="customMapping">
    ///     An optional custom mapping function used to convert an <see cref="Error" /> to an <see cref="IResult" />.
    ///     It is used when it returns a non-<see langword="null" /> value.
    /// </param>
    /// <returns>An <see cref="IResult" /> representing the <see cref="Result{T}" />.</returns>
    public static IResult ToMinimalApiResult<T>(this Result<T> result, int successStatusCode,
        int? successNoResponseStatusCode = null, Func<Error, IResult?>? customMapping = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.Match(
            value => value is null
                ? Results.StatusCode(successNoResponseStatusCode ?? successStatusCode)
                : Results.Json(result.Value, statusCode: successStatusCode),
            error => error.ToMinimalApiResult(customMapping));
    }

    /// <summary>
    ///     Creates an <see cref="IResult" /> from a <see cref="Result{T}" /> instance, using the given
    ///     mapping function when it is successful.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the <see cref="Result{T}" /> value used to determine whether to execute
    ///     the passed function. If successful, this is also the type of the parameter passed to that function.
    /// </typeparam>
    /// <param name="result">The <see cref="Result{T}" /> to convert.</param>
    /// <param name="successMapping">
    ///     The mapping function used to convert the value of a successful <see cref="Result{T}" /> to
    ///     an <see cref="IResult" />. It is also invoked when the value is <see langword="null" />.
    /// </param>
    /// <param name="customMapping">
    ///     An optional custom mapping function used to convert an <see cref="Error" /> to an <see cref="IResult" />.
    ///     It is used when it returns a non-<see langword="null" /> value.
    /// </param>
    /// <returns>An <see cref="IResult" /> representing the <see cref="Result{T}" />.</returns>
    public static IResult ToMinimalApiResult<T>(this Result<T> result, Func<T, IResult> successMapping,
        Func<Error, IResult?>? customMapping = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(successMapping);

        return result.Match(
            successMapping,
            error => error.ToMinimalApiResult(customMapping));
    }

    #endregion

    #region helper methods

    private static IResult ToMinimalApiResult(this Error error, Func<Error, IResult?>? customMapping)
    {
        // Check for the custom mapping passed to the method first
        var mappingResult = customMapping?.Invoke(error);
        if (mappingResult is not null)
            return mappingResult;

        // Fallback to default mapping
        var statusCode = ErrorCategoryMapper.GetStatusCode(error.Category);
        var extensions = ErrorMapper.MapExtensions(error);

        return Results.Problem(error.Detail, error.InstanceUri, statusCode, error.Title, error.TypeUri, extensions);
    }

    #endregion
}
