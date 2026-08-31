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
    public ActionResult GetSuccess()
    {
        var result = Result.Success();

        return result.ToActionResult(this);
    }

    [HttpGet("success/value")]
    public ActionResult GetSuccessWithValue()
    {
        var result = Result<TestValue>.FromValue(new TestValue(13, "Test value"));

        return result.ToActionResult(this);
    }

    [HttpGet("success/null-value")]
    public ActionResult GetSuccessWithNullValue()
    {
        var result = Result<TestValue?>.FromValue(null);

        return result.ToActionResult(this);
    }

    #endregion

    #region ErrorCategory.Validation

    [HttpGet("validation")]
    public ActionResult GetValidation()
    {
        var result = Result.FromError(Error.Validation(
            ErrorUri.Tag("tag:test.com,2026:validation"),
            "Validation title",
            "Validation detail.",
            ErrorUri.Locator("https://test.com/instances/validation")));

        return result.ToActionResult(this);
    }

    [HttpGet("validation/details")]
    public ActionResult GetValidationWithDetailVariants()
    {
        var error = Error.Validation(
            ErrorUri.Tag("tag:test.com,2026:validation"),
            "Validation title",
            "Validation detail.",
            ErrorUri.Locator("https://test.com/instances/validation"));

        error.AddDetail("/age", "must be a positive integer", "test.age.mustBePositive", ("min", (object)0));

        // No pointer and no templated message, so both are omitted from the serialized detail.
        error.AddDetail(null, "must be provided");

        // A template id without parameters: AddDetail yields an empty parameter collection,
        // which the mapper normalizes away, so "params" is omitted.
        error.AddDetail("/name", "must not be empty", "test.name.required");

        return Result.FromError(error).ToActionResult(this);
    }

    #endregion

    #region ErrorCategory.Unauthenticated

    [HttpGet("unauthenticated")]
    public ActionResult GetUnauthenticated()
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
    public ActionResult GetUnauthorized()
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
    public ActionResult GetNotFound()
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
    public ActionResult GetTimeout()
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
    public ActionResult GetConflict()
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
    public ActionResult GetCritical()
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
    public ActionResult GetNotImplemented()
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
    public ActionResult GetUnavailable()
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
    public ActionResult GetFailureWithAllProperties()
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
    public ActionResult GetFailureWithRequiredPropertiesOnly()
    {
        var result = Result.FromError(Error.Failure(
            ErrorUri.Tag("tag:test.com,2026:failure"),
            "Failure title"));

        return result.ToActionResult(this);
    }

    [HttpGet("failure/detail-and-instance")]
    public ActionResult GetFailureWithDetailAndInstance()
    {
        var result = Result.FromError(Error.Failure(
            ErrorUri.Tag("tag:test.com,2026:failure"),
            "Failure title",
            "Failure detail.",
            ErrorUri.Locator("https://test.com/instances/failure")));

        return result.ToActionResult(this);
    }

    [HttpGet("failure/templated-detail")]
    public ActionResult GetFailureWithTemplatedDetail()
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
    public ActionResult GetFailureFromResultWithControllerArgument()
    {
        var result = Result.FromError(CreateFailureError());

        return result.ToActionResult(this);
    }

    [HttpGet("failure/overloads/controller-result")]
    public ActionResult GetFailureFromControllerWithResultArgument()
    {
        var result = Result.FromError(CreateFailureError());

        return this.ToActionResult(result);
    }

    [HttpGet("failure/overloads/generic-result-controller")]
    public ActionResult GetFailureFromGenericResultWithControllerArgument()
    {
        var result = Result<TestValue>.FromError(CreateFailureError());

        return result.ToActionResult(this);
    }

    [HttpGet("failure/overloads/generic-controller-result")]
    public ActionResult GetFailureFromControllerWithGenericResultArgument()
    {
        var result = Result<TestValue>.FromError(CreateFailureError());

        return this.ToActionResult(result);
    }

    #endregion

    #region custom mappings passed to the method

    [HttpGet("per-call/matching")]
    public ActionResult GetFailureWithMatchingMapping()
    {
        var result = Result.FromError(CreateFailureError());

        return result.ToActionResult(this, MapFailureToPaymentRequired);
    }

    [HttpGet("per-call/controller-result")]
    public ActionResult GetFailureWithMatchingMappingFromController()
    {
        var result = Result.FromError(CreateFailureError());

        return this.ToActionResult(result, MapFailureToPaymentRequired);
    }

    [HttpGet("per-call/generic-result-controller")]
    public ActionResult GetFailureWithMatchingMappingFromGenericResult()
    {
        var result = Result<TestValue>.FromError(CreateFailureError());

        return result.ToActionResult(this, MapFailureToPaymentRequired);
    }

    [HttpGet("per-call/generic-controller-result")]
    public ActionResult GetFailureWithMatchingMappingFromControllerWithGenericResult()
    {
        var result = Result<TestValue>.FromError(CreateFailureError());

        return this.ToActionResult(result, MapFailureToPaymentRequired);
    }

    [HttpGet("per-call/not-matching")]
    public ActionResult GetFailureWithNotMatchingMapping()
    {
        var result = Result.FromError(CreateFailureError());

        return result.ToActionResult(this, MapConflictToPaymentRequired);
    }

    [HttpGet("per-call/precedence")]
    public ActionResult GetConflictWithMatchingMapping()
    {
        var result = Result.FromError(Error.Conflict(
            ErrorUri.Tag("tag:test.com,2026:conflict"),
            "Conflict title",
            "Conflict detail.",
            ErrorUri.Locator("https://test.com/instances/conflict")));

        return result.ToActionResult(this, MapConflictToPaymentRequired);
    }

    #endregion

    #region success status code

    [HttpGet("status-code/success")]
    public ActionResult GetSuccessWithStatusCode()
    {
        var result = Result.Success();

        return result.ToActionResult(this, StatusCodes.Status202Accepted);
    }

    [HttpGet("status-code/controller-result")]
    public ActionResult GetSuccessWithStatusCodeFromController()
    {
        var result = Result.Success();

        return this.ToActionResult(result, StatusCodes.Status207MultiStatus);
    }

    [HttpGet("status-code/success/value")]
    public ActionResult GetSuccessWithValueAndStatusCode()
    {
        var result = Result<TestValue>.FromValue(new TestValue(13, "Test value"));

        return result.ToActionResult(this, StatusCodes.Status201Created);
    }

    [HttpGet("status-code/generic-controller-result")]
    public ActionResult GetSuccessWithValueAndStatusCodeFromController()
    {
        var result = Result<TestValue>.FromValue(new TestValue(13, "Test value"));

        return this.ToActionResult(result, StatusCodes.Status203NonAuthoritative);
    }

    [HttpGet("status-code/success/null-value")]
    public ActionResult GetSuccessWithNullValueAndStatusCode()
    {
        var result = Result<TestValue?>.FromValue(null);

        return result.ToActionResult(this, StatusCodes.Status226IMUsed);
    }

    [HttpGet("status-code/success/null-value-status-code")]
    public ActionResult GetSuccessWithNullValueAndNullValueStatusCode()
    {
        var result = Result<TestValue?>.FromValue(null);

        return result.ToActionResult(this, StatusCodes.Status203NonAuthoritative, StatusCodes.Status205ResetContent);
    }

    [HttpGet("status-code/error")]
    public ActionResult GetFailureWithStatusCode()
    {
        var result = Result.FromError(CreateFailureError());

        return result.ToActionResult(this, StatusCodes.Status202Accepted);
    }

    [HttpGet("status-code/error/custom-mapping")]
    public ActionResult GetFailureWithStatusCodeAndMapping()
    {
        var result = Result.FromError(CreateFailureError());

        return result.ToActionResult(this, StatusCodes.Status207MultiStatus, MapFailureToPaymentRequired);
    }

    #endregion

    #region success mapping

    [HttpGet("success-mapping/success")]
    public ActionResult GetSuccessWithSuccessMapping()
    {
        var result = Result.Success();

        return result.ToActionResult(this, MapSuccess);
    }

    [HttpGet("success-mapping/controller-result")]
    public ActionResult GetSuccessWithSuccessMappingFromController()
    {
        var result = Result.Success();

        return this.ToActionResult(result, MapSuccess);
    }

    [HttpGet("success-mapping/success/value")]
    public ActionResult GetSuccessWithValueAndSuccessMapping()
    {
        var result = Result<TestValue>.FromValue(new TestValue(13, "Test value"));

        return result.ToActionResult(this, MapSuccessValue);
    }

    [HttpGet("success-mapping/generic-controller-result")]
    public ActionResult GetSuccessWithValueAndSuccessMappingFromController()
    {
        var result = Result<TestValue>.FromValue(new TestValue(13, "Test value"));

        return this.ToActionResult(result, MapSuccessValue);
    }

    [HttpGet("success-mapping/success/null-value")]
    public ActionResult GetSuccessWithNullValueAndSuccessMapping()
    {
        var result = Result<TestValue?>.FromValue(null);

        return result.ToActionResult(this, MapSuccessValue);
    }

    [HttpGet("success-mapping/error")]
    public ActionResult GetFailureWithSuccessMapping()
    {
        var result = Result.FromError(CreateFailureError());

        return result.ToActionResult(this, MapSuccess);
    }

    [HttpGet("success-mapping/error/custom-mapping")]
    public ActionResult GetFailureWithSuccessMappingAndCustomMapping()
    {
        var result = Result.FromError(CreateFailureError());

        return result.ToActionResult(this, MapSuccess, MapFailureToPaymentRequired);
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

    private static ActionResult? MapFailureToPaymentRequired(Error error, ControllerBase controller)
    {
        return error.Category == ErrorCategory.Failure
            ? controller.StatusCode(StatusCodes.Status402PaymentRequired, new TestValue(11, error.Title))
            : null;
    }

    private static ActionResult? MapConflictToPaymentRequired(Error error, ControllerBase controller)
    {
        return error.Category == ErrorCategory.Conflict
            ? controller.StatusCode(StatusCodes.Status402PaymentRequired, new TestValue(22, error.Title))
            : null;
    }

    private static ActionResult MapSuccess(ControllerBase controller)
    {
        return controller.StatusCode(StatusCodes.Status202Accepted, new TestValue(31, "Mapped success"));
    }

    private static ActionResult MapSuccessValue(TestValue? value, ControllerBase controller)
    {
        return value is null
            ? controller.StatusCode(StatusCodes.Status205ResetContent)
            : controller.StatusCode(StatusCodes.Status203NonAuthoritative, new TestValue(value.Id * 2, value.Name));
    }

    #endregion
}
