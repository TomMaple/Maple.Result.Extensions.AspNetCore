using Maple.Result.Extensions.AspNetCore.Tests.Functional.TestingInfrastructure.Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Maple.Result.Extensions.AspNetCore.Tests.Functional.TestingInfrastructure.Application.Controllers;

[ApiController]
[Route("results")]
public sealed class ResultsController : ControllerBase
{
    #region success

    [HttpGet("success")]
    public IActionResult GetSuccess()
    {
        var result = Result.Success();

        return result.ToActionResult(this);
    }

    [HttpGet("success/value")]
    public IActionResult GetSuccessWithValue()
    {
        var result = Result<TestValue>.FromValue(new TestValue(13, "Test value"));

        return result.ToActionResult(this);
    }

    [HttpGet("success/null-value")]
    public IActionResult GetSuccessWithNullValue()
    {
        var result = Result<TestValue?>.FromValue(null);

        return result.ToActionResult(this);
    }

    #endregion

    #region ErrorCategory.Validation

    [HttpGet("validation")]
    public IActionResult GetValidation()
    {
        var result = Result.FromError(Error.Validation(
            ErrorUri.Tag("tag:test.com,2026:validation"),
            "Validation title",
            "Validation detail.",
            ErrorUri.Locator("https://test.com/instances/validation")));

        return result.ToActionResult(this);
    }

    #endregion

    #region ErrorCategory.Unauthenticated

    [HttpGet("unauthenticated")]
    public IActionResult GetUnauthenticated()
    {
        var result = Result.FromError(Error.Unauthenticated(
            ErrorUri.Tag("tag:test.com,2026:unauthenticated"),
            "Unauthenticated title",
            "Unauthenticated detail.",
            ErrorUri.Locator("https://test.com/instances/unauthenticated")));

        return result.ToActionResult(this);
    }

    #endregion

    #region ErrorCategory.Unauthorized

    [HttpGet("unauthorized")]
    public IActionResult GetUnauthorized()
    {
        var result = Result.FromError(Error.Unauthorized(
            ErrorUri.Tag("tag:test.com,2026:unauthorized"),
            "Unauthorized title",
            "Unauthorized detail.",
            ErrorUri.Locator("https://test.com/instances/unauthorized")));

        return result.ToActionResult(this);
    }

    #endregion

    #region ErrorCategory.NotFound

    [HttpGet("not-found")]
    public IActionResult GetNotFound()
    {
        var result = Result.FromError(Error.NotFound(
            ErrorUri.Tag("tag:test.com,2026:not-found"),
            "Not found title",
            "Not found detail.",
            ErrorUri.Locator("https://test.com/instances/not-found")));

        return result.ToActionResult(this);
    }

    #endregion

    #region ErrorCategory.Timeout

    [HttpGet("timeout")]
    public IActionResult GetTimeout()
    {
        var result = Result.FromError(Error.Timeout(
            ErrorUri.Tag("tag:test.com,2026:timeout"),
            "Timeout title",
            "Timeout detail.",
            ErrorUri.Locator("https://test.com/instances/timeout")));

        return result.ToActionResult(this);
    }

    #endregion

    #region ErrorCategory.Conflict

    [HttpGet("conflict")]
    public IActionResult GetConflict()
    {
        var result = Result.FromError(Error.Conflict(
            ErrorUri.Tag("tag:test.com,2026:conflict"),
            "Conflict title",
            "Conflict detail.",
            ErrorUri.Locator("https://test.com/instances/conflict")));

        return result.ToActionResult(this);
    }

    #endregion

    #region ErrorCategory.Critical

    [HttpGet("critical")]
    public IActionResult GetCritical()
    {
        var result = Result.FromError(Error.Critical(
            ErrorUri.Tag("tag:test.com,2026:critical"),
            "Critical title",
            "Critical detail.",
            ErrorUri.Locator("https://test.com/instances/critical")));

        return result.ToActionResult(this);
    }

    #endregion

    #region ErrorCategory.NotImplemented

    [HttpGet("not-implemented")]
    public IActionResult GetNotImplemented()
    {
        var result = Result.FromError(Error.NotImplemented(
            ErrorUri.Tag("tag:test.com,2026:not-implemented"),
            "Not implemented title",
            "Not implemented detail.",
            ErrorUri.Locator("https://test.com/instances/not-implemented")));

        return result.ToActionResult(this);
    }

    #endregion

    #region ErrorCategory.Unavailable

    [HttpGet("unavailable")]
    public IActionResult GetUnavailable()
    {
        var result = Result.FromError(Error.Unavailable(
            ErrorUri.Tag("tag:test.com,2026:unavailable"),
            "Unavailable title",
            "Unavailable detail.",
            ErrorUri.Locator("https://test.com/instances/unavailable")));

        return result.ToActionResult(this);
    }

    #endregion

    #region ErrorCategory.Failure

    [HttpGet("failure/all-properties")]
    public IActionResult GetFailureWithAllProperties()
    {
        var error = Error.Failure(
                ErrorUri.Tag("tag:test.com,2026:failure"),
                "Failure title",
                "Failure detail.",
                ErrorUri.Locator("https://test.com/instances/failure"),
                "errors.failure.detail",
                ("key1", "value1"), ("key2", 123))
            .AddDetail(
                "#/property1",
                "Property 1 failure detail.",
                "errors.failure.property1",
                ("pk1", "pv1"))
            .AddDetail(
                "#/property2",
                "Property 2 failure detail.");

        var result = Result.FromError(error);

        return result.ToActionResult(this);
    }

    [HttpGet("failure/required-only")]
    public IActionResult GetFailureWithRequiredPropertiesOnly()
    {
        var result = Result.FromError(Error.Failure(
            ErrorUri.Tag("tag:test.com,2026:failure"),
            "Failure title"));

        return result.ToActionResult(this);
    }

    [HttpGet("failure/detail-and-instance")]
    public IActionResult GetFailureWithDetailAndInstance()
    {
        var result = Result.FromError(Error.Failure(
            ErrorUri.Tag("tag:test.com,2026:failure"),
            "Failure title",
            "Failure detail.",
            ErrorUri.Locator("https://test.com/instances/failure")));

        return result.ToActionResult(this);
    }

    [HttpGet("failure/templated-detail")]
    public IActionResult GetFailureWithTemplatedDetail()
    {
        var result = Result.FromError(Error.Failure(
            ErrorUri.Tag("tag:test.com,2026:failure"),
            "Failure title",
            "Failure detail.",
            ErrorUri.Locator("https://test.com/instances/failure"),
            "errors.failure.detail",
            ("key1", "value1"), ("key2", 123)));

        return result.ToActionResult(this);
    }

    #endregion

    #region ErrorCategory.Failure, ToActionResult overloads

    [HttpGet("failure/overloads/result-controller")]
    public IActionResult GetFailureFromResultWithControllerArgument()
    {
        var result = Result.FromError(CreateFailureError());

        return result.ToActionResult(this);
    }

    [HttpGet("failure/overloads/controller-result")]
    public IActionResult GetFailureFromControllerWithResultArgument()
    {
        var result = Result.FromError(CreateFailureError());

        return this.ToActionResult(result);
    }

    [HttpGet("failure/overloads/generic-result-controller")]
    public IActionResult GetFailureFromGenericResultWithControllerArgument()
    {
        var result = Result<TestValue>.FromError(CreateFailureError());

        return result.ToActionResult(this);
    }

    [HttpGet("failure/overloads/generic-controller-result")]
    public IActionResult GetFailureFromControllerWithGenericResultArgument()
    {
        var result = Result<TestValue>.FromError(CreateFailureError());

        return this.ToActionResult(result);
    }

    #endregion

    #region custom mappings passed to the method

    [HttpGet("per-call/matching")]
    public IActionResult GetFailureWithMatchingMapping()
    {
        var result = Result.FromError(CreateFailureError());

        return result.ToActionResult(this, MapFailureToPaymentRequired);
    }

    [HttpGet("per-call/controller-result")]
    public IActionResult GetFailureWithMatchingMappingFromController()
    {
        var result = Result.FromError(CreateFailureError());

        return this.ToActionResult(result, MapFailureToPaymentRequired);
    }

    [HttpGet("per-call/generic-result-controller")]
    public IActionResult GetFailureWithMatchingMappingFromGenericResult()
    {
        var result = Result<TestValue>.FromError(CreateFailureError());

        return result.ToActionResult(this, MapFailureToPaymentRequired);
    }

    [HttpGet("per-call/generic-controller-result")]
    public IActionResult GetFailureWithMatchingMappingFromControllerWithGenericResult()
    {
        var result = Result<TestValue>.FromError(CreateFailureError());

        return this.ToActionResult(result, MapFailureToPaymentRequired);
    }

    [HttpGet("per-call/not-matching")]
    public IActionResult GetFailureWithNotMatchingMapping()
    {
        var result = Result.FromError(CreateFailureError());

        return result.ToActionResult(this, MapConflictToPaymentRequired);
    }

    [HttpGet("per-call/precedence")]
    public IActionResult GetConflictWithMatchingMapping()
    {
        var result = Result.FromError(Error.Conflict(
            ErrorUri.Tag("tag:test.com,2026:conflict"),
            "Conflict title",
            "Conflict detail.",
            ErrorUri.Locator("https://test.com/instances/conflict")));

        return result.ToActionResult(this, MapConflictToPaymentRequired);
    }

    #endregion

    #region helper methods

    private static Error CreateFailureError()
    {
        return Error.Failure(
            ErrorUri.Tag("tag:test.com,2026:failure"),
            "Failure title",
            "Failure detail.",
            ErrorUri.Locator("https://test.com/instances/failure"));
    }

    private static IActionResult? MapFailureToPaymentRequired(Error error, ControllerBase controller)
    {
        return error.Category == ErrorCategory.Failure
            ? controller.StatusCode(StatusCodes.Status402PaymentRequired, new TestValue(11, error.Title))
            : null;
    }

    private static IActionResult? MapConflictToPaymentRequired(Error error, ControllerBase controller)
    {
        return error.Category == ErrorCategory.Conflict
            ? controller.StatusCode(StatusCodes.Status402PaymentRequired, new TestValue(22, error.Title))
            : null;
    }

    #endregion
}
