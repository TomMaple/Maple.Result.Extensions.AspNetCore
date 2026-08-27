using Maple.Result.Extensions.AspNetCore.Tests.Functional.TestingInfrastructure.Application.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Maple.Result.Extensions.AspNetCore.Tests.Functional.TestingInfrastructure.Application;

// The alias is declared inside the namespace so that it takes precedence over the Maple.Result.IResult type.
using IResult = Microsoft.AspNetCore.Http.IResult;

/// <summary>
///     Maps a separate Minimal API endpoint for every scenario covered by the functional tests.
/// </summary>
internal static class MinimalApiEndpoints
{
    internal static void MapMinimalApiEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("minimal");

        MapSuccessEndpoints(group);
        MapErrorEndpoints(group);
        MapSuccessStatusCodeEndpoints(group);
        MapSuccessMappingEndpoints(group);
    }

    #region helper methods

    private static void MapSuccessEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("success", () => Result.Success().ToMinimalApiResult());

        endpoints.MapGet("success/value",
            () => Result<TestValue>.FromValue(new TestValue(13, "Test value")).ToMinimalApiResult());

        endpoints.MapGet("success/null-value",
            () => Result<TestValue?>.FromValue(null).ToMinimalApiResult());
    }

    private static void MapErrorEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("error", () => Result.FromError(CreateFailureError()).ToMinimalApiResult());

        endpoints.MapGet("error/not-found", () => Result.FromError(Error.NotFound(
            ErrorUri.Tag("tag:test.com,2026:not-found"),
            "Not found title",
            "Not found detail.",
            ErrorUri.Locator("https://test.com/instances/not-found"))).ToMinimalApiResult());

        endpoints.MapGet("error/custom-mapping",
            () => Result.FromError(CreateFailureError()).ToMinimalApiResult(MapFailureToPaymentRequired));

        endpoints.MapGet("error/custom-mapping-not-matching",
            () => Result.FromError(CreateFailureError()).ToMinimalApiResult(MapConflictToPaymentRequired));
    }

    private static void MapSuccessStatusCodeEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("status-code/success",
            () => Result.Success().ToMinimalApiResult(StatusCodes.Status202Accepted));

        endpoints.MapGet("status-code/success/value",
            () => Result<TestValue>.FromValue(new TestValue(13, "Test value"))
                .ToMinimalApiResult(StatusCodes.Status201Created));

        endpoints.MapGet("status-code/success/null-value",
            () => Result<TestValue?>.FromValue(null).ToMinimalApiResult(StatusCodes.Status226IMUsed));

        endpoints.MapGet("status-code/success/no-response-status-code",
            () => Result<TestValue?>.FromValue(null)
                .ToMinimalApiResult(StatusCodes.Status203NonAuthoritative, StatusCodes.Status205ResetContent));

        endpoints.MapGet("status-code/error",
            () => Result.FromError(CreateFailureError()).ToMinimalApiResult(StatusCodes.Status202Accepted));
    }

    private static void MapSuccessMappingEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("success-mapping/success", () => Result.Success().ToMinimalApiResult(MapSuccess));

        endpoints.MapGet("success-mapping/success/value",
            () => Result<TestValue>.FromValue(new TestValue(13, "Test value")).ToMinimalApiResult(MapSuccessValue));

        endpoints.MapGet("success-mapping/success/null-value",
            () => Result<TestValue?>.FromValue(null).ToMinimalApiResult(MapSuccessValue));

        endpoints.MapGet("success-mapping/error",
            () => Result.FromError(CreateFailureError()).ToMinimalApiResult(MapSuccess));

        endpoints.MapGet("success-mapping/error/custom-mapping",
            () => Result.FromError(CreateFailureError())
                .ToMinimalApiResult(MapSuccess, MapFailureToPaymentRequired));
    }

    private static Error CreateFailureError()
    {
        return Error.Failure(
            ErrorUri.Tag("tag:test.com,2026:failure"),
            "Failure title",
            "Failure detail.",
            ErrorUri.Locator("https://test.com/instances/failure"));
    }

    private static IResult MapSuccess()
    {
        return Results.Json(new TestValue(31, "Mapped success"), statusCode: StatusCodes.Status202Accepted);
    }

    private static IResult MapSuccessValue(TestValue? value)
    {
        return value is null
            ? Results.StatusCode(StatusCodes.Status205ResetContent)
            : Results.Json(new TestValue(value.Id * 2, value.Name), statusCode: StatusCodes.Status203NonAuthoritative);
    }

    private static IResult? MapFailureToPaymentRequired(Error error)
    {
        return error.Category == ErrorCategory.Failure
            ? Results.Json(new TestValue(11, error.Title), statusCode: StatusCodes.Status402PaymentRequired)
            : null;
    }

    private static IResult? MapConflictToPaymentRequired(Error error)
    {
        return error.Category == ErrorCategory.Conflict
            ? Results.Json(new TestValue(22, error.Title), statusCode: StatusCodes.Status402PaymentRequired)
            : null;
    }

    #endregion
}
