using Maple.Result.Extensions.Functions.Worker.AspNetCore.Tests.Unit.TestingInfrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;

namespace Maple.Result.Extensions.Functions.Worker.AspNetCore.Tests.Unit;

// The aliases are declared inside the namespace so that they take precedence over the Maple.Result types
// of the same name.
using ErrorDetail = Maple.Result.Extensions.AspNetCore.ViewModels.ErrorDetail;
using TemplatedMessage = Maple.Result.Extensions.AspNetCore.ViewModels.TemplatedMessage;

public class ActionResultExtensionsTests
{
    #region consts

    private const string FailureTypeUri = "tag:test.com,2026:failure";
    private const string FailureInstanceUri = "https://test.com/instances/failure";

    #endregion

    #region Result

    [Fact]
    public void ToActionResult_SuccessfulResult_ReturnsNoContent()
    {
        // Arrange
        var sut = Result.Success();

        // Act
        var result = sut.ToActionResult();

        // Assert
        result.ShouldBeOfType<NoContentResult>().StatusCode.ShouldBe(StatusCodes.Status204NoContent);
    }

    [Fact]
    public void ToActionResult_Error_ReturnsProblemDetailsMappedFromTheError()
    {
        // Arrange
        var sut = Result.FromError(CreateFailureError());

        // Act
        var result = sut.ToActionResult();

        // Assert
        var objectResult = result.ShouldBeAssignableTo<ObjectResult>().ShouldNotBeNull();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);

        var problem = objectResult.Value.ShouldBeOfType<ProblemDetails>();
        problem.Status.ShouldBe(StatusCodes.Status422UnprocessableEntity);
        problem.Type.ShouldBe(FailureTypeUri);
        problem.Title.ShouldBe("Failure title");
        problem.Detail.ShouldBe("Failure detail.");
        problem.Instance.ShouldBe(FailureInstanceUri);
        problem.Extensions.ShouldNotContainKey("errors");
        problem.Extensions.ShouldNotContainKey("detailTemplated");
    }

    [Theory]
    [InlineData(ErrorCategory.Validation, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorCategory.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorCategory.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(ErrorCategory.Critical, StatusCodes.Status500InternalServerError)]
    public void ToActionResult_ErrorOfTheGivenCategory_ReturnsExpectedStatusCode(
        ErrorCategory category, int expectedStatusCode)
    {
        // Arrange
        var sut = Result.FromError(CreateError(category));

        // Act
        var result = sut.ToActionResult();

        // Assert
        var objectResult = result.ShouldBeAssignableTo<ObjectResult>().ShouldNotBeNull();
        objectResult.StatusCode.ShouldBe(expectedStatusCode);
        objectResult.Value.ShouldBeOfType<ProblemDetails>().Status.ShouldBe(expectedStatusCode);
    }

    [Fact]
    public void ToActionResult_ErrorWithTemplatedDetail_AddsTheDetailTemplatedExtension()
    {
        // Arrange
        var error = Error.Failure(
            ErrorUri.Tag(FailureTypeUri),
            "Failure title",
            "Failure detail.",
            ErrorUri.Locator(FailureInstanceUri),
            "errors.failure.detail",
            ("key1", "value1"));
        var sut = Result.FromError(error);

        // Act
        var result = sut.ToActionResult();

        // Assert
        var problem = result.ShouldBeAssignableTo<ObjectResult>().ShouldNotBeNull()
            .Value.ShouldBeOfType<ProblemDetails>();
        problem.Extensions["detailTemplated"].ShouldBeOfType<TemplatedMessage>()
            .TemplateId.ShouldBe("errors.failure.detail");
    }

    [Fact]
    public void ToActionResult_ErrorWithDetails_AddsTheErrorsExtension()
    {
        // Arrange
        var error = Error.Validation(
                ErrorUri.Tag("tag:test.com,2026:validation"),
                "Validation title",
                "Validation detail.",
                ErrorUri.Locator("https://test.com/instances/validation"))
            .AddDetail("#/property1", "Property 1 detail.", "errors.property1", ("key1", "value1"))
            .AddDetail("#/property2", "Property 2 detail.");
        var sut = Result.FromError(error);

        // Act
        var result = sut.ToActionResult();

        // Assert
        var problem = result.ShouldBeAssignableTo<ObjectResult>().ShouldNotBeNull()
            .Value.ShouldBeOfType<ProblemDetails>();
        problem.Extensions.ShouldNotContainKey("detailTemplated");

        var errors = problem.Extensions["errors"].ShouldBeOfType<ErrorDetail[]>();
        errors.Length.ShouldBe(2);

        errors[0].PropertyPointer.ShouldBe("#/property1");
        errors[0].Detail.ShouldBe("Property 1 detail.");
        var detailTemplated = errors[0].DetailTemplated.ShouldNotBeNull();
        detailTemplated.TemplateId.ShouldBe("errors.property1");
        detailTemplated.Params.ShouldNotBeNull()["key1"].ShouldBe("value1");

        errors[1].PropertyPointer.ShouldBe("#/property2");
        errors[1].Detail.ShouldBe("Property 2 detail.");
        errors[1].DetailTemplated.ShouldBeNull();
    }

    [Fact]
    public void ToActionResult_ErrorMappingReturningResult_ReturnsTheMappedResult()
    {
        // Arrange
        var sut = Result.FromError(CreateFailureError());

        // Act
        var result = sut.ToActionResult(customErrorMapping: MapFailureToPaymentRequired);

        // Assert
        result.ShouldBeOfType<StatusCodeResult>().StatusCode.ShouldBe(StatusCodes.Status402PaymentRequired);
    }

    [Fact]
    public void ToActionResult_ErrorMappingReturningNull_ReturnsDefaultProblemDetails()
    {
        // Arrange
        var sut = Result.FromError(CreateError(ErrorCategory.NotFound));

        // Act
        var result = sut.ToActionResult(customErrorMapping: MapFailureToPaymentRequired);

        // Assert
        result.ShouldBeAssignableTo<ObjectResult>().ShouldNotBeNull()
            .StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public void ToActionResult_SuccessfulResultWithErrorMapping_DoesNotInvokeTheMapping()
    {
        // Arrange
        var invoked = false;
        var sut = Result.Success();

        // Act
        var result = sut.ToActionResult(customErrorMapping: _ =>
        {
            invoked = true;
            return null;
        });

        // Assert
        result.ShouldBeOfType<NoContentResult>();
        invoked.ShouldBeFalse();
    }

    [Fact]
    public void ToActionResult_SuccessStatusCode_ReturnsTheStatusCode()
    {
        // Arrange
        var sut = Result.Success();

        // Act
        var result = sut.ToActionResult(HttpStatusCode.Accepted);

        // Assert
        result.ShouldBeOfType<StatusCodeResult>().StatusCode.ShouldBe(StatusCodes.Status202Accepted);
    }

    [Fact]
    public void ToActionResult_SuccessStatusCodeWithErrorAndMapping_ReturnsTheMappedResult()
    {
        // Arrange
        var sut = Result.FromError(CreateFailureError());

        // Act
        var result = sut.ToActionResult(HttpStatusCode.Accepted, MapFailureToPaymentRequired);

        // Assert
        result.ShouldBeOfType<StatusCodeResult>().StatusCode.ShouldBe(StatusCodes.Status402PaymentRequired);
    }

    [Fact]
    public void ToActionResult_SuccessMapping_ReturnsTheMappedResult()
    {
        // Arrange
        var sut = Result.Success();

        // Act
        var result = sut.ToActionResult(() => new AcceptedResult());

        // Assert
        result.ShouldBeOfType<AcceptedResult>();
    }

    [Fact]
    public void ToActionResult_SuccessMappingWithError_ReturnsDefaultProblemDetailsWithoutInvokingTheMapping()
    {
        // Arrange
        var invoked = false;
        var sut = Result.FromError(CreateFailureError());

        // Act
        var result = sut.ToActionResult(() =>
        {
            invoked = true;
            return new AcceptedResult();
        });

        // Assert
        result.ShouldBeAssignableTo<ObjectResult>().ShouldNotBeNull()
            .StatusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
        invoked.ShouldBeFalse();
    }

    [Fact]
    public void ToActionResult_SuccessMappingWithErrorAndErrorMapping_ReturnsTheMappedResult()
    {
        // Arrange
        var sut = Result.FromError(CreateFailureError());

        // Act
        var result = sut.ToActionResult(() => new AcceptedResult(), MapFailureToPaymentRequired);

        // Assert
        result.ShouldBeOfType<StatusCodeResult>().StatusCode.ShouldBe(StatusCodes.Status402PaymentRequired);
    }

    #endregion

    #region Result<T>

    [Fact]
    public void ToActionResult_SuccessfulResultWithValue_ReturnsOkWithTheValue()
    {
        // Arrange
        var value = new TestValue(13, "Test value");
        var sut = Result<TestValue>.FromValue(value);

        // Act
        var result = sut.ToActionResult();

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(value);
    }

    [Fact]
    public void ToActionResult_SuccessfulResultWithNullValue_ReturnsNoContent()
    {
        // Arrange
        var sut = Result<TestValue?>.FromValue(null);

        // Act
        var result = sut.ToActionResult();

        // Assert
        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact]
    public void ToActionResult_GenericResultWithError_ReturnsProblemDetails()
    {
        // Arrange
        var sut = Result<TestValue>.FromError(CreateFailureError());

        // Act
        var result = sut.ToActionResult();

        // Assert
        var objectResult = result.ShouldBeAssignableTo<ObjectResult>().ShouldNotBeNull();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
        objectResult.Value.ShouldBeOfType<ProblemDetails>().Title.ShouldBe("Failure title");
    }

    [Fact]
    public void ToActionResult_ValueWithUntypedLambda_SelectsTheSuccessMapping()
    {
        // Arrange
        var value = new TestValue(13, "Test value");
        var sut = Result<TestValue>.FromValue(value);

        // Act
        // The body of the lambda would also compile for an Error parameter.
        var result = sut.ToActionResult(mapped => new ObjectResult(mapped)
        {
            StatusCode = StatusCodes.Status203NonAuthoritative
        });

        // Assert
        var objectResult = result.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status203NonAuthoritative);
        objectResult.Value.ShouldBe(value);
    }

    [Fact]
    public void ToActionResult_ErrorWithUntypedLambda_ReturnsProblemDetailsWithoutInvokingTheLambda()
    {
        // Arrange
        var invoked = false;
        var sut = Result<TestValue>.FromError(CreateFailureError());

        // Act
        // The body of the lambda would also compile for an Error parameter.
        var result = sut.ToActionResult(mapped =>
        {
            invoked = true;
            return new OkObjectResult(mapped);
        });

        // Assert
        var objectResult = result.ShouldBeAssignableTo<ObjectResult>().ShouldNotBeNull();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
        objectResult.Value.ShouldBeOfType<ProblemDetails>();
        invoked.ShouldBeFalse();
    }

    [Fact]
    public void ToActionResult_ValueWithErrorMappingOnly_ReturnsOkWithTheValue()
    {
        // Arrange
        var value = new TestValue(13, "Test value");
        var sut = Result<TestValue>.FromValue(value);

        // Act
        var result = sut.ToActionResult(customErrorMapping: MapFailureToPaymentRequired);

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(value);
    }

    [Fact]
    public void ToActionResult_NullValueWithErrorMappingOnly_ReturnsNoContent()
    {
        // Arrange
        var sut = Result<TestValue?>.FromValue(null);

        // Act
        var result = sut.ToActionResult(customErrorMapping: MapFailureToPaymentRequired);

        // Assert
        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact]
    public void ToActionResult_GenericResultWithErrorAndErrorMappingOnly_ReturnsTheMappedResult()
    {
        // Arrange
        var sut = Result<TestValue>.FromError(CreateFailureError());

        // Act
        var result = sut.ToActionResult(customErrorMapping: MapFailureToPaymentRequired);

        // Assert
        result.ShouldBeOfType<StatusCodeResult>().StatusCode.ShouldBe(StatusCodes.Status402PaymentRequired);
    }

    [Fact]
    public void ToActionResult_NullValueWithSuccessMapping_InvokesTheSuccessMapping()
    {
        // Arrange
        var sut = Result<TestValue?>.FromValue(null);

        // Act
        var result = sut.ToActionResult(value => value is null ? new AcceptedResult() : new OkResult());

        // Assert
        result.ShouldBeOfType<AcceptedResult>();
    }

    [Fact]
    public void ToActionResult_GenericResultWithSuccessMappingAndError_UsesTheErrorMapping()
    {
        // Arrange
        var sut = Result<TestValue>.FromError(CreateFailureError());

        // Act
        var result = sut.ToActionResult(MapValue, MapFailureToPaymentRequired);

        // Assert
        result.ShouldBeOfType<StatusCodeResult>().StatusCode.ShouldBe(StatusCodes.Status402PaymentRequired);
    }

    [Fact]
    public void ToActionResult_GenericResultWithSuccessStatusCode_ReturnsTheStatusCodeWithTheValue()
    {
        // Arrange
        var value = new TestValue(13, "Test value");
        var sut = Result<TestValue>.FromValue(value);

        // Act
        var result = sut.ToActionResult(HttpStatusCode.Created);

        // Assert
        var objectResult = result.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status201Created);
        objectResult.Value.ShouldBe(value);
    }

    [Fact]
    public void ToActionResult_NullValueWithSuccessStatusCode_ReturnsTheSuccessStatusCode()
    {
        // Arrange
        var sut = Result<TestValue?>.FromValue(null);

        // Act
        var result = sut.ToActionResult(HttpStatusCode.Created);

        // Assert
        result.ShouldBeOfType<StatusCodeResult>().StatusCode.ShouldBe(StatusCodes.Status201Created);
    }

    [Fact]
    public void ToActionResult_NullValueWithNoResponseStatusCode_ReturnsTheNoResponseStatusCode()
    {
        // Arrange
        var sut = Result<TestValue?>.FromValue(null);

        // Act
        var result = sut.ToActionResult(HttpStatusCode.Created, HttpStatusCode.Accepted);

        // Assert
        result.ShouldBeOfType<StatusCodeResult>().StatusCode.ShouldBe(StatusCodes.Status202Accepted);
    }

    #endregion

    #region public API

    [Fact]
    public void Assembly_Always_ExposesOnlyTheExtensionMethods()
    {
        // Act
        var exportedTypes = typeof(ActionResultExtensions).Assembly.GetExportedTypes();

        // Assert
        exportedTypes.ShouldBe([typeof(ActionResultExtensions)]);
    }

    #endregion

    #region argument validation

    [Fact]
    public void ToActionResult_NullResult_ThrowsArgumentNullException()
    {
        // Act
        var act = () => ((Result)null!).ToActionResult();

        // Assert
        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("result");
    }

    [Fact]
    public void ToActionResult_NullResultWithSuccessStatusCode_ThrowsArgumentNullException()
    {
        // Act
        var act = () => ((Result)null!).ToActionResult(HttpStatusCode.Accepted);

        // Assert
        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("result");
    }

    [Fact]
    public void ToActionResult_NullResultWithSuccessMapping_ThrowsArgumentNullException()
    {
        // Act
        var act = () => ((Result)null!).ToActionResult(() => new AcceptedResult());

        // Assert
        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("result");
    }

    [Fact]
    public void ToActionResult_NullSuccessMapping_ReturnsNoContent()
    {
        // Arrange
        var sut = Result.Success();

        // Act
        var result = sut.ToActionResult((Func<ActionResult>?)null);

        // Assert
        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact]
    public void ToActionResult_NullGenericResult_ThrowsArgumentNullException()
    {
        // Act
        var act = () => ((Result<TestValue>)null!).ToActionResult();

        // Assert
        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("result");
    }

    [Fact]
    public void ToActionResult_NullGenericResultWithSuccessStatusCode_ThrowsArgumentNullException()
    {
        // Act
        var act = () => ((Result<TestValue>)null!).ToActionResult(HttpStatusCode.Created);

        // Assert
        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("result");
    }

    [Fact]
    public void ToActionResult_NullGenericResultWithSuccessMapping_ThrowsArgumentNullException()
    {
        // Act
        var act = () => ((Result<TestValue>)null!).ToActionResult(MapValue, null);

        // Assert
        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("result");
    }

    [Fact]
    public void ToActionResult_GenericResultWithNullSuccessMapping_ReturnsOkWithTheValue()
    {
        // Arrange
        var value = new TestValue(13, "Test value");
        var sut = Result<TestValue>.FromValue(value);

        // Act
        var result = sut.ToActionResult((Func<TestValue, ActionResult>?)null);

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(value);
    }

    #endregion

    #region helper methods

    private static Error CreateFailureError()
    {
        return Error.Failure(
            ErrorUri.Tag(FailureTypeUri),
            "Failure title",
            "Failure detail.",
            ErrorUri.Locator(FailureInstanceUri));
    }

    private static Error CreateError(ErrorCategory category)
    {
        var typeUri = ErrorUri.Tag($"tag:test.com,2026:{category}".ToLowerInvariant());
        var title = $"{category} title";

        return category switch
        {
            ErrorCategory.Validation => Error.Validation(typeUri, title),
            ErrorCategory.NotFound => Error.NotFound(typeUri, title),
            ErrorCategory.Conflict => Error.Conflict(typeUri, title),
            ErrorCategory.Critical => Error.Critical(typeUri, title),
            _ => throw new NotSupportedException($"Unsupported ErrorCategory: {category}")
        };
    }

    private static ActionResult MapValue(TestValue value)
    {
        return new OkObjectResult(value.Id * 2);
    }

    private static ActionResult? MapFailureToPaymentRequired(Error error)
    {
        return error.Category == ErrorCategory.Failure
            ? new StatusCodeResult(StatusCodes.Status402PaymentRequired)
            : null;
    }

    #endregion
}
