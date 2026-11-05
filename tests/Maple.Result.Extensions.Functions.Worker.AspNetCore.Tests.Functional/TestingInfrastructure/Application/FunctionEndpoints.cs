using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using System;
using System.Net;

namespace Maple.Result.Extensions.Functions.Worker.AspNetCore.Tests.Functional.TestingInfrastructure.Application;

/// <summary>
///     Maps the endpoints playing the role of the functions. They execute the returned <see cref="IActionResult" />
///     the same way as the Azure Functions (isolated worker model) with ASP.NET Core integration:
///     with an empty <see cref="ActionDescriptor" />.
/// </summary>
internal static class FunctionEndpoints
{
    #region consts

    private const string FailureTypeUri = "tag:test.com,2026:failure";
    private const string FailureInstanceUri = "https://test.com/instances/failure";

    #endregion

    internal static void MapFunctionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        Map(endpoints, "success", () => Result.Success().ToActionResult());
        Map(endpoints, "status-code", () => Result.Success().ToActionResult(HttpStatusCode.Accepted));
        Map(endpoints, "success-mapping", () => Result.Success().ToActionResult(MapSuccess));
        Map(endpoints, "error", () => Result.FromError(CreateFailureError()).ToActionResult());
        Map(endpoints, "error/custom-mapping",
            () => Result.FromError(CreateFailureError()).ToActionResult(customErrorMapping: MapFailureToPaymentRequired));
        Map(endpoints, "error/templated", () => Result.FromError(CreateTemplatedError()).ToActionResult());

        Map(endpoints, "value", () => Result<TestValue>.FromValue(CreateValue()).ToActionResult());
        Map(endpoints, "value/null", () => Result<TestValue?>.FromValue(null).ToActionResult());
        Map(endpoints, "value/status-code",
            () => Result<TestValue>.FromValue(CreateValue()).ToActionResult(HttpStatusCode.Created));
        Map(endpoints, "value/success-mapping",
            () => Result<TestValue>.FromValue(CreateValue()).ToActionResult(MapValue));
        Map(endpoints, "value/error", () => Result<TestValue>.FromError(CreateFailureError()).ToActionResult());
    }

    #region helper methods

    private static void Map(IEndpointRouteBuilder endpoints, string pattern, Func<IActionResult> function)
    {
        endpoints.MapGet(pattern, context => function()
            .ExecuteResultAsync(new ActionContext(context, context.GetRouteData(), new ActionDescriptor())));
    }

    private static TestValue CreateValue()
    {
        return new TestValue(13, "Test value");
    }

    private static Error CreateFailureError()
    {
        return Error.Failure(
            ErrorUri.Tag(FailureTypeUri),
            "Failure title",
            "Failure detail.",
            ErrorUri.Locator(FailureInstanceUri));
    }

    private static Error CreateTemplatedError()
    {
        return Error.Failure(
            ErrorUri.Tag(FailureTypeUri),
            "Failure title",
            "Failure detail.",
            ErrorUri.Locator(FailureInstanceUri),
            "errors.failure.detail",
            ("key1", "value1"));
    }

    private static ActionResult MapSuccess()
    {
        return new ObjectResult(new TestValue(31, "Mapped success")) { StatusCode = StatusCodes.Status202Accepted };
    }

    private static ActionResult MapValue(TestValue value)
    {
        return new ObjectResult(new TestValue(value.Id * 2, value.Name))
        {
            StatusCode = StatusCodes.Status203NonAuthoritative
        };
    }

    private static ActionResult? MapFailureToPaymentRequired(Error error)
    {
        return error.Category == ErrorCategory.Failure
            ? new ObjectResult(new TestValue(11, error.Title)) { StatusCode = StatusCodes.Status402PaymentRequired }
            : null;
    }

    #endregion
}
