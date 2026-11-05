using Maple.Result.Extensions.Functions.Worker.AspNetCore.Tests.Functional.TestingInfrastructure.Fixtures;
using Maple.Result.Extensions.Functions.Worker.AspNetCore.Tests.Functional.TestingInfrastructure.Helpers;
using System.Net;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Maple.Result.Extensions.Functions.Worker.AspNetCore.Tests.Functional;

public class ActionResultExtensionsFunctionalTests
    : IClassFixture<TestApplicationFixture>,
        IClassFixture<CustomizedProblemDetailsApplicationFixture>,
        IClassFixture<CustomizedStatusApplicationFixture>
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

    private const string ExpectedValueJson =
        """
        {
          "id": 13,
          "name": "Test value"
        }
        """;

    #endregion

    #region read-only fields

    private readonly HttpClient _sut;
    private readonly HttpClient _sutWithCustomizedProblemDetails;
    private readonly HttpClient _sutWithCustomizedStatus;

    #endregion

    #region constructors

    public ActionResultExtensionsFunctionalTests(
        TestApplicationFixture fixture,
        CustomizedProblemDetailsApplicationFixture customizedFixture,
        CustomizedStatusApplicationFixture customizedStatusFixture)
    {
        _sut = fixture.Client;
        _sutWithCustomizedProblemDetails = customizedFixture.Client;
        _sutWithCustomizedStatus = customizedStatusFixture.Client;
    }

    #endregion

    #region Result

    [Fact]
    public async Task ToActionResult_SuccessfulResult_ReturnsNoContent()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "success");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.NoContent);
        json.ShouldBeEmpty();
    }

    [Fact]
    public async Task ToActionResult_SuccessStatusCode_ReturnsTheStatusCode()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "status-code");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.Accepted);
        json.ShouldBeEmpty();
    }

    [Fact]
    public async Task ToActionResult_SuccessMapping_ReturnsTheMappedResponse()
    {
        // Arrange
        const string ExpectedJson =
            """
            {
              "id": 31,
              "name": "Mapped success"
            }
            """;

        // Act
        var (statusCode, json) = await GetAsync(_sut, "success-mapping");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.Accepted);
        json.ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    [Fact]
    public async Task ToActionResult_Error_ReturnsProblemDetails()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "error");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        json.ShouldBe(JsonHelper.Normalize(ExpectedFailureJson));
    }

    [Fact]
    public async Task ToActionResult_Error_ReturnsProblemJsonContentTypeAndTraceId()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        using var response = await _sut.GetAsync("error", cancellationToken);

        // Assert
        response.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe("application/problem+json");

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        JsonNode.Parse(json).ShouldNotBeNull()["traceId"].ShouldNotBeNull();
    }

    [Fact]
    public async Task ToActionResult_ErrorWithTemplatedDetail_ReturnsTheDetailTemplatedExtension()
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
                  "key1": "value1"
                }
              }
            }
            """;

        // Act
        var (statusCode, json) = await GetAsync(_sut, "error/templated");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        json.ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    [Fact]
    public async Task ToActionResult_ErrorWithMappingPassedToTheMethod_ReturnsTheMappedResponse()
    {
        // Arrange
        const string ExpectedJson =
            """
            {
              "id": 11,
              "name": "Failure title"
            }
            """;

        // Act
        var (statusCode, json) = await GetAsync(_sut, "error/custom-mapping");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.PaymentRequired);
        json.ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    [Fact]
    public async Task ToActionResult_ErrorWithCustomizedProblemDetails_AppliesTheCustomization()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        using var response = await _sutWithCustomizedProblemDetails.GetAsync("error", cancellationToken);

        // Assert
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var customized = JsonNode.Parse(json).ShouldNotBeNull()[CustomizedProblemDetailsApplicationFixture.ExtensionName];
        customized.ShouldNotBeNull().GetValue<bool>().ShouldBeTrue();
    }

    [Fact]
    public async Task ToActionResult_ErrorWithDetails_ReturnsTheErrorsExtension()
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
                  "pointer": "#/age",
                  "detail": "must be a positive integer",
                  "detailTemplated": {
                    "templateId": "errors.age.positive",
                    "params": { "min": 0 }
                  }
                },
                {
                  "pointer": "#/name",
                  "detail": "must not be empty"
                }
              ]
            }
            """;

        // Act
        var (statusCode, json) = await GetAsync(_sut, "error/details");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.BadRequest);
        json.ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    [Fact]
    public async Task ToActionResult_ErrorWithCustomizedStatus_ReturnsTheSameStatusInTheResponseAndTheBody()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        using var response = await _sutWithCustomizedStatus.GetAsync("error", cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        JsonNode.Parse(json).ShouldNotBeNull()["status"].ShouldNotBeNull().GetValue<int>().ShouldBe(400);
    }

    [Fact]
    public async Task ToActionResult_ErrorResultExecutedManyTimes_ReturnsTheTraceIdOfEachRequest()
    {
        // Act
        var firstTraceId = await GetTraceIdAsync(_sut, "error/cached");
        var secondTraceId = await GetTraceIdAsync(_sut, "error/cached");

        // Assert
        secondTraceId.ShouldNotBe(firstTraceId);
    }

    [Fact]
    public async Task ToActionResult_ErrorResultWithReplacedValue_ReturnsTheReplacedValue()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "error/replaced-value");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        json.ShouldBe(JsonHelper.Normalize(ExpectedValueJson));
    }

    [Fact]
    public async Task ToActionResult_ErrorResultWithChangedStatusCodeAndContentType_AppliesTheChanges()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        using var response = await _sut.GetAsync("error/changed-result", cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        response.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe("application/json");

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        JsonNode.Parse(json).ShouldNotBeNull()["status"].ShouldNotBeNull().GetValue<int>().ShouldBe(409);
    }

    #endregion

    #region Result<T>

    [Fact]
    public async Task ToActionResult_SuccessfulResultWithValue_ReturnsOkWithTheValue()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "value");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.OK);
        json.ShouldBe(JsonHelper.Normalize(ExpectedValueJson));
    }

    [Fact]
    public async Task ToActionResult_SuccessfulResultWithNullValue_ReturnsNoContent()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "value/null");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.NoContent);
        json.ShouldBeEmpty();
    }

    [Fact]
    public async Task ToActionResult_ValueWithSuccessStatusCode_ReturnsTheStatusCodeWithTheValue()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "value/status-code");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.Created);
        json.ShouldBe(JsonHelper.Normalize(ExpectedValueJson));
    }

    [Fact]
    public async Task ToActionResult_ValueWithSuccessMapping_ReturnsTheMappedResponse()
    {
        // Arrange
        const string ExpectedJson =
            """
            {
              "id": 26,
              "name": "Test value"
            }
            """;

        // Act
        var (statusCode, json) = await GetAsync(_sut, "value/success-mapping");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.NonAuthoritativeInformation);
        json.ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    [Fact]
    public async Task ToActionResult_GenericResultWithError_ReturnsProblemDetails()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "value/error");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        json.ShouldBe(JsonHelper.Normalize(ExpectedFailureJson));
    }

    #endregion

    #region helper methods

    private static async Task<string> GetTraceIdAsync(HttpClient client, string route)
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        using var response = await client.GetAsync(route, cancellationToken);

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonNode.Parse(json).ShouldNotBeNull()["traceId"].ShouldNotBeNull().GetValue<string>();
    }

    private static async Task<(HttpStatusCode StatusCode, string Json)> GetAsync(HttpClient client, string route)
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        using var response = await client.GetAsync(route, cancellationToken);

        var json = await JsonHelper.ReadNormalizedJsonAsync(response, cancellationToken);

        return (response.StatusCode, json);
    }

    #endregion
}
