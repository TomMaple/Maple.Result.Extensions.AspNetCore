using Maple.Result.Extensions.AspNetCore.Tests.Unit.TestingInfrastructure;
using Maple.Result.Extensions.AspNetCore.Tests.Unit.TestingInfrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Maple.Result.Extensions.AspNetCore.Tests.Unit;

// The aliases are declared inside the namespace so that they take precedence over the Maple.Result types
// of the same name.
using ErrorDetail = AspNetCore.ViewModels.ErrorDetail;
using TemplatedMessage = AspNetCore.ViewModels.TemplatedMessage;

public class ActionResultExtensionsTests
{
    #region consts

    private const string FailureTypeUri = "tag:test.com,2026:failure";
    private const string FailureInstanceUri = "https://test.com/instances/failure";

    #endregion

    #region (Result) default mapping

    [Fact]
    public void ToActionResult_SuccessfulResult_ReturnsNoContent()
    {
        // Arrange
        var controller = TestControllerFactory.Create();
        var sut = Result.Success();

        // Act
        var result = sut.ToActionResult(controller);

        // Assert
        var noContent = result.ShouldBeOfType<NoContentResult>();
        noContent.StatusCode.ShouldBe(StatusCodes.Status204NoContent);
    }

    [Fact]
    public void ToActionResult_Error_ReturnsProblemDetailsMappedFromTheError()
    {
        // Arrange
        var controller = TestControllerFactory.Create();
        var sut = Result.FromError(CreateFailureError());

        // Act
        var result = sut.ToActionResult(controller);

        // Assert
        var objectResult = result.ShouldBeOfType<ObjectResult>();
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
    [InlineData(ErrorCategory.Unauthenticated, StatusCodes.Status401Unauthorized)]
    [InlineData(ErrorCategory.Unauthorized, StatusCodes.Status403Forbidden)]
    [InlineData(ErrorCategory.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorCategory.Timeout, StatusCodes.Status408RequestTimeout)]
    [InlineData(ErrorCategory.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(ErrorCategory.Failure, StatusCodes.Status422UnprocessableEntity)]
    [InlineData(ErrorCategory.Critical, StatusCodes.Status500InternalServerError)]
    [InlineData(ErrorCategory.NotImplemented, StatusCodes.Status501NotImplemented)]
    [InlineData(ErrorCategory.Unavailable, StatusCodes.Status503ServiceUnavailable)]
    public void ToActionResult_ErrorOfTheGivenCategory_ReturnsExpectedStatusCode(
        ErrorCategory category, int expectedStatusCode)
    {
        // Arrange
        var controller = TestControllerFactory.Create();
        var sut = Result.FromError(CreateError(category));

        // Act
        var result = sut.ToActionResult(controller);

        // Assert
        var objectResult = result.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(expectedStatusCode);
        objectResult.Value.ShouldBeOfType<ProblemDetails>().Status.ShouldBe(expectedStatusCode);
    }

    [Fact]
    public void ToActionResult_ErrorWithTemplatedDetail_AddsTheDetailTemplatedExtension()
    {
        // Arrange
        var controller = TestControllerFactory.Create();
        var error = Error.Failure(
            ErrorUri.Tag(FailureTypeUri),
            "Failure title",
            "Failure detail.",
            ErrorUri.Locator(FailureInstanceUri),
            "errors.failure.detail",
            ("key1", "value1"), ("key2", 123));

        var sut = Result.FromError(error);

        // Act
        var result = sut.ToActionResult(controller);

        // Assert
        var problem = result.ShouldBeOfType<ObjectResult>().Value.ShouldBeOfType<ProblemDetails>();
        problem.Extensions.ShouldNotContainKey("errors");

        var detailTemplated = problem.Extensions["detailTemplated"].ShouldBeOfType<TemplatedMessage>();
        detailTemplated.TemplateId.ShouldBe("errors.failure.detail");
        detailTemplated.Params.ShouldNotBeNull();
        detailTemplated.Params["key1"].ShouldBe("value1");
        detailTemplated.Params["key2"].ShouldBe(123);
    }

    [Fact]
    public void ToActionResult_ErrorWithDetails_AddsTheErrorsExtension()
    {
        // Arrange
        var controller = TestControllerFactory.Create();
        var error = Error.Validation(
                ErrorUri.Tag("tag:test.com,2026:validation"),
                "Validation title",
                "Validation detail.",
                ErrorUri.Locator("https://test.com/instances/validation"))
            .AddDetail("#/property1", "Property 1 failure detail.", "errors.failure.property1", ("pk1", "pv1"))
            .AddDetail("#/property2", "Property 2 failure detail.");

        var sut = Result.FromError(error);

        // Act
        var result = sut.ToActionResult(controller);

        // Assert
        var problem = result.ShouldBeOfType<ObjectResult>().Value.ShouldBeOfType<ProblemDetails>();
        problem.Extensions.ShouldNotContainKey("detailTemplated");

        var errors = problem.Extensions["errors"].ShouldBeOfType<ErrorDetail[]>();
        errors.Length.ShouldBe(2);

        var firstError = errors[0];
        firstError.PropertyPointer.ShouldBe("#/property1");
        firstError.Detail.ShouldBe("Property 1 failure detail.");
        firstError.DetailTemplated.ShouldNotBeNull();
        firstError.DetailTemplated.TemplateId.ShouldBe("errors.failure.property1");
        firstError.DetailTemplated.Params.ShouldNotBeNull();
        firstError.DetailTemplated.Params["pk1"].ShouldBe("pv1");

        var secondError = errors[1];
        secondError.PropertyPointer.ShouldBe("#/property2");
        secondError.Detail.ShouldBe("Property 2 failure detail.");
        secondError.DetailTemplated.ShouldBeNull();
    }

    #endregion

    #region (Result) custom error mapping

    [Fact]
    public void ToActionResult_ErrorMatchingTheCustomMapping_ReturnsCustomMappedResult()
    {
        // Arrange
        var controller = TestControllerFactory.Create();
        var sut = Result.FromError(CreateFailureError());

        // Act
        var result = sut.ToActionResult(controller, MapFailureToPaymentRequired);

        // Assert
        var objectResult = result.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status402PaymentRequired);
        objectResult.Value.ShouldBe(new TestValue(11, "Failure title"));
    }

    [Fact]
    public void ToActionResult_ErrorNotMatchingTheCustomMapping_ReturnsDefaultProblemDetails()
    {
        // Arrange
        var controller = TestControllerFactory.Create();
        var sut = Result.FromError(CreateFailureError());

        // Act
        var result = sut.ToActionResult(controller, MapConflictToPaymentRequired);

        // Assert
        var objectResult = result.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
        objectResult.Value.ShouldBeOfType<ProblemDetails>();
    }

    [Fact]
    public void ToActionResult_SuccessfulResultWithCustomMapping_DoesNotInvokeTheCustomMapping()
    {
        // Arrange
        var controller = TestControllerFactory.Create();
        var sut = Result.Success();
        var invoked = false;

        // Act
        var result = sut.ToActionResult(controller, (_, _) =>
        {
            invoked = true;
            return null;
        });

        // Assert
        result.ShouldBeOfType<NoContentResult>();
        invoked.ShouldBeFalse();
    }

    [Fact]
    public void ToActionResult_ErrorWithCustomMapping_PassesTheErrorAndControllerToTheCustomMapping()
    {
        // Arrange
        var controller = TestControllerFactory.Create();
        var error = CreateFailureError();
        var sut = Result.FromError(error);
        Error? mappedError = null;
        ControllerBase? mappedController = null;

        // Act
        sut.ToActionResult(controller, (e, c) =>
        {
            mappedError = e;
            mappedController = c;
            return null;
        });

        // Assert
        mappedError.ShouldBeSameAs(error);
        mappedController.ShouldBeSameAs(controller);
    }

    #endregion

    #region (Result) success status code

    [Fact]
    public void ToActionResult_SuccessfulResultWithSuccessStatusCode_ReturnsTheGivenStatusCode()
    {
        // Arrange
        var controller = TestControllerFactory.Create();
        var sut = Result.Success();

        // Act
        var result = sut.ToActionResult(controller, StatusCodes.Status202Accepted);

        // Assert
        var statusCode = result.ShouldBeOfType<StatusCodeResult>();
        statusCode.StatusCode.ShouldBe(StatusCodes.Status202Accepted);
    }

    [Fact]
    public void ToActionResult_ErrorWithSuccessStatusCode_ReturnsProblemDetails()
    {
        // Arrange
        var controller = TestControllerFactory.Create();
        var sut = Result.FromError(CreateFailureError());

        // Act
        var result = sut.ToActionResult(controller, StatusCodes.Status202Accepted);

        // Assert
        result.ShouldBeOfType<ObjectResult>().StatusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
    }

    [Fact]
    public void ToActionResult_ErrorWithSuccessStatusCodeAndCustomMapping_ReturnsCustomMappedResult()
    {
        // Arrange
        var controller = TestControllerFactory.Create();
        var sut = Result.FromError(CreateFailureError());

        // Act
        var result = sut.ToActionResult(controller, StatusCodes.Status202Accepted, MapFailureToPaymentRequired);

        // Assert
        result.ShouldBeOfType<ObjectResult>().StatusCode.ShouldBe(StatusCodes.Status402PaymentRequired);
    }

    #endregion

    #region (Result) success mapping

    [Fact]
    public void ToActionResult_SuccessfulResultWithSuccessMapping_ReturnsMappedResult()
    {
        // Arrange
        var controller = TestControllerFactory.Create();
        var sut = Result.Success();

        // Act
        var result = sut.ToActionResult(controller, MapSuccess);

        // Assert
        var objectResult = result.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status202Accepted);
        objectResult.Value.ShouldBe(new TestValue(31, "Mapped success"));
    }

    [Fact]
    public void ToActionResult_ErrorWithSuccessMapping_DoesNotInvokeTheSuccessMapping()
    {
        // Arrange
        var controller = TestControllerFactory.Create();
        var sut = Result.FromError(CreateFailureError());
        var invoked = false;

        // Act
        var result = sut.ToActionResult(controller, c =>
        {
            invoked = true;
            return c.Ok();
        });

        // Assert
        result.ShouldBeOfType<ObjectResult>().StatusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
        invoked.ShouldBeFalse();
    }

    [Fact]
    public void ToActionResult_ErrorWithSuccessMappingAndCustomMapping_ReturnsCustomMappedResult()
    {
        // Arrange
        var controller = TestControllerFactory.Create();
        var sut = Result.FromError(CreateFailureError());

        // Act
        var result = sut.ToActionResult(controller, MapSuccess, MapFailureToPaymentRequired);

        // Assert
        result.ShouldBeOfType<ObjectResult>().StatusCode.ShouldBe(StatusCodes.Status402PaymentRequired);
    }

    #endregion

    #region (Result<T>) default mapping

    [Fact]
    public void ToActionResult_SuccessfulResultWithValue_ReturnsOkWithTheValue()
    {
        // Arrange
        var controller = TestControllerFactory.Create();
        var value = new TestValue(13, "Test value");
        var sut = Result<TestValue>.FromValue(value);

        // Act
        var result = sut.ToActionResult(controller);

        // Assert
        var ok = result.ShouldBeOfType<OkObjectResult>();
        ok.StatusCode.ShouldBe(StatusCodes.Status200OK);
        ok.Value.ShouldBeSameAs(value);
    }

    [Fact]
    public void ToActionResult_SuccessfulResultWithNullValue_ReturnsNoContent()
    {
        // Arrange
        var controller = TestControllerFactory.Create();
        var sut = Result<TestValue?>.FromValue(null);

        // Act
        var result = sut.ToActionResult(controller);

        // Assert
        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact]
    public void ToActionResult_GenericResultWithError_ReturnsProblemDetails()
    {
        // Arrange
        var controller = TestControllerFactory.Create();
        var sut = Result<TestValue>.FromError(CreateFailureError());

        // Act
        var result = sut.ToActionResult(controller);

        // Assert
        result.ShouldBeOfType<ObjectResult>().StatusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
    }

    [Fact]
    public void ToActionResult_GenericResultWithErrorMatchingTheCustomMapping_ReturnsCustomMappedResult()
    {
        // Arrange
        var controller = TestControllerFactory.Create();
        var sut = Result<TestValue>.FromError(CreateFailureError());

        // Act
        var result = sut.ToActionResult(controller, MapFailureToPaymentRequired);

        // Assert
        result.ShouldBeOfType<ObjectResult>().StatusCode.ShouldBe(StatusCodes.Status402PaymentRequired);
    }

    #endregion

    #region (Result<T>) success status code

    [Fact]
    public void ToActionResult_SuccessfulResultWithValueAndSuccessStatusCode_ReturnsTheGivenStatusCodeWithTheValue()
    {
        // Arrange
        var controller = TestControllerFactory.Create();
        var value = new TestValue(13, "Test value");
        var sut = Result<TestValue>.FromValue(value);

        // Act
        var result = sut.ToActionResult(controller, StatusCodes.Status201Created);

        // Assert
        var objectResult = result.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status201Created);
        objectResult.Value.ShouldBeSameAs(value);
    }

    [Fact]
    public void ToActionResult_SuccessfulResultWithNullValueAndNoSuccessNoResponseStatusCode_ReturnsTheSuccessStatusCode()
    {
        // Arrange
        var controller = TestControllerFactory.Create();
        var sut = Result<TestValue?>.FromValue(null);

        // Act
        var result = sut.ToActionResult(controller, StatusCodes.Status226IMUsed);

        // Assert
        var statusCode = result.ShouldBeOfType<StatusCodeResult>();
        statusCode.StatusCode.ShouldBe(StatusCodes.Status226IMUsed);
    }

    [Fact]
    public void ToActionResult_SuccessfulResultWithNullValueAndSuccessNoResponseStatusCode_ReturnsTheSuccessNoResponseStatusCode()
    {
        // Arrange
        var controller = TestControllerFactory.Create();
        var sut = Result<TestValue?>.FromValue(null);

        // Act
        var result = sut.ToActionResult(
            controller, StatusCodes.Status203NonAuthoritative, StatusCodes.Status205ResetContent);

        // Assert
        var statusCode = result.ShouldBeOfType<StatusCodeResult>();
        statusCode.StatusCode.ShouldBe(StatusCodes.Status205ResetContent);
    }

    [Fact]
    public void ToActionResult_GenericResultWithErrorAndSuccessStatusCode_ReturnsProblemDetails()
    {
        // Arrange
        var controller = TestControllerFactory.Create();
        var sut = Result<TestValue>.FromError(CreateFailureError());

        // Act
        var result = sut.ToActionResult(controller, StatusCodes.Status201Created);

        // Assert
        result.ShouldBeOfType<ObjectResult>().StatusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
    }

    [Fact]
    public void ToActionResult_GenericResultWithErrorAndBothStatusCodesAndCustomMapping_ReturnsCustomMappedResult()
    {
        // Arrange
        var controller = TestControllerFactory.Create();
        var sut = Result<TestValue>.FromError(CreateFailureError());

        // Act
        var result = sut.ToActionResult(controller, StatusCodes.Status201Created,
            StatusCodes.Status205ResetContent, MapFailureToPaymentRequired);

        // Assert
        result.ShouldBeOfType<ObjectResult>().StatusCode.ShouldBe(StatusCodes.Status402PaymentRequired);
    }

    [Fact]
    public void ToActionResult_GenericResultWithErrorAndSuccessStatusCodeAndCustomMapping_ReturnsCustomMappedResult()
    {
        // Arrange
        var controller = TestControllerFactory.Create();
        var sut = Result<TestValue>.FromError(CreateFailureError());

        // Act
        var result = sut.ToActionResult(controller, StatusCodes.Status201Created,
            customErrorMapping: MapFailureToPaymentRequired);

        // Assert
        result.ShouldBeOfType<ObjectResult>().StatusCode.ShouldBe(StatusCodes.Status402PaymentRequired);
    }

    #endregion

    #region (Result<T>) success mapping

    [Fact]
    public void ToActionResult_SuccessfulResultWithValueAndSuccessMapping_ReturnsMappedValue()
    {
        // Arrange
        var controller = TestControllerFactory.Create();
        var sut = Result<TestValue>.FromValue(new TestValue(13, "Test value"));

        // Act
        var result = sut.ToActionResult(controller, MapSuccessValue);

        // Assert
        var objectResult = result.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status203NonAuthoritative);
        objectResult.Value.ShouldBe(new TestValue(26, "Test value"));
    }

    [Fact]
    public void ToActionResult_SuccessfulResultWithNullValueAndSuccessMapping_InvokesTheSuccessMappingWithNull()
    {
        // Arrange
        var controller = TestControllerFactory.Create();
        var sut = Result<TestValue?>.FromValue(null);

        // Act
        var result = sut.ToActionResult(controller, MapSuccessValue);

        // Assert
        var statusCode = result.ShouldBeOfType<StatusCodeResult>();
        statusCode.StatusCode.ShouldBe(StatusCodes.Status205ResetContent);
    }

    [Fact]
    public void ToActionResult_GenericResultWithErrorAndSuccessMapping_DoesNotInvokeTheSuccessMapping()
    {
        // Arrange
        var controller = TestControllerFactory.Create();
        var sut = Result<TestValue>.FromError(CreateFailureError());
        var invoked = false;

        // Act
        // The lambda parameter is typed explicitly so that the success mapping overload is selected
        // instead of the one taking the custom error mapping.
        var result = sut.ToActionResult(controller, (TestValue _, ControllerBase c) =>
        {
            invoked = true;
            return c.Ok();
        });

        // Assert
        result.ShouldBeOfType<ObjectResult>().StatusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
        invoked.ShouldBeFalse();
    }

    [Fact]
    public void ToActionResult_GenericResultWithErrorAndSuccessMappingAndCustomMapping_ReturnsCustomMappedResult()
    {
        // Arrange
        var controller = TestControllerFactory.Create();
        var sut = Result<TestValue>.FromError(CreateFailureError());

        // Act
        var result = sut.ToActionResult(controller, MapSuccessValue, MapFailureToPaymentRequired);

        // Assert
        result.ShouldBeOfType<ObjectResult>().StatusCode.ShouldBe(StatusCodes.Status402PaymentRequired);
    }

    #endregion

    #region registered result mappings

    [Fact]
    public void ToActionResult_ErrorMatchingARegisteredMapping_ReturnsRegisteredMappedResult()
    {
        // Arrange
        var controller = TestControllerFactory.Create(
            options => options.ErrorMappings.Add(MapFailureToPaymentRequired));

        var sut = Result.FromError(CreateFailureError());

        // Act
        var result = sut.ToActionResult(controller);

        // Assert
        var objectResult = result.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status402PaymentRequired);
        objectResult.Value.ShouldBe(new TestValue(11, "Failure title"));
    }

    [Fact]
    public void ToActionResult_ErrorNotMatchingAnyRegisteredMapping_ReturnsDefaultProblemDetails()
    {
        // Arrange
        var controller = TestControllerFactory.Create(
            options => options.ErrorMappings.Add(MapConflictToPaymentRequired));

        var sut = Result.FromError(CreateFailureError());

        // Act
        var result = sut.ToActionResult(controller);

        // Assert
        var objectResult = result.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
        objectResult.Value.ShouldBeOfType<ProblemDetails>();
    }

    [Fact]
    public void ToActionResult_ErrorWithSeveralRegisteredMappings_UsesTheFirstNonNullResult()
    {
        // Arrange
        var controller = TestControllerFactory.Create(options =>
        {
            options.ErrorMappings.Add(MapConflictToPaymentRequired);
            options.ErrorMappings.Add(MapFailureToPaymentRequired);
            options.ErrorMappings.Add(MapFailureToConflict);
        });

        var sut = Result.FromError(CreateFailureError());

        // Act
        var result = sut.ToActionResult(controller);

        // Assert
        result.ShouldBeOfType<ObjectResult>().StatusCode.ShouldBe(StatusCodes.Status402PaymentRequired);
    }

    [Fact]
    public void ToActionResult_ErrorWithBothMappings_PrefersTheMappingPassedToTheMethod()
    {
        // Arrange
        var controller = TestControllerFactory.Create(
            options => options.ErrorMappings.Add(MapFailureToPaymentRequired));

        var sut = Result.FromError(CreateFailureError());

        // Act
        var result = sut.ToActionResult(controller, MapFailureToConflict);

        // Assert
        result.ShouldBeOfType<ObjectResult>().StatusCode.ShouldBe(StatusCodes.Status409Conflict);
    }

    [Fact]
    public void ToActionResult_ErrorWithARegisteredMappingAndNonMatchingMethodMapping_UsesTheRegisteredMapping()
    {
        // Arrange
        var controller = TestControllerFactory.Create(
            options => options.ErrorMappings.Add(MapFailureToPaymentRequired));

        var sut = Result.FromError(CreateFailureError());

        // Act
        var result = sut.ToActionResult(controller, MapConflictToPaymentRequired);

        // Assert
        result.ShouldBeOfType<ObjectResult>().StatusCode.ShouldBe(StatusCodes.Status402PaymentRequired);
    }

    #endregion

    #region controller-first overloads

    [Fact]
    public void ToActionResult_ControllerFirstWithSuccessfulResult_ReturnsNoContent()
    {
        // Arrange
        var sut = TestControllerFactory.Create();

        // Act
        var result = sut.ToActionResult(Result.Success());

        // Assert
        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact]
    public void ToActionResult_ControllerFirstWithError_ReturnsProblemDetails()
    {
        // Arrange
        var sut = TestControllerFactory.Create();

        // Act
        var result = sut.ToActionResult(Result.FromError(CreateFailureError()));

        // Assert
        result.ShouldBeOfType<ObjectResult>().StatusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
    }

    [Fact]
    public void ToActionResult_ControllerFirstWithSuccessStatusCode_ReturnsTheGivenStatusCode()
    {
        // Arrange
        var sut = TestControllerFactory.Create();

        // Act
        var result = sut.ToActionResult(Result.Success(), StatusCodes.Status202Accepted);

        // Assert
        result.ShouldBeOfType<StatusCodeResult>().StatusCode.ShouldBe(StatusCodes.Status202Accepted);
    }

    [Fact]
    public void ToActionResult_ControllerFirstWithSuccessMapping_ReturnsMappedResult()
    {
        // Arrange
        var sut = TestControllerFactory.Create();

        // Act
        var result = sut.ToActionResult(Result.Success(), MapSuccess);

        // Assert
        result.ShouldBeOfType<ObjectResult>().StatusCode.ShouldBe(StatusCodes.Status202Accepted);
    }

    [Fact]
    public void ToActionResult_ControllerFirstWithGenericResult_ReturnsOkWithTheValue()
    {
        // Arrange
        var sut = TestControllerFactory.Create();
        var value = new TestValue(13, "Test value");

        // Act
        var result = sut.ToActionResult(Result<TestValue>.FromValue(value));

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeSameAs(value);
    }

    [Fact]
    public void ToActionResult_ControllerFirstWithGenericResultAndSuccessStatusCode_ReturnsTheGivenStatusCode()
    {
        // Arrange
        var sut = TestControllerFactory.Create();

        // Act
        var result = sut.ToActionResult(Result<TestValue?>.FromValue(null),
            StatusCodes.Status203NonAuthoritative, StatusCodes.Status205ResetContent);

        // Assert
        result.ShouldBeOfType<StatusCodeResult>().StatusCode.ShouldBe(StatusCodes.Status205ResetContent);
    }

    [Fact]
    public void ToActionResult_ControllerFirstWithGenericResultAndSuccessMapping_ReturnsMappedValue()
    {
        // Arrange
        var sut = TestControllerFactory.Create();

        // Act
        var result = sut.ToActionResult(Result<TestValue>.FromValue(new TestValue(13, "Test value")),
            MapSuccessValue);

        // Assert
        result.ShouldBeOfType<ObjectResult>().StatusCode.ShouldBe(StatusCodes.Status203NonAuthoritative);
    }

    #endregion

    #region guard clauses

    [Fact]
    public void ToActionResult_NullResult_ThrowsArgumentNullException()
    {
        // Arrange
        var controller = TestControllerFactory.Create();

        // Act
        var act = () => ((Result)null!).ToActionResult(controller);

        // Assert
        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("result");
    }

    [Fact]
    public void ToActionResult_NullController_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = Result.Success();

        // Act
        var act = () => sut.ToActionResult((ControllerBase)null!);

        // Assert
        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("controller");
    }

    [Fact]
    public void ToActionResult_NullResultAndNullController_ThrowsArgumentNullExceptionForTheResult()
    {
        // Act
        var act = () => ((Result)null!).ToActionResult((ControllerBase)null!);

        // Assert
        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("result");
    }

    [Fact]
    public void ToActionResult_NullSuccessMapping_ThrowsArgumentNullException()
    {
        // Arrange
        var controller = TestControllerFactory.Create();
        var sut = Result.Success();

        // Act
        var act = () => sut.ToActionResult(controller, (Func<ControllerBase, ActionResult>)null!);

        // Assert
        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("customSuccessMapping");
    }

    [Fact]
    public void ToActionResult_NullResultWithSuccessStatusCode_ThrowsArgumentNullException()
    {
        // Arrange
        var controller = TestControllerFactory.Create();

        // Act
        var act = () => ((Result)null!).ToActionResult(controller, StatusCodes.Status202Accepted);

        // Assert
        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("result");
    }

    [Fact]
    public void ToActionResult_NullGenericResult_ThrowsArgumentNullException()
    {
        // Arrange
        var controller = TestControllerFactory.Create();

        // Act
        var act = () => ((Result<TestValue>)null!).ToActionResult(controller);

        // Assert
        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("result");
    }

    [Fact]
    public void ToActionResult_NullControllerWithGenericResult_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = Result<TestValue>.FromValue(new TestValue(13, "Test value"));

        // Act
        var act = () => sut.ToActionResult((ControllerBase)null!);

        // Assert
        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("controller");
    }

    [Fact]
    public void ToActionResult_GenericResultWithNullSuccessMapping_ThrowsArgumentNullException()
    {
        // Arrange
        var controller = TestControllerFactory.Create();
        var sut = Result<TestValue>.FromValue(new TestValue(13, "Test value"));

        // Act
        var act = () => sut.ToActionResult(controller, (Func<TestValue, ControllerBase, ActionResult>)null!);

        // Assert
        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("customSuccessMapping");
    }

    [Fact]
    public void ToActionResult_ControllerFirstWithNullController_ThrowsArgumentNullException()
    {
        // Act
        var act = () => ((ControllerBase)null!).ToActionResult(Result.Success());

        // Assert
        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("controller");
    }

    [Fact]
    public void ToActionResult_NullResultWithSuccessMapping_ThrowsArgumentNullException()
    {
        // Arrange
        var controller = TestControllerFactory.Create();

        // Act
        var act = () => ((Result)null!).ToActionResult(controller, MapSuccess);

        // Assert
        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("result");
    }

    [Fact]
    public void ToActionResult_NullResultAndNullSuccessMapping_ThrowsArgumentNullExceptionForTheResult()
    {
        // Arrange
        var controller = TestControllerFactory.Create();

        // Act
        var act = () => ((Result)null!).ToActionResult(
            controller, (Func<ControllerBase, ActionResult>)null!);

        // Assert
        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("result");
    }

    [Fact]
    public void ToActionResult_NullGenericResultWithSuccessStatusCode_ThrowsArgumentNullException()
    {
        // Arrange
        var controller = TestControllerFactory.Create();

        // Act
        var act = () => ((Result<TestValue>)null!).ToActionResult(controller, StatusCodes.Status201Created);

        // Assert
        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("result");
    }

    [Fact]
    public void ToActionResult_NullGenericResultWithSuccessMapping_ThrowsArgumentNullException()
    {
        // Arrange
        var controller = TestControllerFactory.Create();

        // Act
        var act = () => ((Result<TestValue>)null!).ToActionResult(controller, MapSuccessValue);

        // Assert
        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("result");
    }

    [Fact]
    public void ToActionResult_NullGenericResultAndNullSuccessMapping_ThrowsArgumentNullExceptionForTheResult()
    {
        // Arrange
        var controller = TestControllerFactory.Create();

        // Act
        var act = () => ((Result<TestValue>)null!).ToActionResult(
            controller, (Func<TestValue, ControllerBase, ActionResult>)null!);

        // Assert
        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("result");
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
            ErrorCategory.Unauthenticated => Error.Unauthenticated(typeUri, title),
            ErrorCategory.Unauthorized => Error.Unauthorized(typeUri, title),
            ErrorCategory.NotFound => Error.NotFound(typeUri, title),
            ErrorCategory.Timeout => Error.Timeout(typeUri, title),
            ErrorCategory.Conflict => Error.Conflict(typeUri, title),
            ErrorCategory.Failure => Error.Failure(typeUri, title),
            ErrorCategory.Critical => Error.Critical(typeUri, title),
            ErrorCategory.NotImplemented => Error.NotImplemented(typeUri, title),
            ErrorCategory.Unavailable => Error.Unavailable(typeUri, title),
            _ => throw new NotSupportedException($"Unsupported ErrorCategory: {category}")
        };
    }

    private static ActionResult MapSuccess(ControllerBase controller)
    {
        return controller.StatusCode(StatusCodes.Status202Accepted, new TestValue(31, "Mapped success"));
    }

    private static ActionResult MapSuccessValue(TestValue? value, ControllerBase controller)
    {
        return value is null
            ? controller.StatusCode(StatusCodes.Status205ResetContent)
            : (ActionResult)controller.StatusCode(
                StatusCodes.Status203NonAuthoritative, new TestValue(value.Id * 2, value.Name));
    }

    private static ActionResult? MapFailureToPaymentRequired(Error error, ControllerBase controller)
    {
        return error.Category == ErrorCategory.Failure
            ? controller.StatusCode(StatusCodes.Status402PaymentRequired, new TestValue(11, error.Title))
            : null;
    }

    private static ActionResult? MapFailureToConflict(Error error, ControllerBase controller)
    {
        return error.Category == ErrorCategory.Failure
            ? controller.StatusCode(StatusCodes.Status409Conflict, new TestValue(33, error.Title))
            : null;
    }

    private static ActionResult? MapConflictToPaymentRequired(Error error, ControllerBase controller)
    {
        return error.Category == ErrorCategory.Conflict
            ? controller.StatusCode(StatusCodes.Status402PaymentRequired, new TestValue(22, error.Title))
            : null;
    }

    #endregion
}
