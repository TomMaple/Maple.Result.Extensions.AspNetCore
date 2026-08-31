using Maple.Result.Extensions.AspNetCore.Tests.Functional.TestingInfrastructure.Fixtures;
using Maple.Result.Extensions.AspNetCore.Tests.Functional.TestingInfrastructure.Helpers;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Maple.Result.Extensions.AspNetCore.Tests.Functional;

public class ResultExtensionsFunctionalTests
    : IClassFixture<TestApplicationFixture>, IClassFixture<CustomMappingApplicationFixture>
{
    #region consts

    private const string ExpectedFailureJson =
        """
        {
          "type": "tag:test.com,2026:failure",
          "title": "Failure title",
          "status": 422,
          "detail": "Failure detail.",
          "instance": "https://test.com/instances/failure"
        }
        """;

    private const string ExpectedMappedSuccessJson =
        """
        {
          "id": 31,
          "name": "Mapped success"
        }
        """;

    private const string ExpectedMappedValueJson =
        """
        {
          "id": 26,
          "name": "Test value"
        }
        """;

    private const string ExpectedFailureMappingJson =
        """
        {
          "id": 11,
          "name": "Failure title"
        }
        """;

    #endregion

    #region read-only fields

    private readonly HttpClient _sut;
    private readonly HttpClient _sutWithCustomMapping;

    #endregion

    #region constructors

    public ResultExtensionsFunctionalTests(
        TestApplicationFixture fixture,
        CustomMappingApplicationFixture customMappingFixture)
    {
        _sut = fixture.Client;
        _sutWithCustomMapping = customMappingFixture.Client;
    }

    #endregion

    #region success

    [Fact]
    public async Task ToActionResult_SuccessfulResult_ReturnsNoContent()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/success");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.NoContent);
        json.ShouldBeEmpty();
    }

    [Fact]
    public async Task ToActionResult_SuccessfulResultWithValue_ReturnsOkWithValue()
    {
        // Arrange
        const string ExpectedJson =
            """
            {
              "id": 13,
              "name": "Test value"
            }
            """;

        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/success/value");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.OK);
        json.ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    [Fact]
    public async Task ToActionResult_SuccessfulResultWithNullValue_ReturnsNoContent()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/success/null-value");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.NoContent);
        json.ShouldBeEmpty();
    }

    #endregion

    #region ErrorCategory.Validation

    [Fact]
    public async Task ToActionResult_ValidationError_ReturnsBadRequestProblemDetails()
    {
        // Arrange
        const string ExpectedJson =
            """
            {
              "type": "tag:test.com,2026:validation",
              "title": "Validation title",
              "status": 400,
              "detail": "Validation detail.",
              "instance": "https://test.com/instances/validation"
            }
            """;

        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/validation");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.BadRequest);
        json.ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    [Fact]
    public async Task ToActionResult_ValidationErrorWithDetailVariants_ReturnsProblemDetailsWithErrorsExtension()
    {
        // Arrange
        const string ExpectedJson =
            """
            {
              "type": "tag:test.com,2026:validation",
              "title": "Validation title",
              "status": 400,
              "detail": "Validation detail.",
              "instance": "https://test.com/instances/validation",
              "errors": [
                {
                  "pointer": "/age",
                  "detail": "must be a positive integer",
                  "detailTemplated": {
                    "templateId": "test.age.mustBePositive",
                    "params": { "min": 0 }
                  }
                },
                {
                  "detail": "must be provided"
                },
                {
                  "pointer": "/name",
                  "detail": "must not be empty",
                  "detailTemplated": { "templateId": "test.name.required" }
                }
              ]
            }
            """;

        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/validation/details");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.BadRequest);
        json.ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    #endregion

    #region ErrorCategory.Unauthenticated

    [Fact]
    public async Task ToActionResult_UnauthenticatedError_ReturnsUnauthorizedProblemDetails()
    {
        // Arrange
        const string ExpectedJson =
            """
            {
              "type": "tag:test.com,2026:unauthenticated",
              "title": "Unauthenticated title",
              "status": 401,
              "detail": "Unauthenticated detail.",
              "instance": "https://test.com/instances/unauthenticated"
            }
            """;

        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/unauthenticated");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.Unauthorized);
        json.ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    #endregion

    #region ErrorCategory.Unauthorized

    [Fact]
    public async Task ToActionResult_UnauthorizedError_ReturnsForbiddenProblemDetails()
    {
        // Arrange
        const string ExpectedJson =
            """
            {
              "type": "tag:test.com,2026:unauthorized",
              "title": "Unauthorized title",
              "status": 403,
              "detail": "Unauthorized detail.",
              "instance": "https://test.com/instances/unauthorized"
            }
            """;

        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/unauthorized");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.Forbidden);
        json.ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    #endregion

    #region ErrorCategory.NotFound

    [Fact]
    public async Task ToActionResult_NotFoundError_ReturnsNotFoundProblemDetails()
    {
        // Arrange
        const string ExpectedJson =
            """
            {
              "type": "tag:test.com,2026:not-found",
              "title": "Not found title",
              "status": 404,
              "detail": "Not found detail.",
              "instance": "https://test.com/instances/not-found"
            }
            """;

        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/not-found");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.NotFound);
        json.ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    #endregion

    #region ErrorCategory.Timeout

    [Fact]
    public async Task ToActionResult_TimeoutError_ReturnsRequestTimeoutProblemDetails()
    {
        // Arrange
        const string ExpectedJson =
            """
            {
              "type": "tag:test.com,2026:timeout",
              "title": "Timeout title",
              "status": 408,
              "detail": "Timeout detail.",
              "instance": "https://test.com/instances/timeout"
            }
            """;

        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/timeout");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.RequestTimeout);
        json.ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    #endregion

    #region ErrorCategory.Conflict

    [Fact]
    public async Task ToActionResult_ConflictError_ReturnsConflictProblemDetails()
    {
        // Arrange
        const string ExpectedJson =
            """
            {
              "type": "tag:test.com,2026:conflict",
              "title": "Conflict title",
              "status": 409,
              "detail": "Conflict detail.",
              "instance": "https://test.com/instances/conflict"
            }
            """;

        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/conflict");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.Conflict);
        json.ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    #endregion

    #region ErrorCategory.Critical

    [Fact]
    public async Task ToActionResult_CriticalError_ReturnsInternalServerErrorProblemDetails()
    {
        // Arrange
        const string ExpectedJson =
            """
            {
              "type": "tag:test.com,2026:critical",
              "title": "Critical title",
              "status": 500,
              "detail": "Critical detail.",
              "instance": "https://test.com/instances/critical"
            }
            """;

        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/critical");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.InternalServerError);
        json.ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    #endregion

    #region ErrorCategory.NotImplemented

    [Fact]
    public async Task ToActionResult_NotImplementedError_ReturnsNotImplementedProblemDetails()
    {
        // Arrange
        const string ExpectedJson =
            """
            {
              "type": "tag:test.com,2026:not-implemented",
              "title": "Not implemented title",
              "status": 501,
              "detail": "Not implemented detail.",
              "instance": "https://test.com/instances/not-implemented"
            }
            """;

        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/not-implemented");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.NotImplemented);
        json.ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    #endregion

    #region ErrorCategory.Unavailable

    [Fact]
    public async Task ToActionResult_UnavailableError_ReturnsServiceUnavailableProblemDetails()
    {
        // Arrange
        const string ExpectedJson =
            """
            {
              "type": "tag:test.com,2026:unavailable",
              "title": "Unavailable title",
              "status": 503,
              "detail": "Unavailable detail.",
              "instance": "https://test.com/instances/unavailable"
            }
            """;

        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/unavailable");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        json.ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    #endregion

    #region ErrorCategory.Failure

    [Fact]
    public async Task ToActionResult_FailureErrorWithAllProperties_ReturnsUnprocessableEntityProblemDetails()
    {
        // Arrange
        const string ExpectedJson =
            """
            {
              "type": "tag:test.com,2026:failure",
              "title": "Failure title",
              "status": 422,
              "detail": "Failure detail.",
              "instance": "https://test.com/instances/failure",
              "errors": [
                {
                  "pointer": "#/property1",
                  "detail": "Property 1 failure detail.",
                  "detailTemplated": {
                    "templateId": "errors.failure.property1",
                    "params": {
                      "pk1": "pv1"
                    }
                  }
                },
                {
                  "pointer": "#/property2",
                  "detail": "Property 2 failure detail."
                }
              ],
              "detailTemplated": {
                "templateId": "errors.failure.detail",
                "params": {
                  "key1": "value1",
                  "key2": 123
                }
              }
            }
            """;

        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/failure/all-properties");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        json.ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    [Fact]
    public async Task ToActionResult_FailureErrorWithRequiredPropertiesOnly_ReturnsUnprocessableEntityProblemDetailsWithoutOptionalProperties()
    {
        // Arrange
        const string ExpectedJson =
            """
            {
              "type": "tag:test.com,2026:failure",
              "title": "Failure title",
              "status": 422
            }
            """;

        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/failure/required-only");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        json.ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    [Fact]
    public async Task ToActionResult_FailureErrorWithDetailAndInstance_ReturnsUnprocessableEntityProblemDetailsWithoutExtensions()
    {
        // Arrange
        const string ExpectedJson =
            """
            {
              "type": "tag:test.com,2026:failure",
              "title": "Failure title",
              "status": 422,
              "detail": "Failure detail.",
              "instance": "https://test.com/instances/failure"
            }
            """;

        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/failure/detail-and-instance");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        json.ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    [Fact]
    public async Task ToActionResult_FailureErrorWithTemplatedDetail_ReturnsUnprocessableEntityProblemDetailsWithDetailTemplated()
    {
        // Arrange
        const string ExpectedJson =
            """
            {
              "type": "tag:test.com,2026:failure",
              "title": "Failure title",
              "status": 422,
              "detail": "Failure detail.",
              "instance": "https://test.com/instances/failure",
              "detailTemplated": {
                "templateId": "errors.failure.detail",
                "params": {
                  "key1": "value1",
                  "key2": 123
                }
              }
            }
            """;

        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/failure/templated-detail");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        json.ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    #endregion

    #region ErrorCategory.Failure, ToActionResult overloads

    [Fact]
    public async Task ToActionResult_FailureErrorFromResultWithControllerArgument_ReturnsUnprocessableEntityProblemDetails()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/failure/overloads/result-controller");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        json.ShouldBe(JsonHelper.Normalize(ExpectedFailureJson));
    }

    [Fact]
    public async Task ToActionResult_FailureErrorFromControllerWithResultArgument_ReturnsUnprocessableEntityProblemDetails()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/failure/overloads/controller-result");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        json.ShouldBe(JsonHelper.Normalize(ExpectedFailureJson));
    }

    [Fact]
    public async Task ToActionResult_FailureErrorFromGenericResultWithControllerArgument_ReturnsUnprocessableEntityProblemDetails()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/failure/overloads/generic-result-controller");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        json.ShouldBe(JsonHelper.Normalize(ExpectedFailureJson));
    }

    [Fact]
    public async Task ToActionResult_FailureErrorFromControllerWithGenericResultArgument_ReturnsUnprocessableEntityProblemDetails()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/failure/overloads/generic-controller-result");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        json.ShouldBe(JsonHelper.Normalize(ExpectedFailureJson));
    }

    #endregion

    #region custom mappings

    [Fact]
    public async Task ToActionResult_ErrorMatchingCustomMapping_ReturnsCustomMappedResponse()
    {
        // Arrange
        const string ExpectedJson =
            """
            {
              "id": 99,
              "name": "Conflict title"
            }
            """;

        // Act
        var (statusCode, json) = await GetAsync(_sutWithCustomMapping, "results/conflict");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.Gone);
        json.ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    [Fact]
    public async Task ToActionResult_ErrorMatchingCustomMappingByTypeUri_ReturnsCustomMappedResponse()
    {
        // Arrange
        const string ExpectedJson =
            """
            {
              "id": 42,
              "name": "tag:test.com,2026:validation"
            }
            """;

        // Act
        var (statusCode, json) = await GetAsync(_sutWithCustomMapping, "results/validation");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        json.ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    [Fact]
    public async Task ToActionResult_ErrorMatchingTwoCustomMappings_ReturnsResponseOfTheFirstMatchingMapping()
    {
        // Arrange
        const string ExpectedJson =
            """
            {
              "id": 1,
              "name": "First matching mapping"
            }
            """;

        // Act
        var (statusCode, json) = await GetAsync(_sutWithCustomMapping, "results/timeout");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.UpgradeRequired);
        json.ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    [Fact]
    public async Task ToActionResult_ErrorMatchingCustomMappingUsingController_ReturnsCustomMappedResponseWithHeader()
    {
        // Arrange
        const string ExpectedHeader = "Not implemented title";
        const string ExpectedJson =
            """
            {
              "id": 7,
              "name": "Not implemented title"
            }
            """;

        // Act
        var (statusCode, json, header) = await GetWithHeaderAsync(
            _sutWithCustomMapping,
            "results/not-implemented",
            CustomMappingApplicationFixture.ErrorTitleHeaderName);

        // Assert
        statusCode.ShouldBe(HttpStatusCode.OK);
        header.ShouldBe(ExpectedHeader);
        json.ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    [Fact]
    public async Task ToActionResult_ErrorNotMatchingCustomMapping_ReturnsDefaultProblemDetails()
    {
        // Arrange
        const string ExpectedJson =
            """
            {
              "type": "tag:test.com,2026:not-found",
              "title": "Not found title",
              "status": 404,
              "detail": "Not found detail.",
              "instance": "https://test.com/instances/not-found"
            }
            """;

        // Act
        var (statusCode, json) = await GetAsync(_sutWithCustomMapping, "results/not-found");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.NotFound);
        json.ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    #endregion

    #region custom mappings passed to the method

    [Fact]
    public async Task ToActionResult_ErrorMatchingMappingPassedToTheMethod_ReturnsCustomMappedResponse()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/per-call/matching");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.PaymentRequired);
        json.ShouldBe(JsonHelper.Normalize(ExpectedFailureMappingJson));
    }

    [Fact]
    public async Task ToActionResult_ErrorMatchingMappingPassedToTheMethodFromController_ReturnsCustomMappedResponse()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/per-call/controller-result");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.PaymentRequired);
        json.ShouldBe(JsonHelper.Normalize(ExpectedFailureMappingJson));
    }

    [Fact]
    public async Task ToActionResult_ErrorMatchingMappingPassedToTheMethodFromGenericResult_ReturnsCustomMappedResponse()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/per-call/generic-result-controller");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.PaymentRequired);
        json.ShouldBe(JsonHelper.Normalize(ExpectedFailureMappingJson));
    }

    [Fact]
    public async Task ToActionResult_ErrorMatchingMappingPassedToTheMethodFromControllerWithGenericResult_ReturnsCustomMappedResponse()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/per-call/generic-controller-result");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.PaymentRequired);
        json.ShouldBe(JsonHelper.Normalize(ExpectedFailureMappingJson));
    }

    [Fact]
    public async Task ToActionResult_ErrorNotMatchingMappingPassedToTheMethod_ReturnsDefaultProblemDetails()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/per-call/not-matching");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        json.ShouldBe(JsonHelper.Normalize(ExpectedFailureJson));
    }

    [Fact]
    public async Task ToActionResult_ErrorMatchingMappingPassedToTheMethodAndRegisteredMapping_ReturnsResponseOfTheMappingPassedToTheMethod()
    {
        // Arrange
        const string ExpectedJson =
            """
            {
              "id": 22,
              "name": "Conflict title"
            }
            """;

        // Act
        var (statusCode, json) = await GetAsync(_sutWithCustomMapping, "results/per-call/precedence");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.PaymentRequired);
        json.ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    #endregion

    #region success status code

    [Fact]
    public async Task ToActionResult_SuccessfulResultWithSuccessStatusCode_ReturnsGivenStatusCode()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/status-code/success");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.Accepted);
        json.ShouldBeEmpty();
    }

    [Fact]
    public async Task ToActionResult_SuccessfulResultWithSuccessStatusCodeFromController_ReturnsGivenStatusCode()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/status-code/controller-result");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.MultiStatus);
        json.ShouldBeEmpty();
    }

    [Fact]
    public async Task ToActionResult_SuccessfulResultWithValueAndSuccessStatusCode_ReturnsGivenStatusCodeWithValue()
    {
        // Arrange
        const string ExpectedJson =
            """
            {
              "id": 13,
              "name": "Test value"
            }
            """;

        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/status-code/success/value");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.Created);
        json.ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    [Fact]
    public async Task ToActionResult_SuccessfulResultWithValueAndSuccessStatusCodeFromController_ReturnsGivenStatusCodeWithValue()
    {
        // Arrange
        const string ExpectedJson =
            """
            {
              "id": 13,
              "name": "Test value"
            }
            """;

        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/status-code/generic-controller-result");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.NonAuthoritativeInformation);
        json.ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    [Fact]
    public async Task ToActionResult_SuccessfulResultWithNullValueAndNoNullValueStatusCode_ReturnsSuccessStatusCode()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/status-code/success/null-value");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.IMUsed);
        json.ShouldBeEmpty();
    }

    [Fact]
    public async Task ToActionResult_SuccessfulResultWithNullValueAndNullValueStatusCode_ReturnsNullValueStatusCode()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/status-code/success/null-value-status-code");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.ResetContent);
        json.ShouldBeEmpty();
    }

    [Fact]
    public async Task ToActionResult_ErrorWithSuccessStatusCode_ReturnsDefaultProblemDetails()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/status-code/error");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        json.ShouldBe(JsonHelper.Normalize(ExpectedFailureJson));
    }

    [Fact]
    public async Task ToActionResult_ErrorWithSuccessStatusCodeAndMappingPassedToTheMethod_ReturnsCustomMappedResponse()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/status-code/error/custom-mapping");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.PaymentRequired);
        json.ShouldBe(JsonHelper.Normalize(ExpectedFailureMappingJson));
    }

    #endregion

    #region success mapping

    [Fact]
    public async Task ToActionResult_SuccessfulResultWithSuccessMapping_ReturnsMappedResponse()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/success-mapping/success");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.Accepted);
        json.ShouldBe(JsonHelper.Normalize(ExpectedMappedSuccessJson));
    }

    [Fact]
    public async Task ToActionResult_SuccessfulResultWithSuccessMappingFromController_ReturnsMappedResponse()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/success-mapping/controller-result");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.Accepted);
        json.ShouldBe(JsonHelper.Normalize(ExpectedMappedSuccessJson));
    }

    [Fact]
    public async Task ToActionResult_SuccessfulResultWithValueAndSuccessMapping_ReturnsMappedValue()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/success-mapping/success/value");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.NonAuthoritativeInformation);
        json.ShouldBe(JsonHelper.Normalize(ExpectedMappedValueJson));
    }

    [Fact]
    public async Task ToActionResult_SuccessfulResultWithValueAndSuccessMappingFromController_ReturnsMappedValue()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/success-mapping/generic-controller-result");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.NonAuthoritativeInformation);
        json.ShouldBe(JsonHelper.Normalize(ExpectedMappedValueJson));
    }

    [Fact]
    public async Task ToActionResult_SuccessfulResultWithNullValueAndSuccessMapping_ReturnsMappedNullValueResponse()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/success-mapping/success/null-value");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.ResetContent);
        json.ShouldBeEmpty();
    }

    [Fact]
    public async Task ToActionResult_ErrorWithSuccessMapping_ReturnsDefaultProblemDetails()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/success-mapping/error");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        json.ShouldBe(JsonHelper.Normalize(ExpectedFailureJson));
    }

    [Fact]
    public async Task ToActionResult_ErrorWithSuccessMappingAndMappingPassedToTheMethod_ReturnsCustomMappedResponse()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "results/success-mapping/error/custom-mapping");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.PaymentRequired);
        json.ShouldBe(JsonHelper.Normalize(ExpectedFailureMappingJson));
    }

    #endregion

    #region helper methods

    private static async Task<(HttpStatusCode StatusCode, string Json)> GetAsync(HttpClient client, string route)
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        using var response = await client.GetAsync(route, cancellationToken);

        var json = await JsonHelper.ReadNormalizedJsonAsync(response, cancellationToken);

        return (response.StatusCode, json);
    }

    private static async Task<(HttpStatusCode StatusCode, string Json, string? Header)> GetWithHeaderAsync(
        HttpClient client,
        string route,
        string headerName)
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        using var response = await client.GetAsync(route, cancellationToken);

        var json = await JsonHelper.ReadNormalizedJsonAsync(response, cancellationToken);
        var header = response.Headers.TryGetValues(headerName, out var values)
            ? string.Join(",", values)
            : null;

        return (response.StatusCode, json, header);
    }

    #endregion
}
